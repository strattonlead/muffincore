using System.Collections.Generic;

namespace Muffin.EntityFrameworkCore.AuditTrail
{
    public class AuditTrailCache
    {
        public List<AuditEntry> Changes { get; internal set; } = new List<AuditEntry>();
    }
}
