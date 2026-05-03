using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Billing.DTOs;
using HospitalApp.Core.Application.Features.Consults.DTOs;
using HospitalApp.Core.Application.Features.Reports.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HospitalApp.Infrastructure.Shared.Services;

public class PdfService : IPdfService
{
    static PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateInvoice(InvoiceDto invoice, string clinicName, string clinicAddress)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text(clinicName).Bold().FontSize(18);
                    col.Item().Text(clinicAddress).FontSize(9).FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(4)
                        .Row(row =>
                        {
                            row.RelativeItem().Text($"FACTURA #{invoice.InvoiceNumber}").Bold().FontSize(14);
                            row.ConstantItem(120).AlignRight()
                                .Text($"Fecha: {invoice.CreatedAt:dd/MM/yyyy}").FontSize(9);
                        });
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Background(Colors.Grey.Lighten3).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Text($"Paciente: {invoice.PatientName}").Bold();
                        row.ConstantItem(150).AlignRight().Text($"Estado: {invoice.Status}");
                    });

                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(4);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken2).Padding(4)
                                .Text("Descripción").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken2).Padding(4).AlignCenter()
                                .Text("Cant.").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken2).Padding(4).AlignRight()
                                .Text("Precio").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken2).Padding(4).AlignRight()
                                .Text("Descuento").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken2).Padding(4).AlignRight()
                                .Text("Total").FontColor(Colors.White).Bold();
                        });

                        foreach (var (item, idx) in invoice.LineItems.Select((x, i) => (x, i)))
                        {
                            var bg = idx % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            table.Cell().Background(bg).Padding(4).Text(item.Description);
                            table.Cell().Background(bg).Padding(4).AlignCenter().Text(item.Quantity.ToString());
                            table.Cell().Background(bg).Padding(4).AlignRight().Text($"RD$ {item.UnitPrice:N2}");
                            table.Cell().Background(bg).Padding(4).AlignRight().Text($"RD$ {item.DiscountAmount:N2}");
                            table.Cell().Background(bg).Padding(4).AlignRight().Text($"RD$ {item.PatientAmount:N2}");
                        }
                    });

                    col.Item().PaddingTop(8).AlignRight().Column(summary =>
                    {
                        summary.Item().Row(r =>
                        {
                            r.ConstantItem(140).Text("Subtotal:").AlignRight();
                            r.ConstantItem(100).Text($"RD$ {invoice.Subtotal:N2}").AlignRight();
                        });
                        summary.Item().Row(r =>
                        {
                            r.ConstantItem(140).Text("Descuento:").AlignRight();
                            r.ConstantItem(100).Text($"RD$ {invoice.DiscountAmount:N2}").AlignRight();
                        });
                        summary.Item().Row(r =>
                        {
                            r.ConstantItem(140).Text("ITBIS:").AlignRight();
                            r.ConstantItem(100).Text($"RD$ {invoice.TaxAmount:N2}").AlignRight();
                        });
                        summary.Item().Row(r =>
                        {
                            r.ConstantItem(140).Text("Seguro:").AlignRight();
                            r.ConstantItem(100).Text($"RD$ {invoice.InsuranceCoverageAmount:N2}").AlignRight();
                        });
                        summary.Item().BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingTop(4).Row(r =>
                        {
                            r.ConstantItem(140).Text("TOTAL:").Bold().AlignRight();
                            r.ConstantItem(100).Text($"RD$ {invoice.PatientResponsibilityAmount:N2}").Bold().AlignRight();
                        });
                        if (invoice.BalanceDue > 0)
                        {
                            summary.Item().Row(r =>
                            {
                                r.ConstantItem(140).Text("Balance Pendiente:").FontColor(Colors.Red.Medium).AlignRight();
                                r.ConstantItem(100).Text($"RD$ {invoice.BalanceDue:N2}").FontColor(Colors.Red.Medium).AlignRight();
                            });
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateSickNote(ConsultDto consult, string patientName, string doctorName, int daysRest)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(3, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Content().Column(col =>
                {
                    col.Item().AlignCenter().Text("CERTIFICADO MÉDICO DE REPOSO").Bold().FontSize(16);
                    col.Item().PaddingTop(20).Text($"Yo, {doctorName}, certifico que el/la paciente:");
                    col.Item().PaddingTop(8).Text(patientName).Bold().FontSize(14);
                    col.Item().PaddingTop(8).Text(
                        $"requiere reposo médico por {daysRest} día(s) a partir de la fecha {consult.FinishedAt?.ToString("dd/MM/yyyy") ?? DateTime.Today.ToString("dd/MM/yyyy")}.");

                    if (!string.IsNullOrEmpty(consult.DiagnosisDescription))
                    {
                        col.Item().PaddingTop(8).Text($"Diagnóstico: {consult.DiagnosisDescription}");
                    }

                    col.Item().PaddingTop(40).AlignRight().Text("_______________________________");
                    col.Item().AlignRight().Text(doctorName).Bold();
                    col.Item().AlignRight().Text($"Fecha: {DateTime.Today:dd/MM/yyyy}").FontSize(9);
                });
            });
        }).GeneratePdf();
    }

    public byte[] GeneratePrescription(ConsultDto consult, string patientName, string doctorName)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(3, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Content().Column(col =>
                {
                    col.Item().AlignCenter().Text("PRESCRIPCIÓN MÉDICA").Bold().FontSize(16);
                    col.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Text($"Paciente: {patientName}").Bold();
                        row.ConstantItem(150).AlignRight().Text($"Fecha: {DateTime.Today:dd/MM/yyyy}");
                    });
                    col.Item().BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingBottom(8)
                        .Text($"Doctor: {doctorName}");

                    col.Item().PaddingTop(16).Text("Rp/").Bold().FontSize(14);

                    if (!string.IsNullOrEmpty(consult.TreatmentPlan))
                    {
                        col.Item().PaddingTop(8).Text(consult.TreatmentPlan);
                    }

                    col.Item().PaddingTop(60).AlignRight().Text("_______________________________");
                    col.Item().AlignRight().Text(doctorName).Bold();
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateDailyRevenueReport(DailyRevenueSummaryDto reportData, DateTime date)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("Daily Revenue Report").Bold().FontSize(18);
                    col.Item().PaddingTop(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(4)
                        .Text($"Date: {date:dd/MM/yyyy}").FontSize(10).FontColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                                .Text("Metric").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignRight()
                                .Text("Value").FontColor(Colors.White).Bold();
                        });

                        var rows = new[]
                        {
                            ("Total Revenue",       $"RD$ {reportData.TotalRevenue:N2}"),
                            ("Cash Revenue",        $"RD$ {reportData.CashRevenue:N2}"),
                            ("Card Revenue",        $"RD$ {reportData.CardRevenue:N2}"),
                            ("Transfer Revenue",    $"RD$ {reportData.TransferRevenue:N2}"),
                            ("Insurance Revenue",   $"RD$ {reportData.InsuranceRevenue:N2}"),
                            ("Total Invoices",      reportData.TotalInvoices.ToString()),
                            ("Paid Invoices",       reportData.PaidInvoices.ToString()),
                        };

                        foreach (var (idx, (label, value)) in rows.Select((r, i) => (i, r)))
                        {
                            var bg = idx % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            table.Cell().Background(bg).Padding(6).Text(label);
                            table.Cell().Background(bg).Padding(6).AlignRight().Text(value);
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateAccountsReceivableReport(List<AccountsReceivableDto> reportData, DateTime asOf)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text("Accounts Receivable Report").Bold().FontSize(18);
                    col.Item().PaddingTop(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(4)
                        .Text($"As of: {asOf:dd/MM/yyyy}").FontSize(10).FontColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            foreach (var heading in new[] { "Invoice #", "Patient", "Balance Due", "Days", "Aging" })
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text(heading).FontColor(Colors.White).Bold();
                            }
                        });

                        foreach (var (item, idx) in reportData.Select((x, i) => (x, i)))
                        {
                            var bg = idx % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            table.Cell().Background(bg).Padding(5).Text(item.InvoiceNumber);
                            table.Cell().Background(bg).Padding(5).Text(item.PatientName);
                            table.Cell().Background(bg).Padding(5).AlignRight().Text($"RD$ {item.BalanceDue:N2}");
                            table.Cell().Background(bg).Padding(5).AlignCenter().Text(item.DaysOutstanding.ToString());
                            table.Cell().Background(bg).Padding(5).AlignCenter().Text(item.AgingBucket);
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateInventoryReport(List<InventoryReportItemDto> reportData, DateTime asOf)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text("Inventory Report").Bold().FontSize(18);
                    col.Item().PaddingTop(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(4)
                        .Text($"As of: {asOf:dd/MM/yyyy}").FontSize(10).FontColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            foreach (var heading in new[] { "Generic Name", "Brand", "Stock", "Min Stock", "Sale Price", "Expiry" })
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text(heading).FontColor(Colors.White).Bold();
                            }
                        });

                        foreach (var (item, idx) in reportData.Select((x, i) => (x, i)))
                        {
                            var bg = item.IsOutOfStock ? Colors.Red.Lighten4
                                   : item.IsLowStock   ? Colors.Orange.Lighten4
                                   : idx % 2 == 0      ? Colors.White
                                                       : Colors.Grey.Lighten4;

                            table.Cell().Background(bg).Padding(5).Text(item.GenericName);
                            table.Cell().Background(bg).Padding(5).Text(item.BrandName ?? "-");
                            table.Cell().Background(bg).Padding(5).AlignCenter().Text(item.CurrentStock.ToString());
                            table.Cell().Background(bg).Padding(5).AlignCenter().Text(item.MinimumStockThreshold.ToString());
                            table.Cell().Background(bg).Padding(5).AlignRight().Text($"RD$ {item.SalePrice:N2}");
                            table.Cell().Background(bg).Padding(5).AlignCenter()
                                .Text(item.EarliestExpirationDate.HasValue ? item.EarliestExpirationDate.Value.ToString("dd/MM/yyyy") : "-");
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateShiftReport(ShiftReportData data)
    {
        var totalCashIn  = data.Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
        var totalCashOut = data.Transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("REPORTE DE TURNO — Lova Salud").Bold().FontSize(18);
                    col.Item().PaddingTop(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(6)
                        .Row(row =>
                        {
                            row.RelativeItem().Text($"Turno: {data.ShiftId}").FontSize(8).FontColor(Colors.Grey.Medium);
                            row.ConstantItem(200).AlignRight()
                                .Text($"Apertura: {data.OpenedAt:dd/MM/yyyy HH:mm}  |  Cierre: {data.ClosedAt:dd/MM/yyyy HH:mm}")
                                .FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    // Transactions table
                    col.Item().Text("Transacciones").Bold().FontSize(12);
                    col.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(4);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            foreach (var heading in new[] { "Tipo", "Monto", "Descripción", "Aprobado", "Hora" })
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text(heading).FontColor(Colors.White).Bold();
                            }
                        });

                        foreach (var (tx, idx) in data.Transactions.Select((x, i) => (x, i)))
                        {
                            var bg = idx % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            table.Cell().Background(bg).Padding(5).Text(tx.Type);
                            table.Cell().Background(bg).Padding(5).AlignRight().Text($"RD$ {tx.Amount:N2}");
                            table.Cell().Background(bg).Padding(5).Text(tx.Description ?? "-");
                            table.Cell().Background(bg).Padding(5).AlignCenter().Text(tx.IsApproved ? "Sí" : "No");
                            table.Cell().Background(bg).Padding(5).Text(tx.CreatedAt.ToString("HH:mm"));
                        }
                    });

                    // Summary section
                    col.Item().PaddingTop(16).BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingTop(10)
                        .Text("Resumen").Bold().FontSize(12);

                    col.Item().PaddingTop(8).AlignRight().Column(summary =>
                    {
                        void SummaryRow(ColumnDescriptor s, string label, decimal value, string? color = null)
                        {
                            s.Item().Row(r =>
                            {
                                var labelText = r.ConstantItem(180).AlignRight().Text(label);
                                var valueText = r.ConstantItem(120).AlignRight().Text($"RD$ {value:N2}");
                                if (color is not null)
                                {
                                    labelText.FontColor(color);
                                    valueText.FontColor(color);
                                }
                            });
                        }

                        SummaryRow(summary, "Balance de apertura:", data.OpeningBalance);
                        SummaryRow(summary, "Total ingresos:", totalCashIn);
                        SummaryRow(summary, "Total egresos:", totalCashOut);
                        SummaryRow(summary, "Balance esperado:", data.ExpectedBalance);
                        SummaryRow(summary, "Balance de cierre:", data.ClosingBalance);
                        SummaryRow(summary, "Diferencia:", data.Discrepancy,
                            data.Discrepancy != 0 ? Colors.Red.Medium : null);
                    });

                    // Notes
                    if (!string.IsNullOrWhiteSpace(data.Notes))
                    {
                        col.Item().PaddingTop(16).Column(notes =>
                        {
                            notes.Item().Text("Notas:").Bold();
                            notes.Item().PaddingTop(4).Text(data.Notes).FontColor(Colors.Grey.Darken2);
                        });
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}
