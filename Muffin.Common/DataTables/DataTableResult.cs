using Newtonsoft.Json;

namespace Muffin.Common.DataTables
{
    public class DataTableResult<T>
    {
        [JsonProperty("description")]
        public DataTableDescription<T> TableDescription { get; set; } // kann man das so serialisieren??

        [JsonProperty("data")]
        public DataTableModel TableData { get; set; }
    }
}
