using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> productBuilder)
    {
        productBuilder.ToTable("Products");

        productBuilder.HasKey(product => product.ProductId);

        productBuilder.Property(product => product.Name)
            .IsRequired()
            .HasMaxLength(150);

        // Ajustado para compatibilidad nativa con SQL Server (decimal en lugar de NUMBER)
        productBuilder.Property(product => product.Price)
            .HasColumnType("decimal(10,2)");

        productBuilder.Property(product => product.Stock)
            .IsRequired();

        productBuilder.Property(product => product.IsActive)
            .IsRequired();

        // 🚀 NUEVO: Agregamos los productos semilla usando objetos anónimos
        productBuilder.HasData(
            new
            {
                ProductId = 1,
                Name = "Teclado 1",
                Price = 1.50m,
                Stock = 50,
                IsActive = true
            },
            new
            {
                ProductId = 2,
                Name = "Proyecto Epson",
                Price = 0.80m,
                Stock = 100,
                IsActive = true
            },
            new
            {
                ProductId = 3,
                Name = "Laptop HP ",
                Price = 1.20m,
                Stock = 30,
                IsActive = true
            }
        );
    }
}