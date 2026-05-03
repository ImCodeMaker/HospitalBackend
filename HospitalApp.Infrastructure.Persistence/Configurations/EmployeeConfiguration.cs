using HospitalApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalApp.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.Property(e => e.Salary).HasColumnType("decimal(12,2)");
        builder.Property(e => e.NationalId).HasMaxLength(20).IsRequired();
        builder.HasIndex(e => e.NationalId).IsUnique();

        builder.HasOne(e => e.DirectSupervisor)
            .WithMany()
            .HasForeignKey(e => e.DirectSupervisorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.PayrollRecords)
            .WithOne(p => p.Employee)
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
