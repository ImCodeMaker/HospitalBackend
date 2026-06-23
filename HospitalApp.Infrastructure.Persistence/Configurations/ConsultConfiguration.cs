using HospitalApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalApp.Infrastructure.Persistence.Configurations;

public class ConsultConfiguration : IEntityTypeConfiguration<Consult>
{
    public void Configure(EntityTypeBuilder<Consult> builder)
    {
        builder.ToTable("consults");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ChiefComplaint).HasMaxLength(1000);
        builder.Property(c => c.DiagnosisCodes).HasMaxLength(500);
        builder.Property(c => c.DiagnosisDescription).HasMaxLength(2000);
        builder.Property(c => c.WeightKg).HasPrecision(5, 2);
        builder.Property(c => c.HeightCm).HasPrecision(5, 2);
        builder.Property(c => c.Bmi).HasPrecision(4, 1);
        builder.Property(c => c.TemperatureCelsius).HasPrecision(4, 1);
        builder.Property(c => c.O2Saturation).HasPrecision(4, 1);

        // JSONB column for specialty-specific dynamic fields
        builder.Property(c => c.SpecialtyData).HasColumnType("jsonb");

        builder.HasIndex(c => new { c.PatientId, c.Status });
        builder.HasIndex(c => new { c.DoctorId, c.CreatedAt });
        builder.HasIndex(c => new { c.DoctorId, c.Status, c.CreatedAt });
        builder.HasIndex(c => new { c.PatientId, c.CreatedAt });

        builder.HasOne(c => c.Patient)
            .WithMany(p => p.Consults)
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Specialty)
            .WithMany(s => s.Consults)
            .HasForeignKey(c => c.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Prescriptions)
            .WithOne(p => p.Consult)
            .HasForeignKey(p => p.ConsultId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.LabOrders)
            .WithOne(l => l.Consult)
            .HasForeignKey(l => l.ConsultId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Images)
            .WithOne(i => i.Consult)
            .HasForeignKey(i => i.ConsultId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.FollowUpAppointments)
            .WithOne(a => a.OriginatingConsult)
            .HasForeignKey(a => a.OriginatingConsultId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Invoice)
            .WithOne(i => i.Consult)
            .HasForeignKey<Invoice>(i => i.ConsultId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
