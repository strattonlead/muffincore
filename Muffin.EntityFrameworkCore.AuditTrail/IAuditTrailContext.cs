using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Muffin.EntityFrameworkCore.AuditTrail
{
    public interface IAuditTrailContext
    {
        DbSet<AuditEntity> Audits { get; set; }
    }
}
