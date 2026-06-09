using PuntoVenta.Application.DTOs.Auth;
using PuntoVenta.Application.Validators;

namespace PuntoVenta.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidCredentials_ShouldPass()
    {
        var dto = new LoginRequestDto
        {
            Email    = "admin@puntoventa.local",
            Password = "Admin123*"
        };

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("correo-invalido")]
    public void Validate_InvalidEmail_ShouldFail(string email)
    {
        var dto = new LoginRequestDto { Email = email, Password = "Admin123*" };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequestDto.Email));
    }

    [Fact]
    public void Validate_EmptyPassword_ShouldFail()
    {
        var dto = new LoginRequestDto
        {
            Email    = "admin@puntoventa.local",
            Password = string.Empty
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequestDto.Password));
    }
}
