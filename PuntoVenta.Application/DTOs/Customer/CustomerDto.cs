namespace PuntoVenta.Application.DTOs.Customer;

public class CustomerDto
{
    public int    CustomerId     { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string FirstName      { get; set; } = string.Empty;
    public string LastName       { get; set; } = string.Empty;
    public string? Phone         { get; set; }
    public string? Address       { get; set; }
    public string? City          { get; set; }
    public string? Email         { get; set; }
    public bool   IsActive       { get; set; }
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string FullName       => $"{FirstName} {LastName}";
}