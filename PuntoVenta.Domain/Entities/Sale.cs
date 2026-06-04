using PuntoVenta.Domain.Enums;
using PuntoVenta.Domain.Exceptions;

namespace PuntoVenta.Domain.Entities;

public class Sale
{
    public const decimal TaxRate = 0.12m;

    public int         SaleId      { get; private set; }
    public int         CustomerId  { get; private set; }
    public string      CustomerDocument { get; private set; } = string.Empty;
    public string      CustomerName     { get; private set; } = string.Empty;
    public DateTime    SaleDate    { get; private set; }
    public PaymentType PaymentType { get; private set; }
    public decimal     Subtotal    { get; private set; }
    public decimal     TaxAmount   { get; private set; }
    public decimal     Total       { get; private set; }
    public SaleStatus  Status      { get; private set; } = SaleStatus.Draft;

    public int?        UserId      { get; private set; }
    public string?     SellerName  { get; private set; }

    public Customer Customer { get; private set; } = null!;
    public User?    User     { get; private set; }

    // ✅ Sin readonly para poder asignarlo en el método Create
    private List<SaleDetail> _details = [];
    public IReadOnlyList<SaleDetail> Details => _details.AsReadOnly();

    private Sale() { }

    public static Sale Create(int customerId, string customerDocument, string customerName, PaymentType paymentType, List<SaleDetail> details, int? userId = null, string? sellerName = null)
    {
        if (customerId <= 0)
            throw new DomainException("El cliente es obligatorio.");
        if (string.IsNullOrWhiteSpace(customerDocument))
            throw new DomainException("El documento del cliente es obligatorio para el historial.");
        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("El nombre del cliente es obligatorio para el historial.");

        if (details == null || details.Count == 0)
            throw new DomainException("La factura debe tener al menos un producto.");

        var subtotal  = details.Sum(saleDetail => saleDetail.Subtotal);
        var taxAmount = Math.Round(subtotal * TaxRate, 2);
        var total     = subtotal + taxAmount;

        // ✅ Se asigna a través del constructor privado
        return new Sale
        {
            CustomerId       = customerId,
            CustomerDocument = customerDocument,
            CustomerName     = customerName,
            PaymentType      = paymentType,
            SaleDate         = DateTime.Now,
            Subtotal         = subtotal,
            TaxAmount        = taxAmount,
            Total            = total,
            Status           = SaleStatus.Draft,
            UserId           = userId,
            SellerName       = sellerName,
            _details         = details
        };
    }

    public void UpdateDraft(int customerId, string customerDocument, string customerName, PaymentType paymentType, List<SaleDetail> details, int? userId = null, string? sellerName = null)
    {
        if (Status != SaleStatus.Draft)
            throw new DomainException("Solo se pueden modificar facturas en estado Borrador.");

        if (customerId <= 0)
            throw new DomainException("El cliente es obligatorio.");
        if (string.IsNullOrWhiteSpace(customerDocument))
            throw new DomainException("El documento del cliente es obligatorio para el historial.");
        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("El nombre del cliente es obligatorio para el historial.");

        CustomerId       = customerId;
        CustomerDocument = customerDocument;
        CustomerName     = customerName;
        PaymentType      = paymentType;
        UserId           = userId;
        SellerName       = sellerName;

        _details.Clear();
        if (details != null && details.Count > 0)
        {
            _details.AddRange(details);
        }

        var subtotal = _details.Sum(saleDetail => saleDetail.Subtotal);
        TaxAmount    = Math.Round(subtotal * TaxRate, 2);
        Subtotal     = subtotal;
        Total        = subtotal + TaxAmount;
    }

    public void ConfirmSale()
    {
        if (Status == SaleStatus.Confirmed)
            throw new DomainException("La factura ya se encuentra confirmada.");

        if (Status == SaleStatus.Cancelled)
            throw new DomainException("La factura ya se encuentra cancelada.");

        Status = SaleStatus.Confirmed;
    }

    public void CancelSale()
    {
        if (Status == SaleStatus.Cancelled)
            throw new DomainException("La factura ya se encuentra cancelada.");

        Status = SaleStatus.Cancelled;
    }

    public void MarkAsPaid()
    {
        ConfirmSale();
    }

    public void VoidSale()
    {
        CancelSale();
    }
}