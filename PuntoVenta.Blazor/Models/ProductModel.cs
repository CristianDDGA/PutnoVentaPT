using System.ComponentModel.DataAnnotations;

namespace PuntoVenta.Blazor.Models;

public class ProductModel
{
    public int     ProductId { get; set; }
    public string  Name      { get; set; } = string.Empty;
    public decimal Price     { get; set; }
    public int     Stock     { get; set; }
    public bool    IsActive  { get; set; }
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}

public class CreateProductModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string  Name  { get; set; } = string.Empty;

    [Range(0.01, 10000000.0, ErrorMessage = "El precio debe ser mayor a cero.")]
    public decimal Price { get; set; }

    [Range(0, 10000000, ErrorMessage = "El stock no puede ser negativo o inválido.")]
    public int     Stock { get; set; }
}