using Newtonsoft.Json;
using System;

namespace Muffin.Hetzner.Robot.Api.Models
{
    public class StorageBox
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "login")]
        public string Login { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "product")]
        public string Product { get; set; }

        [JsonProperty(PropertyName = "cancelled")]
        public bool Cancelled { get; set; }

        [JsonProperty(PropertyName = "locked")]
        public bool Locked { get; set; }

        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }

        [JsonProperty(PropertyName = "linked_server")]
        public long? LinkedServer { get; set; }

        [JsonProperty(PropertyName = "paid_until")] // "2015-10-23"
        public DateTime? PaidUntil { get; set; }

        [JsonProperty(PropertyName = "disk_quota")]
        public long? DiskQuota { get; set; }

        [JsonProperty(PropertyName = "disk_usage")]
        public long? DiskUsage { get; set; }

        [JsonProperty(PropertyName = "disk_usage_data")]
        public long? DiskUsageData { get; set; }

        [JsonProperty(PropertyName = "disk_usage_snapshots")]
        public long? DiskUsageSnapshots { get; set; }

        [JsonProperty(PropertyName = "webdav")]
        public bool? Webdav { get; set; }

        [JsonProperty(PropertyName = "samba")]
        public bool? Samba { get; set; }

        [JsonProperty(PropertyName = "ssh")]
        public bool? Ssh { get; set; }

        [JsonProperty(PropertyName = "external_reachability")]
        public bool? ExternalReachability { get; set; }

        [JsonProperty(PropertyName = "zfs")]
        public bool? Zfs { get; set; }

        [JsonProperty(PropertyName = "server")]
        public string Server { get; set; }

        [JsonProperty(PropertyName = "host_system")]
        public string HostSystem { get; set; }
    }

    public class StorageBoxWrapper
    {
        [JsonProperty(PropertyName = "storagebox")]
        public StorageBox StorageBox { get; set; }
    }
}
