using System.ComponentModel.DataAnnotations;

namespace PuntoVenta.Blazor.Models;

public class CustomerModel
{
    public int     CustomerId     { get; set; }
    public string  DocumentNumber { get; set; } = string.Empty;
    public string  FirstName      { get; set; } = string.Empty;
    public string  LastName       { get; set; } = string.Empty;
    public string? Phone          { get; set; }
    public string? Address        { get; set; }
    public string? City           { get; set; }
    public string? Email          { get; set; }
    public bool    IsActive       { get; set; }
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string  FullName       => $"{FirstName} {LastName}";
}

/// <summary>
/// Payload sent to POST /api/Customers when creating a new customer from the modal form.
/// </summary>
public class CreateCustomerModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "El número de documento (RUC/Cédula) es obligatorio.")]
    [StringLength(20, ErrorMessage = "El número de documento no puede superar los 20 caracteres.")]
    public string  DocumentNumber { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string  FirstName      { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
    public string  LastName       { get; set; } = string.Empty;

    [StringLength(10, ErrorMessage = "El teléfono no puede superar los 10 caracteres.")]
    [RegularExpression(@"^\d*$", ErrorMessage = "El teléfono solo puede contener números.")]
    public string? Phone          { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
    public string? Address        { get; set; }

    [StringLength(100, ErrorMessage = "La ciudad no puede superar los 100 caracteres.")]
    public string? City           { get; set; }

    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    public string? Email          { get; set; }
}

public class UpdateCustomerModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "El número de documento (RUC/Cédula) es obligatorio.")]
    [StringLength(20, ErrorMessage = "El número de documento no puede superar los 20 caracteres.")]
    public string  DocumentNumber { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string  FirstName      { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
    public string  LastName       { get; set; } = string.Empty;

    [StringLength(10, ErrorMessage = "El teléfono no puede superar los 10 caracteres.")]
    [RegularExpression(@"^\d*$", ErrorMessage = "El teléfono solo puede contener números.")]
    public string? Phone          { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
    public string? Address        { get; set; }

    [StringLength(100, ErrorMessage = "La ciudad no puede superar los 100 caracteres.")]
    public string? City           { get; set; }

    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    public string? Email          { get; set; }
}