using PuntoVenta.Application.DTOs.Product;
using PuntoVenta.Application.Validators;

namespace PuntoVenta.Tests.Validators;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [Fact]
    public void Validate_ValidProduct_ShouldPass()
    {
        var dto = new CreateProductDto { Name = "Laptop", Price = 999.99m, Stock = 10 };

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_ShouldFail(string name)
    {
        var dto = new CreateProductDto { Name = name, Price = 10m, Stock = 5 };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductDto.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidPrice_ShouldFail(decimal price)
    {
        var dto = new CreateProductDto { Name = "Mouse", Price = price, Stock = 5 };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductDto.Price));
    }

    [Fact]
    public void Validate_NegativeStock_ShouldFail()
    {
        var dto = new CreateProductDto { Name = "Teclado", Price = 25m, Stock = -1 };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductDto.Stock));
    }
}
