namespace PuntoVenta.Application.DTOs.Product;

public class ProductDto
{
    public int     ProductId { get; set; }
    public string  Name      { get; set; } = string.Empty;
    public decimal Price     { get; set; }
    public int     Stock     { get; set; }
    public bool    IsActive  { get; set; }
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}