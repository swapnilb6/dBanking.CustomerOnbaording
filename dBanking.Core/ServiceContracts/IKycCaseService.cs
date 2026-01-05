using dBanking.Core.DTOS;
using dBanking.Core.Entities;

namespace dBanking.Core.ServiceContracts
{
    public interface IKycCaseService
    {
        Task<KycCase> StartForCustomerAsync(KycCaseCreateRequestDto dto, CancellationToken ct);
        Task<KycCase> UpdateStatusAsync(KycStatusUpdateRequestDto dto, CancellationToken ct);
        Task<KycCase?> GetByIdAsync(Guid caseId, CancellationToken ct);
        Task<IReadOnlyList<KycCase>> GetByCustomerAsync(Guid customerId, CancellationToken ct);
    }
}
