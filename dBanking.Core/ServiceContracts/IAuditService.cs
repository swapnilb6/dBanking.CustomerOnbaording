using dBanking.Core.DTOS;



namespace dBanking.Core.ServiceContracts
{
    public interface IAuditService
    {
        Task RecordAsync(AuditEntryDto entry, CancellationToken ct = default);
    }

}
