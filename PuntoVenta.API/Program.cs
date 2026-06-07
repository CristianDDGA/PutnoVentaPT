using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi; // 🌟 RESTAURADO: El espacio de nombres correcto que tu proyecto reconoce
using PuntoVenta.Infrastructure;
using PuntoVenta.Infrastructure.Persistence;
using PuntoVenta.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ====================================================================
// 1. INYECCIÓN DE DEPENDENCIAS Y CAPAS (PIPELINE)
// ====================================================================
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<PuntoVenta.Application.Validators.CreateSaleValidator>();
builder.Services.AddControllers();

// Configuración de la Autenticación con JWT Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSecret = builder.Configuration["Security:JwtSecret"] ?? "PuntoVenta_Dev_Secret_Key_2026_Must_Be_At_Least_32_Chars";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Security:Issuer"] ?? "PuntoVenta.API",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Security:Audience"] ?? "PuntoVenta.Client",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.Name
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

// Configuración de Swagger UI adaptada perfectamente a los tipos nativos de tu versión
builder.Services.AddSwaggerGen(options =>
{
    var bearerScheme = new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    options.AddSecurityDefinition("Bearer", bearerScheme);

    // 🌟 ENLACE CORRETO: Formato compatible con OpenApiSecuritySchemeReference para tu .NET
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", null, null)] = new List<string>()
    });
});

// Servicios de Semilla y Pruebas Masivas
builder.Services.AddScoped<DataSeedingService>();
builder.Services.AddScoped<SecuritySeedService>();

// Política de CORS para Blazor
builder.Services.AddCors(corsOptions =>
    corsOptions.AddPolicy("BlazorPolicy", corsPolicy =>
        corsPolicy.WithOrigins("http://localhost:5169", "https://localhost:7177")
                  .AllowAnyMethod()
                  .AllowAnyHeader()));

var app = builder.Build();

// ====================================================================
// 2. CONFIGURACIÓN DEL PIPELINE DE PETICIONES HTTP (MIDDLEWARES)
// ====================================================================

// 🌟 CAPTURADOR DE ERRORES GLOBAL: Monitorea todo el ciclo de vida desde el inicio
// para guardar fallos de seguridad o base de datos en la tabla ErrorLog.
app.UseMiddleware<PuntoVenta.API.Middleware.ExceptionMiddleware>();

// Migración y Semilla Automática al arrancar la API
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    var securitySeedService = scope.ServiceProvider.GetRequiredService<SecuritySeedService>();
    await securitySeedService.EnsureSeededAsync();

    // SQL Server maneja las identidades automáticamente después del seeding
}

// Entorno de Desarrollo para Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("BlazorPolicy");

// Orden de Seguridad Nativo de ASP.NET Core: Autenticación siempre va ANTES de Autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();