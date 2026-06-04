using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PuntoVenta.Application.DTOs.Sale;
using PuntoVenta.Application.Interfaces.Services;

namespace PuntoVenta.Infrastructure.Services;

public class PdfService : IPdfService
{
    public byte[] GenerateInvoice(SaleDto saleDto)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(pdfDocument =>
        {
            pdfDocument.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Content().Column(col =>
                {
                    // ── HEADER ───────────────────────────────────────────────
                    col.Item().Row(row =>
                    {
                        // Left - Company Title
                        row.RelativeItem(6).Column(left =>
                        {
                            left.Item().Text("PUNTO DE VENTA").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                            left.Item().Text("SISTEMA DE FACTURACIÓN").FontSize(11).Bold().FontColor(Colors.Grey.Darken2);
                        });

                        // Right - Document Info
                        row.RelativeItem(4).Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Column(right =>
                        {
                            right.Item().Text("FACTURA").FontSize(14).Bold().AlignCenter();
                            right.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            
                            right.Item().PaddingTop(5).Row(r => 
                            {
                                r.RelativeItem().Text("Nº Factura:");
                                r.RelativeItem().Text($"#{saleDto.SaleId:D6}").Bold().AlignRight();
                            });
                            
                            right.Item().PaddingTop(2).Row(r => 
                            {
                                r.RelativeItem().Text("Fecha:");
                                r.RelativeItem().Text(saleDto.SaleDate.ToString("dd/MM/yyyy HH:mm")).AlignRight();
                            });
                        });
                    });

                    // ── CUSTOMER INFO ────────────────────────────────────
                    col.Item().PaddingTop(15).BorderTop(1).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(10).Row(row =>
                    {
                        row.RelativeItem(6).Column(left => 
                        {
                            left.Item().Text(t => { t.Span("Cliente: ").Bold(); t.Span(saleDto.CustomerName.ToUpper()); });
                            left.Item().PaddingTop(2).Text(t => { t.Span("RUC/Cédula: ").Bold(); t.Span(saleDto.CustomerDocument); });
                            
                            var addressStr = saleDto.CustomerAddress;
                            if(!string.IsNullOrEmpty(saleDto.CustomerCity))
                                addressStr += $", {saleDto.CustomerCity}";
                                
                            left.Item().PaddingTop(2).Text(t => { t.Span("Dirección: ").Bold(); t.Span(addressStr); });

                            if (!string.IsNullOrEmpty(saleDto.CustomerPhone))
                                left.Item().PaddingTop(2).Text(t => { t.Span("Teléfono: ").Bold(); t.Span(saleDto.CustomerPhone); });

                            if (!string.IsNullOrEmpty(saleDto.CustomerEmail))
                                left.Item().PaddingTop(2).Text(t => { t.Span("Email: ").Bold(); t.Span(saleDto.CustomerEmail); });
                        });
                        row.RelativeItem(4).Column(right => 
                        {
                            right.Item().Text(t => { t.Span("Método de Pago: ").Bold(); t.Span(saleDto.PaymentType.ToUpper()); });
                            if (!string.IsNullOrEmpty(saleDto.SellerName))
                                right.Item().PaddingTop(2).Text(t => { t.Span("Vendedor: ").Bold(); t.Span(saleDto.SellerName); });
                        });
                    });

                    // ── TABLE ────────────────────────────────────────────
                    col.Item().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);    // ID
                            columns.RelativeColumn(4);    // Producto
                            columns.RelativeColumn(1.5f); // Cantidad
                            columns.RelativeColumn(2);    // Precio Unit.
                            columns.RelativeColumn(2);    // Subtotal
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingBottom(2).Text("ID").Bold().AlignCenter();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingBottom(2).Text("Producto").Bold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingBottom(2).Text("Cantidad").Bold().AlignCenter();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingBottom(2).Text("Precio Unit.").Bold().AlignRight();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingBottom(2).Text("Subtotal").Bold().AlignRight();
                        });

                        // Items
                        foreach (var detail in saleDto.Details)
                        {
                            table.Cell().PaddingVertical(4).Text($"{detail.ProductId}").AlignCenter();
                            table.Cell().PaddingVertical(4).Text(detail.ProductName);
                            table.Cell().PaddingVertical(4).Text(detail.Quantity.ToString()).AlignCenter();
                            table.Cell().PaddingVertical(4).Text($"${detail.UnitPrice:F2}").AlignRight();
                            table.Cell().PaddingVertical(4).Text($"${detail.Subtotal:F2}").AlignRight();
                        }
                    });

                    // ── FOOTER SECTIONS ──────────────────────────────────
                    col.Item().PaddingTop(20).Row(row =>
                    {
                        // Bottom Left: Empty or simple message
                        row.RelativeItem(6).PaddingRight(20).Column(left => 
                        {
                            left.Item().Text("Gracias por su compra.").Italic().FontColor(Colors.Grey.Darken1);
                        });

                        // Bottom Right: Totals
                        row.RelativeItem(4).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(8).Column(totals => 
                        {
                            void AddTotalRow(string label, string value, bool isBold = false)
                            {
                                totals.Item().PaddingVertical(2).Row(r => 
                                {
                                    if(isBold) {
                                        r.RelativeItem().Text(label).Bold();
                                        r.RelativeItem().Text(value).Bold().AlignRight();
                                    } else {
                                        r.RelativeItem().Text(label);
                                        r.RelativeItem().Text(value).AlignRight();
                                    }
                                });
                            }

                            AddTotalRow("Subtotal:", $"${saleDto.Subtotal:F2}");
                            AddTotalRow("IVA (12%):", $"${saleDto.TaxAmount:F2}");
                            
                            totals.Item().PaddingVertical(4).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            
                            AddTotalRow("Total:", $"${saleDto.Total:F2}", isBold: true);
                        });
                    });
                });
            });
        }).GeneratePdf();
    }
}