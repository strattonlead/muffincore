using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Muffin.StateManagement
{
    [JsonConverter(typeof(PartialAppStateJsonConverter))]
    public class PartialAppState
    {
        public Dictionary<string, AppStateChange> Changes { get; set; } = new Dictionary<string, AppStateChange>();

        public PartialAppState Merge(PartialAppState other)
        {
            if (other != null)
            {
                foreach (var key in other.Changes.Keys)
                {
                    Changes[key] = other.Changes[key];
                }
            }
            return this;
        }

        public PartialAppState Add(string path, object value)
        {
            return Update(path, value);
        }

        public PartialAppState Update(string path, object value)
        {
            Changes[path] = AppStateChange.Add(path, value);
            return this;
        }

        public PartialAppState Delete(string path)
        {
            Changes[path] = AppStateChange.Delete(path);
            return this;
        }

        public PartialAppState UpdateTranslation(Dictionary<string, string> dict)
        {
            if (dict != null)
            {
                foreach (var pair in dict)
                {
                    Update(pair.Key, pair.Value);
                }
            }
            return this;
        }

        public PartialAppState UpdateTranslations(IEnumerable<Dictionary<string, string>> dicts)
        {
            foreach (var dict in dicts)
            {
                UpdateTranslation(dict);
            }
            return this;
        }
    }

    public static class PartialAppStateExtensions
    {
        public static PartialAppState MergeRange(this IEnumerable<PartialAppState> partialAppStates)
        {
            if (partialAppStates == null)
            {
                return new PartialAppState();
            }

            var partialAppState = partialAppStates.FirstOrDefault();
            var others = partialAppStates.Skip(1).ToArray();
            foreach (var other in others)
            {
                partialAppState.Merge(other);
            }
            return partialAppState;
        }
    }
}
