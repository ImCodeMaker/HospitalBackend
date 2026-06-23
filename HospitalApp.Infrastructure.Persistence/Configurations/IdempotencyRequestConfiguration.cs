using HospitalApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalApp.Infrastructure.Persistence.Configurations;

public class IdempotencyRequestConfiguration : IEntityTypeConfiguration<IdempotencyRequest>
{
    public void Configure(EntityTypeBuilder<IdempotencyRequest> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Key).HasMaxLength(160).IsRequired();
        builder.Property(i => i.RequestHash).HasMaxLength(64).IsRequired();
        builder.Property(i => i.Method).HasMaxLength(10).IsRequired();
        builder.Property(i => i.Path).HasMaxLength(512).IsRequired();
        builder.Property(i => i.Status).HasMaxLength(20).IsRequired();
        builder.Property(i => i.ResponseContentType).HasMaxLength(128);
        builder.Property(i => i.ResponseLocation).HasMaxLength(512);
        builder.Property(i => i.ResponseBody).HasColumnType("text");

        builder.HasIndex(i => new { i.Key, i.Method, i.Path, i.UserId }).IsUnique();
        builder.HasIndex(i => i.ExpiresAt);
        builder.ToTable("idempotency_requests");
    }
}
