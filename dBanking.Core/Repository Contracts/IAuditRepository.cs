using dBanking.Core.Entities;

namespace dBanking.Core.Repository_Contracts
{
    public interface IAuditRepository
    {
        Task AddAsync(AuditRecord audit, CancellationToken ct = default);
    }
}
