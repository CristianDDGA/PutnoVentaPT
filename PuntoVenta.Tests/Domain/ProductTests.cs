using PuntoVenta.Domain.Entities;
using PuntoVenta.Domain.Exceptions;

namespace PuntoVenta.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_ValidData_ShouldSetProperties()
    {
        var product = Product.Create("Monitor", 250m, 15);

        Assert.Equal("Monitor", product.Name);
        Assert.Equal(250m, product.Price);
        Assert.Equal(15, product.Stock);
        Assert.True(product.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_ShouldThrow(string name)
    {
        Assert.Throws<DomainException>(() => Product.Create(name, 10m, 5));
    }

    [Fact]
    public void Create_ZeroPrice_ShouldThrow()
    {
        Assert.Throws<DomainException>(() => Product.Create("Cable", 0m, 5));
    }

    [Fact]
    public void ReduceStock_ValidQuantity_ShouldDecreaseStock()
    {
        var product = Product.Create("USB", 5m, 10);

        product.ReduceStock(3);

        Assert.Equal(7, product.Stock);
    }

    [Fact]
    public void ReduceStock_InsufficientStock_ShouldThrow()
    {
        var product = Product.Create("USB", 5m, 2);

        var ex = Assert.Throws<DomainException>(() => product.ReduceStock(5));
        Assert.Contains("Stock insuficiente", ex.Message);
    }

    [Fact]
    public void AddStock_ValidQuantity_ShouldIncreaseStock()
    {
        var product = Product.Create("USB", 5m, 10);

        product.AddStock(5);

        Assert.Equal(15, product.Stock);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var product = Product.Create("Hub", 20m, 8);

        product.Deactivate();

        Assert.False(product.IsActive);
    }
}
