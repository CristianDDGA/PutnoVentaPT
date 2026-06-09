using ClosedXML.Excel;
using PuntoVenta.Application.DTOs.Product;
using PuntoVenta.Application.DTOs.Sale;
using PuntoVenta.Infrastructure.Services;

namespace PuntoVenta.Tests.Services;

public class ExcelExportServiceTests
{
    private readonly ExcelExportService _service = new();

    [Fact]
    public void ExportSales_ShouldProduceValidWorkbook()
    {
        var sales = new List<SaleDto>
        {
            new()
            {
                SaleId       = 1001,
                CustomerName = "Cliente Demo",
                SaleDate     = new DateTime(2026, 6, 9, 10, 30, 0),
                SellerName   = "Vendedor",
                Subtotal     = 100m,
                TaxAmount    = 12m,
                Total        = 112m,
                Status       = "Pagada",
                PaymentType  = "Efectivo"
            }
        };

        var bytes = _service.ExportSales(sales);

        Assert.NotEmpty(bytes);
        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("Ventas");

        Assert.Equal("Nº Factura", worksheet.Cell(1, 1).GetString());
        Assert.Equal(1001, worksheet.Cell(2, 1).GetValue<int>());
        Assert.Equal("Cliente Demo", worksheet.Cell(2, 2).GetString());
    }

    [Fact]
    public void ExportProducts_ShouldProduceValidWorkbook()
    {
        var products = new List<ProductDto>
        {
            new()
            {
                ProductId = 1,
                Name      = "Teclado",
                Price     = 45.50m,
                Stock     = 20,
                IsActive  = true
            }
        };

        var bytes = _service.ExportProducts(products);

        Assert.NotEmpty(bytes);
        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("Productos");

        Assert.Equal("ID", worksheet.Cell(1, 1).GetString());
        Assert.Equal("Teclado", worksheet.Cell(2, 2).GetString());
        Assert.Equal("Activo", worksheet.Cell(2, 5).GetString());
    }

    [Fact]
    public void ExportSales_EmptyList_ShouldProduceHeaderOnly()
    {
        var bytes = _service.ExportSales([]);

        Assert.NotEmpty(bytes);
        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("Ventas");

        Assert.Equal("Nº Factura", worksheet.Cell(1, 1).GetString());
        Assert.True(worksheet.Cell(2, 1).IsEmpty());
    }
}
