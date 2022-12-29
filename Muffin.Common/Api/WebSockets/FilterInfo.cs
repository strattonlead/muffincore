using Muffin.Common.Util;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;

namespace Muffin.Common.Api.WebSockets
{
    public class FilterInfo
    {
        #region Properties

        [JsonProperty(PropertyName = "conjuctionOperator")]
        public ConjonctionType ConjonctionOperator { get; set; }

        [JsonProperty(PropertyName = "property")]
        public string Property { get; set; }

        [JsonProperty(PropertyName = "relationalOperator")]
        public RelationalType RelationalOperator { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "isValueAsList")]
        public bool IsValueAsList { get; set; }

        #endregion

        #region Helper

        public PropertyInfo GetPropertyInfo<T>()
        {
            return typeof(T).GetProperties().FirstOrDefault(x => string.Equals(Property, x.Name));
        }

        #endregion
    }
}
