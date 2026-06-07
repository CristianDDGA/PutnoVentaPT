using PuntoVenta.Domain.Exceptions;

namespace PuntoVenta.Domain.Entities;

public class Product
{
    public int     ProductId { get; private set; }
    public string  Name      { get; private set; } = string.Empty;
    public decimal Price     { get; private set; }
    public int     Stock     { get; private set; }
    public bool    IsActive  { get; private set; } = true;
    public string? LastModifiedBy { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }

    private Product() { }

    public static Product Create(string name, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del producto es obligatorio.");

        if (price <= 0)
            throw new DomainException("El precio debe ser mayor a cero.");

        if (stock < 0)
            throw new DomainException("El stock no puede ser negativo.");

        return new Product
        {
            Name  = name.Trim(),
            Price = price,
            Stock = stock,
            IsActive = true
        };
    }

    public void Update(string name, decimal price, int stock, string? modifiedBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del producto es obligatorio.");

        if (price <= 0)
            throw new DomainException("El precio debe ser mayor a cero.");

        if (stock < 0)
            throw new DomainException("El stock no puede ser negativo.");

        Name = name.Trim();
        Price = price;
        Stock = stock;

        if (!string.IsNullOrWhiteSpace(modifiedBy))
        {
            LastModifiedBy = modifiedBy;
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    // Lógica de negocio: reducir stock al vender
    public void ReduceStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad debe ser mayor a cero.");

        if (quantity > Stock)
            throw new DomainException($"Stock insuficiente para '{Name}'. Disponible: {Stock}.");

        Stock -= quantity;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad debe ser mayor a cero.");

        Stock += quantity;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}