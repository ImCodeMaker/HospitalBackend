using HospitalApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalApp.Infrastructure.Persistence.Configurations;

public class ClinicSettingsConfiguration : IEntityTypeConfiguration<ClinicSettings>
{
    public void Configure(EntityTypeBuilder<ClinicSettings> builder)
    {
        builder.Property(c => c.OperatingHours).HasColumnType("jsonb");
        builder.Property(c => c.ClinicName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Rnc).HasMaxLength(20);
        builder.Property(c => c.ItbisRate).HasColumnType("decimal(5,4)");
    }
}
