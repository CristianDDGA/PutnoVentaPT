using PuntoVenta.Domain.Exceptions;
namespace PuntoVenta.Domain.Entities;

public class Customer
{
    public int    CustomerId     { get; private set; }
    public string DocumentNumber { get; private set; } = string.Empty;
    public string FirstName      { get; private set; } = string.Empty;
    public string LastName       { get; private set; } = string.Empty;
    public string? Phone         { get; private set; }
    public string? Address       { get; private set; }
    public string? City          { get; private set; }
    public string? Email         { get; private set; }
    public bool    IsActive      { get; private set; } = true;
    public string? LastModifiedBy { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }

    // Constructor para EF Core
    private Customer() { }

    public static Customer Create(
        string  documentNumber,
        string  firstName,
        string  lastName,
        string? phone,
        string? address,
        string? city,
        string? email)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new DomainException("El número de documento (RUC/Cédula) es obligatorio.");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("El nombre del cliente es obligatorio.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("El apellido del cliente es obligatorio.");

        return new Customer
        {
            DocumentNumber = documentNumber.Trim(),
            FirstName      = firstName.Trim(),
            LastName       = lastName.Trim(),
            Phone          = phone?.Trim(),
            Address        = address?.Trim(),
            City           = city?.Trim(),
            Email          = email?.Trim(),
            IsActive       = true
        };
    }

    public string FullName => $"{FirstName} {LastName}";

    public void Update(
        string  documentNumber,
        string  firstName,
        string  lastName,
        string? phone,
        string? address,
        string? city,
        string? email,
        string? modifiedBy = null)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new DomainException("El número de documento (RUC/Cédula) es obligatorio.");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("El nombre del cliente es obligatorio.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("El apellido del cliente es obligatorio.");

        DocumentNumber = documentNumber.Trim();
        FirstName      = firstName.Trim();
        LastName       = lastName.Trim();
        Phone          = phone?.Trim();
        Address        = address?.Trim();
        City           = city?.Trim();
        Email          = email?.Trim();

        if (!string.IsNullOrWhiteSpace(modifiedBy))
        {
            LastModifiedBy = modifiedBy;
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}