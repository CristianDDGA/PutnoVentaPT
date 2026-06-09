using PuntoVenta.Infrastructure.Services;

namespace PuntoVenta.Tests.Infrastructure;

public class SecurityPasswordHasherTests
{
    [Fact]
    public void HashPassword_ShouldProduceVerifiableHash()
    {
        const string password = "Admin123*";

        var hash = SecurityPasswordHasher.HashPassword(password);

        Assert.StartsWith("PBKDF2$", hash);
        Assert.True(SecurityPasswordHasher.VerifyPassword(password, hash));
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ShouldReturnFalse()
    {
        var hash = SecurityPasswordHasher.HashPassword("Admin123*");

        Assert.False(SecurityPasswordHasher.VerifyPassword("WrongPass1*", hash));
    }

    [Fact]
    public void VerifyPassword_InvalidHashFormat_ShouldReturnFalse()
    {
        Assert.False(SecurityPasswordHasher.VerifyPassword("Admin123*", "invalid-hash"));
    }

    [Fact]
    public void HashPassword_SamePassword_ShouldProduceDifferentSalts()
    {
        const string password = "Seller123*";

        var hash1 = SecurityPasswordHasher.HashPassword(password);
        var hash2 = SecurityPasswordHasher.HashPassword(password);

        Assert.NotEqual(hash1, hash2);
        Assert.True(SecurityPasswordHasher.VerifyPassword(password, hash1));
        Assert.True(SecurityPasswordHasher.VerifyPassword(password, hash2));
    }
}
