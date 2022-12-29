using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Muffin.Services
{
    public class MemoryCacheFactory
    {
        #region properties

        public readonly Dictionary<string, IMemoryCache> Caches = new Dictionary<string, IMemoryCache>();

        #endregion

        #region Factory

        public IMemoryCache Get(string identifier)
        {
            if (identifier == null)
            {
                return null;
            }

            if (!Caches.TryGetValue(identifier, out var cache))
            {
                cache = new MemoryCache(new MemoryCacheOptions() { });
            }
            return cache;
        }

        public IMemoryCache Get(Type type)
        {
            return Get(type.AssemblyQualifiedName);
        }

        public IMemoryCache Get<T>()
        {
            return Get(typeof(T));
        }

        #endregion
    }

    public static class MemoryCacheFactoryExtensions
    {
        public static void AddMemoryCacheFactory(this IServiceCollection services)
        {
            services.AddSingleton<MemoryCacheFactory>();
        }
    }
}
