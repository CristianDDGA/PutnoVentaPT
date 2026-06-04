namespace PuntoVenta.Blazor.Models;

public class SaleModel
{
    public int                   SaleId       { get; set; }
    public int                   CustomerId   { get; set; }
    public string                CustomerName { get; set; } = string.Empty;
    public string                SellerName   { get; set; } = string.Empty;
    public DateTime              SaleDate     { get; set; }
    public string                PaymentType  { get; set; } = "Efectivo";
    public decimal               Subtotal     { get; set; }
    public decimal               TaxAmount    { get; set; }
    public decimal               Total        { get; set; }
    public string                Status       { get; set; } = string.Empty;
    public List<SaleDetailModel> Details      { get; set; } = [];
}

public class SaleDetailModel
{
    public int     ProductId   { get; set; }
    public string  ProductName { get; set; } = string.Empty;
    public int     Quantity    { get; set; }
    public decimal UnitPrice   { get; set; }
    public decimal Subtotal    => Quantity * UnitPrice;
    public int     MaxStock    { get; set; }
}

public class CreateSaleModel
{
    public int                         CustomerId  { get; set; }
    public string                      PaymentType { get; set; } = "Efectivo";
    public List<CreateSaleDetailModel> Details     { get; set; } = [];
}

public class CreateSaleDetailModel
{
    public int ProductId { get; set; }
    public int Quantity  { get; set; }
}