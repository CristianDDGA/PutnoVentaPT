using ClosedXML.Excel;
using PuntoVenta.Application.DTOs.Product;
using PuntoVenta.Application.DTOs.Sale;
using PuntoVenta.Application.Interfaces.Services;

namespace PuntoVenta.Infrastructure.Services;

public class ExcelExportService : IExcelExportService
{
    public byte[] ExportSales(IEnumerable<SaleDto> sales)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Ventas");

        var headers = new[]
        {
            "Nº Factura", "Cliente", "Fecha", "Vendedor", "Subtotal", "IVA", "Total", "Estado", "Pago"
        };

        for (var col = 0; col < headers.Length; col++)
            worksheet.Cell(1, col + 1).Value = headers[col];

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F46E5");
        headerRow.Style.Font.FontColor = XLColor.White;

        var row = 2;
        foreach (var sale in sales)
        {
            worksheet.Cell(row, 1).Value = sale.SaleId;
            worksheet.Cell(row, 2).Value = sale.CustomerName;
            worksheet.Cell(row, 3).Value = sale.SaleDate;
            worksheet.Cell(row, 4).Value = sale.SellerName;
            worksheet.Cell(row, 5).Value = sale.Subtotal;
            worksheet.Cell(row, 6).Value = sale.TaxAmount;
            worksheet.Cell(row, 7).Value = sale.Total;
            worksheet.Cell(row, 8).Value = sale.Status;
            worksheet.Cell(row, 9).Value = sale.PaymentType;
            row++;
        }

        worksheet.Column(3).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
        worksheet.Columns(5, 7).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportProducts(IEnumerable<ProductDto> products)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Productos");

        var headers = new[]
        {
            "ID", "Nombre", "Precio", "Stock", "Estado", "Modificado Por", "Última Modificación"
        };

        for (var col = 0; col < headers.Length; col++)
            worksheet.Cell(1, col + 1).Value = headers[col];

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F46E5");
        headerRow.Style.Font.FontColor = XLColor.White;

        var row = 2;
        foreach (var product in products)
        {
            worksheet.Cell(row, 1).Value = product.ProductId;
            worksheet.Cell(row, 2).Value = product.Name;
            worksheet.Cell(row, 3).Value = product.Price;
            worksheet.Cell(row, 4).Value = product.Stock;
            worksheet.Cell(row, 5).Value = product.IsActive ? "Activo" : "Inactivo";
            worksheet.Cell(row, 6).Value = product.LastModifiedBy ?? "-";
            worksheet.Cell(row, 7).Value = product.LastModifiedAt;
            row++;
        }

        worksheet.Column(3).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Column(7).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
