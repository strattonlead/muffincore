using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Muffin.EntityFrameworkCore.Entity;
using Newtonsoft.Json;

namespace Muffin.EntityFrameworkCore.AuditTrail
{
    // https://codewithmukesh.com/blog/audit-trail-implementation-in-aspnet-core/
    [Index(nameof(DateTimeUtc))]
    [Index(nameof(AuditType))]
    [Index(nameof(SingleId))]
    //[Index(nameof(UserId))]
    public class AuditEntity : BaseEntity
    {
        [JsonProperty(PropertyName = "dateTimeUtc", NullValueHandling = NullValueHandling.Include)]
        public DateTime DateTimeUtc { get; set; }

        [JsonProperty(PropertyName = "auditType", NullValueHandling = NullValueHandling.Include)]
        public AuditType AuditType { get; set; }

        [JsonProperty(PropertyName = "identityId", NullValueHandling = NullValueHandling.Include)]
        public long? IdentityId { get; set; }

        [JsonProperty(PropertyName = "singleId", NullValueHandling = NullValueHandling.Include)]
        public long? SingleId { get; set; }

        [JsonProperty(PropertyName = "tableName", NullValueHandling = NullValueHandling.Include)]
        public string TableName { get; set; }

        [JsonProperty(PropertyName = "keyValues", NullValueHandling = NullValueHandling.Include)]
        public Dictionary<string, object> KeyValues { get; set; } = new Dictionary<string, object>();

        [JsonProperty(PropertyName = "oldValues", NullValueHandling = NullValueHandling.Include)]
        public Dictionary<string, object> OldValues { get; set; } = new Dictionary<string, object>();

        [JsonProperty(PropertyName = "newValues", NullValueHandling = NullValueHandling.Include)]
        public Dictionary<string, object> NewValues { get; set; } = new Dictionary<string, object>();

        [JsonProperty(PropertyName = "affectedColumns", NullValueHandling = NullValueHandling.Include)]
        public List<string> AffectedColumns { get; set; } = new List<string>();
    }

    public class AuditEntityEntityTypeConfiguration : IEntityTypeConfiguration<AuditEntity>
    {
        public void Configure(EntityTypeBuilder<AuditEntity> builder)
        {
            builder.Property(x => x.KeyValues).HasConversion(x => _serialize(x), x => _deserialize<Dictionary<string, object>>(x));
            builder.Property(x => x.OldValues).HasConversion(x => _serialize(x), x => _deserialize<Dictionary<string, object>>(x));
            builder.Property(x => x.NewValues).HasConversion(x => _serialize(x), x => _deserialize<Dictionary<string, object>>(x));
            builder.Property(x => x.AffectedColumns).HasConversion(x => _serialize(x), x => _deserialize<List<string>>(x));
        }

        private T _deserialize<T>(string s)
        {
            if (s == null)
            {
                return default;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(s);
            }
            catch { }
            return default;
        }

        private string _serialize(Dictionary<string, object> dict)
        {
            if (dict == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(dict);
        }

        private string _serialize(List<string> list)
        {
            if (list == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(list);
        }
    }
}
