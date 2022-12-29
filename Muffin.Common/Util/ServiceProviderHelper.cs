using System;

namespace Muffin.Common.Util
{
    public static class ServiceProviderHelper
    {
        public static T TryGetService<T>(this IServiceProvider serviceProvider)
        where T : class
        {
            T service = null;
            if (serviceProvider != null)
            {
                service = serviceProvider.GetService(typeof(T)) as T;
            }
            return service;
        }

        public static object TryGetService(this IServiceProvider serviceProvider, Type type)
        {
            object service = null;
            if (serviceProvider != null)
            {
                try
                {
                    service = serviceProvider.GetService(type);
                }
                catch { }
            }
            return service;
        }
    }
}
