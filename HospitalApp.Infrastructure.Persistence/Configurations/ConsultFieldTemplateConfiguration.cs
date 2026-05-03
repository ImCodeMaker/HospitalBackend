using HospitalApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalApp.Infrastructure.Persistence.Configurations;

public class ConsultFieldTemplateConfiguration : IEntityTypeConfiguration<ConsultFieldTemplate>
{
    public void Configure(EntityTypeBuilder<ConsultFieldTemplate> builder)
    {
        builder.Property(c => c.FieldOptions).HasColumnType("jsonb");
        builder.Property(c => c.FieldKey).HasMaxLength(100).IsRequired();
        builder.Property(c => c.FieldLabel).HasMaxLength(200).IsRequired();
        builder.Property(c => c.FieldType).HasMaxLength(50).IsRequired();
        builder.HasIndex(c => new { c.SpecialtyId, c.FieldKey }).IsUnique();
    }
}
