using System.Collections.Generic;
using System.Linq;

namespace Muffin.Common.Util
{
    public static class LookupHelper
    {
        public static bool TryGetValue<TKey, TElement>(this ILookup<TKey, TElement> lookup, TKey key, out IEnumerable<TElement> element)
        {
            if (lookup.Contains(key))
            {
                element = lookup[key];
                return true;
            }

            element = null;
            return false;
        }
    }
}
