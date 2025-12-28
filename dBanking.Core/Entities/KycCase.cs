using System.ComponentModel.DataAnnotations;

namespace dBanking.Core.Entities
{
    public class KycCase
    {
        [Key]
        public Guid KycCaseId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CustomerId { get; set; }

        // Navigation back to Customer
        public Customer Customer { get; set; } = default!;

        [Required]
        public KycStatus Status { get; set; } = KycStatus.PENDING;

        public string? ProviderRef { get; set; }

        /// <summary>
        /// Evidence references (doc IDs). Stored as JSON (string) for portability.
        /// If you're on PostgreSQL, you can map to jsonb and use List&lt;string&gt; with a value converter.
        /// </summary>
        public string? EvidenceRefsJson { get; set; }

        /// <summary>
        /// Consent text displayed to the customer (versioned in provider/backend).
        /// </summary>
        [Required]
        public string ConsentText { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when consent was accepted.
        /// </summary>
        [Required]
        public DateTime AcceptedAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CheckedAt { get; set; }
    }

}
