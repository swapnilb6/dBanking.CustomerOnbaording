using dBanking.Core.Repository_Contracts;
using System.Text.Json;
using dBanking.Core.DTOS;
using dBanking.Core.Entities;
using dBanking.Core.ServiceContracts;

namespace dBanking.Core.Services
{
  
    public sealed class AuditService : IAuditService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private readonly IAuditRepository _repo;
        //private readonly IClock _clock; // optional abstraction for time

        public AuditService(IAuditRepository repo)
        {
            _repo = repo;

        }

        public Task RecordAsync(AuditEntryDto entry, CancellationToken ct = default)
        {
            var beforeJson = entry.BeforeSnapshot is null ? null : JsonSerializer.Serialize(entry.BeforeSnapshot, JsonOptions);
            var afterJson = entry.AfterSnapshot is null ? null : JsonSerializer.Serialize(entry.AfterSnapshot, JsonOptions);

            var audit = new AuditRecord
            {
                EntityType = entry.EntityType,
                Action = entry.Action,
                TargetEntityId = entry.TargetEntityId,
                RelatedEntityId = entry.RelatedEntityId,
                Actor = entry.Actor,
                CorrelationId = entry.CorrelationId,
                Timestamp = DateTime.FromFileTimeUtc(DateTime.UtcNow.ToFileTimeUtc()), // or DateTimeOffset.UtcNow
                BeforeJson = beforeJson,
                AfterJson = afterJson,
                Source = entry.Source,
                Environment = entry.Environment
            };

            return _repo.AddAsync(audit, ct);
        }

        
    }

}
