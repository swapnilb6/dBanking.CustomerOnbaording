using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace dBanking.Core.DTOS
{
    public sealed record KycStatusUpdateRequestDto(
        [property: Required] Guid CustomerId,
        [property: Required] Guid KycCaseId,
        [property: Required] KycStatusDto Status,
        string? ProviderRef = null,
        List<string>? EvidenceRefs = null,
        DateTime? CheckedAt = null
    );

}
