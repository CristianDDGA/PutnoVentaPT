using Bogus;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Domain.Entities;
using PuntoVenta.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PuntoVenta.Infrastructure.Persistence;

public class DataSeedingService
{
    private readonly AppDbContext _context;

    public DataSeedingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task GenerarDatosEstresAsync(int recordCount = 100)
    {
        _context.Database.SetCommandTimeout(300);

        var fakerTech = new Faker("en");
        var fakerEs = new Faker("es");

        // recordCount se recibe como parámetro (por defecto 100)

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. LIMPIEZA PREVIA
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [StockMovements]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [SaleDetails]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Sales]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Products]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Customers]");
            
            // Reiniciar los contadores de identidad (IDENTITY)
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('[StockMovements]', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('[SaleDetails]', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('[Sales]', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('[Products]', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('[Customers]', RESEED, 0)");

            // 2. GENERACIÓN E INSERCIÓN DE DATOS CON EF CORE
            var customers = new List<Customer>();
            for (int i = 0; i < recordCount; i++)
            {
                var customer = Customer.Create(
                    documentNumber: fakerEs.Random.ReplaceNumbers("18########"),
                    firstName: fakerEs.Name.FirstName(),
                    lastName: fakerEs.Name.LastName(),
                    phone: fakerEs.Phone.PhoneNumber("09########"),
                    address: fakerEs.Address.StreetAddress(),
                    city: "Ambato",
                    email: fakerEs.Internet.Email()
                );
                customers.Add(customer);
            }
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            var products = new List<Product>();
            var marcasTech = new[] { "Asus ROG", "MSI Pro", "Corsair", "Logitech G", "Razer", "Samsung Evo", "Kingston Fury", "Intel Core", "AMD Ryzen", "Sony", "Apple", "Dell UltraSharp", "Gigabyte" };
            var categoriesTech = new[] { "Gaming Laptop", "Mechanical Keyboard", "Wireless Mouse", "NVMe M.2 SSD", "Graphics Card RTX", "DDR5 RAM 16GB", "Curved Monitor", "Processor", "Liquid Cooling", "Headset 7.1" };

            for (int i = 0; i < recordCount; i++)
            {
                var marca = fakerTech.PickRandom(marcasTech);
                var categoria = fakerTech.PickRandom(categoriesTech);
                var modelo = fakerTech.Commerce.Product();
                var product = Product.Create(
                    name: $"{marca} {categoria} ({modelo})",
                    price: Math.Round(fakerTech.Random.Decimal(15.00m, 1499.00m), 2),
                    stock: fakerTech.Random.Number(5, 300)
                );
                products.Add(product);
            }
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            var sales = new List<Sale>();
            for (int i = 0; i < recordCount; i++)
            {
                var randomCustomer = fakerEs.PickRandom(customers);
                
                // Seleccionar de 1 a 3 productos aleatorios para el detalle
                int itemsEnVenta = fakerEs.Random.Number(1, 3);
                var selectedProducts = fakerEs.PickRandom(products, itemsEnVenta).ToList();

                var saleDetails = new List<SaleDetail>();
                foreach (var prod in selectedProducts)
                {
                    int cantidad = fakerEs.Random.Number(1, 2);
                    saleDetails.Add(SaleDetail.Create(
                        productId: prod.ProductId,
                        productName: prod.Name,
                        quantity: cantidad,
                        unitPrice: prod.Price
                    ));
                }

                var sale = Sale.Create(
                    customerId: randomCustomer.CustomerId,
                    customerDocument: randomCustomer.DocumentNumber,
                    customerName: randomCustomer.FullName,
                    paymentType: PaymentType.Cash,
                    details: saleDetails,
                    userId: null,
                    sellerName: null
                );

                // Como la venta es histórica, la confirmamos directamente y ajustamos la fecha
                sale.ConfirmSale();
                // No llamar a UpdateDraft después de ConfirmSale (provoca excepción). Los detalles ya se asignaron en Create.
                
                sales.Add(sale);
            }
            await _context.Sales.AddRangeAsync(sales);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}