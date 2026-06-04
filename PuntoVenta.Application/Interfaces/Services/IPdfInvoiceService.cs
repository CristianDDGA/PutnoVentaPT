using PuntoVenta.Application.DTOs.Sale;

namespace PuntoVenta.Application.Interfaces.Services;

public interface IPdfInvoiceService
{
    byte[] GenerateInvoicePdf(SaleDto sale);
}
