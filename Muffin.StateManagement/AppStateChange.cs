using Newtonsoft.Json;

namespace Muffin.StateManagement
{
    public class AppStateChange
    {
        #region Properties

        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }

        [JsonProperty(PropertyName = "value")]
        public object Value { get; set; }

        #endregion

        #region Helper

        public static AppStateChange Add(string path, object value)
        {
            return new AppStateChange()
            {
                Path = path,
                Value = value
            };
        }

        public static AppStateChange Delete(string path)
        {
            return new AppStateChange() { Path = path };
        }

        #endregion
    }
}
