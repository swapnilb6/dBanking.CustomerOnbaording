using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dBanking.Core.DTOS
{
    public sealed record AuditRecordDto(
        Guid AuditId,
        [property: Required] AuditEntityTypeDto EntityType,
        [property: Required] AuditActionDto Action,
        [property: Required, StringLength(256)] string Actor,
        [property: Required] DateTime Timestamp,
        string? BeforeJson,
        [property: Required] string AfterJson
    );

}
