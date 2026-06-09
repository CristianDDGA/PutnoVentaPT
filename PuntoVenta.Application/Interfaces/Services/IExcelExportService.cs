using PuntoVenta.Application.DTOs.Product;
using PuntoVenta.Application.DTOs.Sale;

namespace PuntoVenta.Application.Interfaces.Services;

public interface IExcelExportService
{
    byte[] ExportSales(IEnumerable<SaleDto> sales);
    byte[] ExportProducts(IEnumerable<ProductDto> products);
}
