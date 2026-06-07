using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using PuntoVenta.Blazor;
using PuntoVenta.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// URL base de la API
builder.Services.AddScoped(serviceProvider => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5291/")
});

builder.Services.AddAuthorizationCore();

// Registrar servicios
builder.Services.AddScoped<AuthSessionService>();
builder.Services.AddScoped<AuthenticationStateProvider, BlazorAuthenticationStateProvider>();
builder.Services.AddScoped<CustomerApiService>();
builder.Services.AddScoped<ProductApiService>();
builder.Services.AddScoped<SaleApiService>();
builder.Services.AddScoped<DashboardApiService>();
builder.Services.AddScoped<UserApiService>();
builder.Services.AddScoped<ErrorLogApiService>();
builder.Services.AddScoped<UserModeService>();

await builder.Build().RunAsync();