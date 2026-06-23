using HospitalApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalApp.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber).HasMaxLength(50).IsRequired();
        builder.Property(i => i.Subtotal).HasPrecision(12, 2);
        builder.Property(i => i.DiscountAmount).HasPrecision(12, 2);
        builder.Property(i => i.TaxAmount).HasPrecision(12, 2);
        builder.Property(i => i.InsuranceCoverageAmount).HasPrecision(12, 2);
        builder.Property(i => i.TotalAmount).HasPrecision(12, 2);
        builder.Property(i => i.PatientResponsibilityAmount).HasPrecision(12, 2);
        builder.Property(i => i.PaidAmount).HasPrecision(12, 2);

        builder.Ignore(i => i.BalanceDue);

        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => new { i.PatientId, i.Status, i.CreatedAt });
        builder.HasIndex(i => new { i.Status, i.CreatedAt });
        builder.HasIndex(i => i.DueDate);

        builder.HasMany(i => i.LineItems)
            .WithOne(li => li.Invoice)
            .HasForeignKey(li => li.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
