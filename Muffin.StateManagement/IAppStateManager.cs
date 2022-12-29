using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Muffin.StateManagement
{
    public interface IAppStateManager<TAppState>
    {
        Task<TAppState> ReloadAppStateAsync();
        Task<TAppState> GetStateAsync(string checksum = null);
        Task<PartialAppState> GetFullStateChangeAsync();
        Dictionary<long, TAppState> MergeStates<T>(T item, out Dictionary<long, PartialAppState> partialAppStates);
        Dictionary<long, TAppState> MergeStatesRange<T>(IEnumerable<T> items, out Dictionary<long, PartialAppState> partialAppStates);
    }

    public static class AppStateManagerExtensions
    {
        public static Dictionary<long, TAppState> DefaultMergeStrategy<TAppState, T>(this IAppStateManager<TAppState> appStateManager, IEnumerable<T> items, out Dictionary<long, PartialAppState> partialAppStates)
        {
            Dictionary<long, TAppState> result = null;
            partialAppStates = null;
            if (items != null)
            {
                partialAppStates = new Dictionary<long, PartialAppState>();
                foreach (var item in items)
                {
                    var temp = new Dictionary<long, PartialAppState>();
                    result = appStateManager.MergeStates(item, out temp);

                    if (temp != null)
                    {
                        foreach (var key in temp.Keys)
                        {
                            if (!partialAppStates.ContainsKey(key))
                            {
                                partialAppStates[key] = temp[key];
                            }
                            else
                            {
                                partialAppStates[key].Merge(temp[key]);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static void AddAppStateManager<TAppState, TImplementation>(this IServiceCollection services)
            where TImplementation : class, IAppStateManager<TAppState>
        {
            services.AddAppStateManager<TAppState, TImplementation>(null);
        }

        public static void AddAppStateManager<TAppState, TImplementation>(this IServiceCollection services, Action<AppStateManagerOptionsBuilder<TAppState>> optionsBuilderAction)
            where TImplementation : class, IAppStateManager<TAppState>
        {
            if (optionsBuilderAction != null)
            {
                var optionsBuilder = new AppStateManagerOptionsBuilder<TAppState>();
                optionsBuilderAction(optionsBuilder);
                var options = optionsBuilder.Build();
                services.AddSingleton(options);
            }
            else
            {
                services.AddSingleton(new AppStateManagerOptions<TAppState>());
            }

            services.AddSingleton<AppStateCache<TAppState>>();
            services.AddSingleton<AppStateEvents<TAppState>>();
            services.AddScoped<IAppStateManager<TAppState>, TImplementation>();
        }
    }
}
