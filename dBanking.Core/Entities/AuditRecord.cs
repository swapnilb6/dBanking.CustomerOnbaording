using System.ComponentModel.DataAnnotations;
namespace dBanking.Core.Entities
{

    
    /// <summary>
    /// Immutable audit log (append-only): records who did what, when,
    /// and the before/after JSON snapshots.
    /// </summary>
    public class AuditRecord
    {
        [Key]
        public Guid AuditId { get; set; } = Guid.NewGuid();

        [Required]
        public AuditEntityType EntityType { get; set; }

        /// <summary>
        /// The primary key of the target entity (e.g., CustomerId or KycCaseId).
        /// </summary>
        [Required]
        public Guid TargetEntityId { get; set; }

        [Required]
        public AuditAction Action { get; set; }

        /// <summary>
        /// Actor identifier (e.g., token subject, email, system).
        /// </summary>
        [Required, StringLength(256)]
        public string Actor { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp in UTC.
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Snapshot before mutation (nullable for CREATE).
        /// </summary>
        public string? BeforeJson { get; set; }

        /// <summary>
        /// Snapshot after mutation (required).
        /// </summary>
        [Required]
        public string AfterJson { get; set; } = string.Empty;

        /// <summary>
        /// Optional correlation ID for tracing a request across services.
        /// </summary>
        public string? CorrelationId { get; set; }
    }

}
