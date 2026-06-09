using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi; // 🌟 RESTAURADO: El espacio de nombres correcto que tu proyecto reconoce
using PuntoVenta.Infrastructure;
using PuntoVenta.Infrastructure.Persistence;
using PuntoVenta.Infrastructure.Services;
using System;
using System.Text;
using System.Linq;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// If we will fallback to SQLite for local development (SKIP_MIGRATIONS),
// ensure the SQLite provider is initialized to avoid the "You need to call
// SQLitePCL.raw.SetProvider()" runtime error.
var _skipMigrationsEnvForSqlite = Environment.GetEnvironmentVariable("SKIP_MIGRATIONS");
var _useSqliteFallback = !string.IsNullOrEmpty(_skipMigrationsEnvForSqlite) &&
                         (_skipMigrationsEnvForSqlite.Equals("1") || _skipMigrationsEnvForSqlite.Equals("true", StringComparison.OrdinalIgnoreCase));
if (_useSqliteFallback)
{
    try
    {
        // Try to initialize SQLite provider via reflection if the SQLitePCL bundle
        // is available at runtime. This avoids a compile-time dependency on the
        // SQLitePCL package.
        var batteriesType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType("SQLitePCL.Batteries", false, false))
            .FirstOrDefault(t => t != null);

        if (batteriesType != null)
        {
            var initMethod = batteriesType.GetMethod("Init", BindingFlags.Public | BindingFlags.Static);
            initMethod?.Invoke(null, null);
            builder.Logging.Services.BuildServiceProvider().GetService<ILoggerFactory>()?.CreateLogger("Program")?.LogInformation("SQLite provider initialized via reflection.");
        }
    }
    catch
    {
        // Ignore - if the provider bundle isn't present, we'll surface a clearer error later.
    }
}

// ====================================================================
// 1. INYECCIÓN DE DEPENDENCIAS Y CAPAS (PIPELINE)
// ====================================================================
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<PuntoVenta.Application.Validators.CreateSaleValidator>();
// Añadimos un filtro global AllowAnonymous en desarrollo para evitar bloqueos por [Authorize]
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AllowAnonymousFilter());
});

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

// En entorno de desarrollo permitimos peticiones anónimas para probar el front.
// Esto evita que la API devuelva 401 si la DB/auth no está disponible.
builder.Services.AddAuthorization(options =>
{
    // Permitir acceso en desarrollo: tanto DefaultPolicy como FallbackPolicy son permisivas.
    var permissive = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAssertion(_ => true)
        .Build();

    options.DefaultPolicy = permissive;
    options.FallbackPolicy = permissive;
});
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
        corsPolicy.WithOrigins("http://localhost:5169", "https://localhost:7177","https://localhost:5170")
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
var skipMigrationsEnv = System.Environment.GetEnvironmentVariable("SKIP_MIGRATIONS");
var skipMigrations = !string.IsNullOrEmpty(skipMigrationsEnv) &&
                     (skipMigrationsEnv.Equals("1") || skipMigrationsEnv.Equals("true", StringComparison.OrdinalIgnoreCase));

if (!skipMigrations)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();

            var securitySeedService = scope.ServiceProvider.GetRequiredService<SecuritySeedService>();
            await securitySeedService.EnsureSeededAsync();

            // Ejecutar generación de datos de estrés (100 registros por tabla)
            // Esto persistirá los datos en la base de datos configurada en DefaultConnection
            var dataSeedingService = scope.ServiceProvider.GetRequiredService<DataSeedingService>();
            await dataSeedingService.GenerarDatosEstresAsync(100);

            // SQL Server maneja las identidades automáticamente después del seeding
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetService<ILogger<Program>>();
        logger?.LogWarning(ex, "No se pudo aplicar migraciones o seedear la base de datos. Se continúa sin aplicar migraciones.");
    }
}
else
{
    app.Logger.LogInformation("SKIP_MIGRATIONS is set; skipping database migrations and seeding.");
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