using Microsoft.Extensions.DependencyInjection;
using Muffin.Common.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Muffin.StateManagement
{
    public class AppStateCache<TAppState>
    {
        #region Properties

        private readonly ExpirableDictionary<long, TAppState> Cache;
        private readonly AppStateEvents<TAppState> Events;
        private readonly AppStateManagerOptions<TAppState> Options;

        #endregion

        #region Constructor

        public AppStateCache(IServiceProvider serviceProvider)
        {
            Options = serviceProvider.GetRequiredService<AppStateManagerOptions<TAppState>>();
            Cache = new ExpirableDictionary<long, TAppState>(Options.Timeout);
            Events = serviceProvider.GetRequiredService<AppStateEvents<TAppState>>();
        }

        #endregion

        #region Cache

        public long[] Keys { get => Values.Keys.ToArray(); }

        public Dictionary<long, TAppState> Values { get => Cache.ToDictionary(); }

        public void TryRemove(long key)
        {
            Cache.TryRemove(key, out _);
        }

        public bool TryGetValue(long key, [MaybeNullWhen(false)] out TAppState value)
        {
            return Cache.TryGetValue(key, out value);
        }

        public TAppState this[long key]
        {
            get => Cache[key];
            set
            {
                Cache[key] = value;
                Events.OnAppStateChangedEvent?.Invoke(this, new AppStateEventArgs<TAppState>(key, value));
            }
        }

        public Dictionary<long, TAppState> ToDictionary() => Cache.ToDictionary();

        #endregion
    }
}
