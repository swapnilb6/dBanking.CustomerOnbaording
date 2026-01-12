// Infrastructure/Persistence/Configurations/AuditRecordEntityTypeConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using dBanking.Core.Entities;
public sealed class AuditRecordEntityTypeConfiguration : IEntityTypeConfiguration<AuditRecord>
{
    public void Configure(EntityTypeBuilder<AuditRecord> b)
    {
        b.ToTable("AuditRecords", schema: "public");

        b.HasKey(x => x.AuditRecordId);
        b.Property(x => x.EntityType).IsRequired().HasMaxLength(100);
        b.Property(x => x.Action).IsRequired().HasMaxLength(100);

        // Map to PostgreSQL jsonb
        b.Property(x => x.BeforeJson).HasColumnType("jsonb");
        b.Property(x => x.AfterJson).HasColumnType("jsonb");

        b.Property(x => x.Actor).IsRequired().HasMaxLength(200);
        b.Property(x => x.CorrelationId).HasMaxLength(100);

        // Helpful indexes
        b.HasIndex(x => new { x.EntityType, x.TargetEntityId });
        b.HasIndex(x => x.Timestamp);
        b.HasIndex(x => x.CorrelationId);
        b.HasIndex(x => x.Action);
    }
}
