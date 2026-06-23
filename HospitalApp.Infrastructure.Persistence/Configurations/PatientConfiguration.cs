using HospitalApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalApp.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.DocumentNumber).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Nationality).HasMaxLength(100).IsRequired();
        builder.Property(p => p.HomeAddress).HasMaxLength(500).IsRequired();
        builder.Property(p => p.Email).HasMaxLength(200);
        builder.Property(p => p.Phone).HasMaxLength(30);
        builder.Property(p => p.InsurancePolicyNumber).HasMaxLength(100);
        builder.Property(p => p.InsurancePolicyHolderName).HasMaxLength(200);
        builder.Property(p => p.GuardianFirstName).HasMaxLength(100);
        builder.Property(p => p.GuardianLastName).HasMaxLength(100);
        builder.Property(p => p.GuardianDocumentNumber).HasMaxLength(50);
        builder.Property(p => p.GuardianPhone).HasMaxLength(30);
        builder.Property(p => p.GuardianEmail).HasMaxLength(200);
        builder.Property(p => p.InsuranceCoveragePercentage).HasPrecision(5, 2);

        builder.HasIndex(p => new { p.DocumentType, p.DocumentNumber }).IsUnique();
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => new { p.FirstName, p.LastName });
        builder.HasIndex(p => p.LastName);
        builder.HasIndex(p => p.Email);
        builder.HasIndex(p => p.Phone);

        builder.Ignore(p => p.IsMinor);

        builder.HasOne(p => p.InsuranceCompany)
            .WithMany(ic => ic.Patients)
            .HasForeignKey(p => p.InsuranceCompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Consults)
            .WithOne(c => c.Patient)
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Appointments)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Invoices)
            .WithOne(i => i.Patient)
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
