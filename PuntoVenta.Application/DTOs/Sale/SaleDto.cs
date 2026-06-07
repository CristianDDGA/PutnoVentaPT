namespace PuntoVenta.Application.DTOs.Sale;

public class SaleDto
{
    public int            SaleId           { get; set; }
    public int            CustomerId       { get; set; }
    public string         CustomerName     { get; set; } = string.Empty;
    public string         CustomerDocument { get; set; } = string.Empty;
    public string         CustomerAddress  { get; set; } = string.Empty;
    public string         CustomerCity     { get; set; } = string.Empty;
    public string         CustomerPhone    { get; set; } = string.Empty;
    public string         CustomerEmail    { get; set; } = string.Empty;
    public string         SellerName       { get; set; } = string.Empty;
    public string         CreatedByUsername { get; set; } = string.Empty;
    public DateTime       SaleDate    { get; set; }
    public string         PaymentType { get; set; } = string.Empty;
    public decimal        Subtotal    { get; set; }
    public decimal        TaxAmount   { get; set; }
    public decimal        Total       { get; set; }
    public string         Status      { get; set; } = string.Empty;
    public List<SaleDetailDto> Details { get; set; } = [];
}