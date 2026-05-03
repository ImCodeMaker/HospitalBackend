using HospitalApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalApp.Infrastructure.Persistence.Configurations;

public class MedicationConfiguration : IEntityTypeConfiguration<Medication>
{
    public void Configure(EntityTypeBuilder<Medication> builder)
    {
        builder.ToTable("medications");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.GenericName).HasMaxLength(200).IsRequired();
        builder.Property(m => m.BrandName).HasMaxLength(200);
        builder.Property(m => m.AtcCode).HasMaxLength(20);
        builder.Property(m => m.Strength).HasMaxLength(50).IsRequired();
        builder.Property(m => m.UnitOfMeasure).HasMaxLength(50).IsRequired();
        builder.Property(m => m.StorageLocation).HasMaxLength(100);
        builder.Property(m => m.Supplier).HasMaxLength(200);
        builder.Property(m => m.ControlledSubstanceClass).HasMaxLength(50);
        builder.Property(m => m.BatchNumber).HasMaxLength(100);
        builder.Property(m => m.CostPrice).HasPrecision(10, 2);
        builder.Property(m => m.SalePrice).HasPrecision(10, 2);

        builder.HasIndex(m => m.GenericName);
        builder.HasIndex(m => m.CurrentStock);
        builder.HasIndex(m => m.EarliestExpirationDate);

        builder.Ignore(m => m.IsLowStock);
        builder.Ignore(m => m.IsOutOfStock);
        builder.Ignore(m => m.IsExpired);
        builder.Ignore(m => m.IsExpiringSoon);

        builder.HasMany(m => m.StockTransactions)
            .WithOne(t => t.Medication)
            .HasForeignKey(t => t.MedicationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
