using PuntoVenta.Application.DTOs.User;
using PuntoVenta.Application.Validators;

namespace PuntoVenta.Tests.Validators;

public class CreateUserValidatorTests
{
    private readonly CreateUserValidator _validator = new();

    private static CreateUserDto ValidDto() => new()
    {
        Username = "juan.perez",
        Password = "Admin123*",
        FullName = "Juan Pérez",
        Email    = "juan@example.com",
        RoleId   = 1
    };

    [Fact]
    public void Validate_ValidDto_ShouldPass()
    {
        var result = _validator.Validate(ValidDto());
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("usuario invalido")]
    public void Validate_InvalidUsername_ShouldFail(string username)
    {
        var dto = ValidDto();
        dto.Username = username;

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserDto.Username));
    }

    [Theory]
    [InlineData("short1")]
    [InlineData("noupper1*")]
    [InlineData("NOLOWER1*")]
    [InlineData("NoNumber*")]
    [InlineData("NoSpecial1")]
    public void Validate_WeakPassword_ShouldFail(string password)
    {
        var dto = ValidDto();
        dto.Password = password;

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserDto.Password));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_InvalidEmail_ShouldFail(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserDto.Email));
    }

    [Fact]
    public void Validate_RoleIdZero_ShouldFail()
    {
        var dto = ValidDto();
        dto.RoleId = 0;

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserDto.RoleId));
    }
}
