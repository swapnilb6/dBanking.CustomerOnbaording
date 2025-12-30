
using dBanking.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace dBanking.Infrastructure.DbContext
{
    /// <summary>
    /// EF Core DbContext for Customer Onboarding bounded context, backed by PostgreSQL.
    /// </summary>
    public class AppDBContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        // DbSets
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<KycCase> KycCases => Set<KycCase>();
        public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            // -----------------------------
            // Customer
            // -----------------------------
            b.Entity<Customer>(e =>
            {
                e.HasKey(x => x.CustomerId);

                e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
                e.Property(x => x.LastName).HasMaxLength(100).IsRequired();

                // Map DateOnly to PostgreSQL 'date' column
                e.Property(x => x.Dob)
                 .HasColumnType("date")
                 .IsRequired();

                e.Property(x => x.Email).HasMaxLength(256).IsRequired();
                e.Property(x => x.Phone).HasMaxLength(32).IsRequired();

                e.Property(x => x.Status).IsRequired();
                e.Property(x => x.CreatedAt).IsRequired();

                e.HasIndex(x => x.Email).IsUnique();
                e.HasIndex(x => x.Phone);
                e.HasIndex(x => new { x.FirstName, x.LastName, x.Dob });

                e.HasMany(x => x.KycCases)
                 .WithOne(x => x.Customer)
                 .HasForeignKey(x => x.CustomerId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // -----------------------------
            // KycCase
            // -----------------------------
            b.Entity<KycCase>(e =>
            {
                e.HasKey(x => x.KycCaseId);

                e.Property(x => x.Status).IsRequired();
                e.Property(x => x.ConsentText).IsRequired();
                e.Property(x => x.AcceptedAt).IsRequired();
                e.Property(x => x.CreatedAt).IsRequired();

                // Optional: store evidence refs as jsonb
                // Uncomment if you want PostgreSQL jsonb type:
                // e.Property(x => x.EvidenceRefsJson).HasColumnType("jsonb");

                e.HasIndex(x => new { x.CustomerId, x.Status });
            });

            // -----------------------------
            // AuditRecord (append-only semantics in app layer)
            // -----------------------------
            b.Entity<AuditRecord>(e =>
            {
                e.HasKey(x => x.AuditId);

                e.Property(x => x.EntityType).IsRequired();
                e.Property(x => x.TargetEntityId).IsRequired();
                e.Property(x => x.Action).IsRequired();
                e.Property(x => x.Actor).HasMaxLength(256).IsRequired();
                e.Property(x => x.Timestamp).IsRequired();

                // Optional: store before/after as jsonb for richer querying
                // e.Property(x => x.BeforeJson).HasColumnType("jsonb");
                // e.Property(x => x.AfterJson).HasColumnType("jsonb").IsRequired();

                e.HasIndex(x => new { x.EntityType, x.TargetEntityId, x.Timestamp });
            });
        }
    }
}


