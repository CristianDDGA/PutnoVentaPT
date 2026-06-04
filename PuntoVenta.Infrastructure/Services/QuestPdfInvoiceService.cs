using PuntoVenta.Application.DTOs.Sale;
using PuntoVenta.Application.Interfaces.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PuntoVenta.Infrastructure.Services;

public class QuestPdfInvoiceService : IPdfInvoiceService
{
    public QuestPdfInvoiceService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateInvoicePdf(SaleDto sale)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(header => ComposeHeader(header, sale));
                page.Content().Element(content => ComposeContent(content, sale));
                page.Footer().Element(footer => ComposeFooter(footer, sale));
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, SaleDto sale)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("FACTURA").FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text($"Factura #{sale.SaleId}");
                column.Item().Text($"Fecha: {sale.SaleDate:dd/MM/yyyy HH:mm}");
                column.Item().Text($"Método de Pago: {sale.PaymentType}");
                column.Item().Text($"Estado: {sale.Status}");
            });

            row.ConstantItem(100).Height(50).Placeholder(); // Placeholder for logo
        });
    }

    private void ComposeContent(IContainer container, SaleDto sale)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(5);

            column.Item().Row(row =>
            {
                row.RelativeItem().Component(new AddressComponent("Información del Cliente", sale.CustomerName, sale.CustomerDocument, sale.CustomerAddress, sale.CustomerCity, sale.CustomerPhone, sale.CustomerEmail));
                row.ConstantItem(50);
                row.RelativeItem().Component(new AddressComponent("Atendido por", sale.SellerName, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));
            });

            column.Item().PaddingTop(20).Element(tableContainer => ComposeTable(tableContainer, sale));

            var totalPrice = sale.Total;
            column.Item().AlignRight().Text($"Subtotal: ${sale.Subtotal:N2}").FontSize(12);
            column.Item().AlignRight().Text($"Impuestos: ${sale.TaxAmount:N2}").FontSize(12);
            column.Item().AlignRight().Text($"Total: ${totalPrice:N2}").FontSize(14).SemiBold();
        });
    }

    private void ComposeTable(IContainer container, SaleDto sale)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.ConstantColumn(80);
                columns.ConstantColumn(80);
                columns.ConstantColumn(80);
            });

            table.Header(header =>
            {
                header.Cell().Text("Producto").SemiBold();
                header.Cell().AlignRight().Text("Precio Unit.");
                header.Cell().AlignRight().Text("Cant.");
                header.Cell().AlignRight().Text("Subtotal");

                header.Cell().ColumnSpan(4).PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);
            });

            foreach (var detail in sale.Details)
            {
                table.Cell().Element(CellStyle).Text(detail.ProductName);
                table.Cell().Element(CellStyle).AlignRight().Text($"${detail.UnitPrice:N2}");
                table.Cell().Element(CellStyle).AlignRight().Text($"{detail.Quantity}");
                table.Cell().Element(CellStyle).AlignRight().Text($"${detail.Subtotal:N2}");

                static IContainer CellStyle(IContainer cellContainer)
                {
                    return cellContainer.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            }
        });
    }

    private void ComposeFooter(IContainer container, SaleDto sale)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Página ");
            x.CurrentPageNumber();
            x.Span(" de ");
            x.TotalPages();
        });
    }
}

public class AddressComponent : IComponent
{
    private string Title { get; }
    private string Name { get; }
    private string Document { get; }
    private string Address { get; }
    private string City { get; }
    private string Phone { get; }
    private string Email { get; }

    public AddressComponent(string title, string name, string document, string address, string city, string phone, string email)
    {
        Title = title;
        Name = name;
        Document = document;
        Address = address;
        City = city;
        Phone = phone;
        Email = email;
    }

    public void Compose(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(2);

            column.Item().BorderBottom(1).PaddingBottom(5).Text(Title).SemiBold();
            
            column.Item().Text(Name);
            if (!string.IsNullOrWhiteSpace(Document)) column.Item().Text($"Doc: {Document}");
            if (!string.IsNullOrWhiteSpace(Address)) column.Item().Text(Address);
            if (!string.IsNullOrWhiteSpace(City)) column.Item().Text(City);
            if (!string.IsNullOrWhiteSpace(Phone)) column.Item().Text(Phone);
            if (!string.IsNullOrWhiteSpace(Email)) column.Item().Text(Email);
        });
    }
}
