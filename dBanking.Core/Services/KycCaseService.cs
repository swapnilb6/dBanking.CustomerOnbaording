
using AutoMapper;
using dBanking.Core.DTOS;
using dBanking.Core.Entities;
using dBanking.Core.Messages;
using dBanking.Core.Repository_Contracts;
using dBanking.Core.ServiceContracts;
using FluentValidation;
using MassTransit;
using System.Text.Json;

namespace dBanking.Core.Services
{
    public sealed class KycCaseService : IKycCaseService
    {
        private readonly IKycCaseRepository _kycCases;
        private readonly ICustomerRepository _customers;
        private readonly IPublishEndpoint _publish;
        private readonly IMapper _mapper;

        public KycCaseService(
            IKycCaseRepository kycCases,
            ICustomerRepository customers,
            IMapper mapper,
            IPublishEndpoint publish)
        {
            _kycCases = kycCases;
            _customers = customers;
            _mapper = mapper;
            _publish = publish;
        }

        public async Task<KycCase> StartForCustomerAsync(KycCaseCreateRequestDto dto, CancellationToken ct)
        {
            // Idempotent start: if already PENDING case exists, return it
            var existing = await _kycCases.FindOpenForCustomerAsync(dto.CustomerId, ct);
            if (existing is not null) return existing;

            // Ensure customer exists
            var customer = await _customers.GetByIdAsync(dto.CustomerId, ct);
            if (customer is null)
                throw new KeyNotFoundException($"Customer '{dto.CustomerId}' not found.");

            // Map DTO -> entity
            var entity = _mapper.Map<KycCase>(dto);

            await _kycCases.AddAsync(entity, ct);
            await _kycCases.SaveChangesAsync(ct);

            // (Optional) Audit: create KYC case
            // await _audit.RecordAsync(...)

            return entity;
        }

        public async Task<KycCase> UpdateStatusAsync(KycStatusUpdateRequestDto dto, CancellationToken ct)
        {
            var caseEntity = await _kycCases.GetByIdAsync(dto.KycCaseId, ct);
            if (caseEntity is null)
                throw new KeyNotFoundException($"KYC case '{dto.KycCaseId}' not found.");

            if (caseEntity.CustomerId != dto.CustomerId)
                throw new ValidationException("CustomerId mismatch for provided KycCaseId.");

            var target = MapDtoStatus(dto.Status);

            // Terminal guard
            if (caseEntity.Status == KycStatus.VERIFIED || caseEntity.Status == KycStatus.FAILED)
                throw new InvalidOperationException("Cannot update a terminal KYC case (VERIFIED/FAILED).");

            // Allowed transitions: PENDING -> VERIFIED/FAILED
            if (caseEntity.Status != KycStatus.PENDING)
                throw new InvalidOperationException($"Unsupported transition from {caseEntity.Status}.");

            var oldStatus = caseEntity.Status;

            caseEntity.Status = target;
            caseEntity.ProviderRef = dto.ProviderRef ?? caseEntity.ProviderRef;

            // Update evidence refs if provided
            if (dto.EvidenceRefs is not null)
            {
                caseEntity.EvidenceRefsJson = JsonSerializer.Serialize(dto.EvidenceRefs);
            }

            // CheckedAt must be set on terminal states
            if (target == KycStatus.VERIFIED || target == KycStatus.FAILED)
            {
                caseEntity.CheckedAt = dto.CheckedAt ?? DateTime.UtcNow;
            }

            await _kycCases.UpdateAsync(caseEntity, ct);
            await _kycCases.SaveChangesAsync(ct);

            // Reflect on Customer aggregate (VERIFIED only)
            if (target == KycStatus.VERIFIED)
            {
                var customer = await _customers.GetByIdAsync(caseEntity.CustomerId, ct);
                if (customer is not null)
                {
                    customer.Status = CustomerStatus.VERIFIED;
                    await _customers.UpdateAsync(customer, ct);
                    await _customers.SaveChangesAsync(ct);
                }
            }

            // Publish KYC status changed event
            await _publish.Publish<KycStatusChanged>(new
            {
                KycCaseId = caseEntity.KycCaseId,
                caseEntity.CustomerId,
                OldStatus = oldStatus,
                NewStatus = caseEntity.Status,
                ProviderRef = caseEntity.ProviderRef,
                CheckedAtUtc = caseEntity.CheckedAt
            }, ct);

            // (Optional) Audit: status change
            // await _audit.RecordAsync(...)

            return caseEntity;
        }

        public Task<KycCase?> GetByIdAsync(Guid caseId, CancellationToken ct) =>
            _kycCases.GetByIdAsync(caseId, ct);

        public Task<IReadOnlyList<KycCase>> GetByCustomerAsync(Guid customerId, CancellationToken ct) =>
            _kycCases.GetByCustomerAsync(customerId, ct);

        private static KycStatus MapDtoStatus(KycStatusDto dto) => dto switch
        {
            KycStatusDto.PENDING => KycStatus.PENDING,
            KycStatusDto.VERIFIED => KycStatus.VERIFIED,
            KycStatusDto.FAILED => KycStatus.FAILED,
            _ => throw new ValidationException("Unsupported KYC status.")
        };
    }
}
