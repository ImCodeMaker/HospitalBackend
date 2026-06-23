using HospitalApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalApp.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Reason).HasMaxLength(500);
        builder.Property(a => a.Notes).HasMaxLength(1000);

        builder.HasIndex(a => new { a.AssignedDoctorId, a.ScheduledDate });
        builder.HasIndex(a => new { a.PatientId, a.Status });
        builder.HasIndex(a => new { a.Status, a.ScheduledDate });
        builder.HasIndex(a => new { a.AssignedDoctorId, a.Status, a.ScheduledDate });
        builder.HasIndex(a => a.ScheduledDate);
    }
}
