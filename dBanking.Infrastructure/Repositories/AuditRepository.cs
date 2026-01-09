using dBanking.Core.Entities;
using dBanking.Core.Repository_Contracts;
using dBanking.Infrastructure.DbContext;

namespace dBanking.Infrastructure.Repositories
{
    public sealed class AuditRepository : IAuditRepository
    {
        private readonly AppPostgresDbContext _db;

        public AuditRepository(AppPostgresDbContext db) => _db = db;

        public async Task AddAsync(AuditRecord audit, CancellationToken ct = default)
        {
            await _db.Set<AuditRecord>().AddAsync(audit, ct);
            await _db.SaveChangesAsync(ct); // Commit immediately; no updates allowed
        }

    }
}
