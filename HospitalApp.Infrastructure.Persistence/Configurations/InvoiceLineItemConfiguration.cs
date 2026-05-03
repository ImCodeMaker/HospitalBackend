using HospitalApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalApp.Infrastructure.Persistence.Configurations;

public class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.ToTable("invoice_line_items");
        builder.HasKey(li => li.Id);

        builder.Property(li => li.Description).HasMaxLength(500).IsRequired();
        builder.Property(li => li.UnitPrice).HasPrecision(10, 2);
        builder.Property(li => li.DiscountAmount).HasPrecision(10, 2);
        builder.Property(li => li.InsuranceCoverageAmount).HasPrecision(10, 2);

        builder.Ignore(li => li.PatientAmount);
    }
}
