using HospitalApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalApp.Infrastructure.Persistence.Configurations;

public class CajaShiftConfiguration : IEntityTypeConfiguration<CajaShift>
{
    public void Configure(EntityTypeBuilder<CajaShift> builder)
    {
        builder.Property(c => c.OpeningBalance).HasColumnType("decimal(12,2)");
        builder.Property(c => c.ClosingBalance).HasColumnType("decimal(12,2)");
        builder.Property(c => c.ExpectedBalance).HasColumnType("decimal(12,2)");
        builder.Property(c => c.Discrepancy).HasColumnType("decimal(12,2)");

        builder.HasMany(s => s.Transactions)
            .WithOne(t => t.Shift)
            .HasForeignKey(t => t.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
