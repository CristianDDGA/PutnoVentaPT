using PuntoVenta.Application.DTOs.Sale;
using PuntoVenta.Application.Validators;

namespace PuntoVenta.Tests.Validators;

public class CreateSaleValidatorTests
{
    private readonly CreateSaleValidator _validator = new();

    [Fact]
    public void Validate_ValidSale_ShouldPass()
    {
        var dto = new CreateSaleDto
        {
            CustomerId = 1,
            Details =
            [
                new CreateSaleDetailDto { ProductId = 1, Quantity = 2 }
            ]
        };

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NoCustomer_ShouldFail()
    {
        var dto = new CreateSaleDto
        {
            CustomerId = 0,
            Details = [new CreateSaleDetailDto { ProductId = 1, Quantity = 1 }]
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateSaleDto.CustomerId));
    }

    [Fact]
    public void Validate_EmptyDetails_ShouldFail()
    {
        var dto = new CreateSaleDto { CustomerId = 1, Details = [] };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateSaleDto.Details));
    }

    [Fact]
    public void Validate_InvalidDetailQuantity_ShouldFail()
    {
        var dto = new CreateSaleDto
        {
            CustomerId = 1,
            Details = [new CreateSaleDetailDto { ProductId = 1, Quantity = 0 }]
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
    }
}
