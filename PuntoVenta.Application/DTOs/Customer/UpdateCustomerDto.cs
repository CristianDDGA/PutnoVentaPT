namespace PuntoVenta.Application.DTOs.Customer;

public class UpdateCustomerDto
{
    public string  DocumentNumber { get; set; } = string.Empty;
    public string  FirstName      { get; set; } = string.Empty;
    public string  LastName       { get; set; } = string.Empty;
    public string? Phone          { get; set; }
    public string? Address        { get; set; }
    public string? City           { get; set; }
    public string? Email          { get; set; }
}
