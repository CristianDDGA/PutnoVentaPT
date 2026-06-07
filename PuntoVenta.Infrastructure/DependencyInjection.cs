using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PuntoVenta.Application.Interfaces.Repositories;
using PuntoVenta.Application.Interfaces.Services;
using PuntoVenta.Application.Services;
using PuntoVenta.Infrastructure.Persistence;
using PuntoVenta.Infrastructure.Repositories;
using PuntoVenta.Infrastructure.Services;

namespace PuntoVenta.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. CAMBIO AQUÍ: Cambiamos UseOracle por UseSqlServer
        services.AddDbContext<AppDbContext>(dbContextOptions =>
            dbContextOptions.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))); // Esto asegura que encuentre las migraciones aquí

        // Repositorios (Se quedan exactamente igual)
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Servicios de Application (Se quedan exactamente igual)
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IErrorLogService, ErrorLogService>();

        // Servicios de Infrastructure (Se quedan exactamente igual)
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<SecuritySeedService>();

        return services;
    }
}