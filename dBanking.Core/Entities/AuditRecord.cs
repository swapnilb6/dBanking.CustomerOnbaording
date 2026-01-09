using System.ComponentModel.DataAnnotations;
namespace dBanking.Core.Entities
{
    public sealed class AuditRecord
    {
        public Guid AuditRecordId { get; init; } = Guid.NewGuid();

        public string EntityType { get; init; } = default!;           // e.g., "Customer", "KycCase"
        public string Action { get; init; } = default!;                // e.g., "CustomerCreated", "KycStarted"

        public Guid? TargetEntityId { get; init; }                     // e.g., customerId or kycCaseId
        public Guid? RelatedEntityId { get; init; }                    // optional (e.g., kycCaseId when action on customer)

        public string Actor { get; init; } = default!;                 // user/service principal / client id
        public string? CorrelationId { get; init; }                    // trace across HTTP & AMQP

        public DateTimeOffset Timestamp { get; init; }                 // UTC
        public string? BeforeJson { get; init; }                       // jsonb
        public string? AfterJson { get; init; }                        // jsonb

        // Optional categorization
        public string? Source { get; init; }                           // "API", "Consumer", "Scheduler"
        public string? Environment { get; init; }                      // "dev", "qa", "prod"
    }
}
