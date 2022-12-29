using System.Collections.Generic;

namespace Muffin.Common.Util
{
    public class GlobalCache
    {
        #region Sinlgeton

        private GlobalCache() { }
        private static GlobalCache _instance = null;
        private static object _lock = new object();
        public static GlobalCache Instance
        {
            get
            {
                lock (_lock)
                    if (_instance == null)
                        _instance = new GlobalCache();
                return _instance;
            }
        }

        #endregion

        #region Properties

        private Dictionary<string, object> _cache = new Dictionary<string, object>();

        #endregion

        #region Helper

        public object Get(string key)
        {
            if (_cache.ContainsKey(key))
                return _cache[key];
            return null;
        }

        public void Add(string key, object value)
        {
            _cache.Add(key, value);
            OnAdd?.Invoke(key, value);
        }

        public void Remove(string key)
        {
            if (_cache.ContainsKey(key))
            {
                _cache.Remove(key);
                OnRemove?.Invoke(key, true);
            }
            else
                OnRemove?.Invoke(key, false);
        }

        public void Clear()
        {
            _cache.Clear();
            OnClear?.Invoke();
        }

        #endregion

        #region Events

        public CacheClearEvent OnClear { get; set; }
        public CacheAddEvent OnAdd { get; set; }
        public CacheRemoveEvent OnRemove { get; set; }

        public delegate void CacheClearEvent();
        public delegate void CacheAddEvent(string key, object value);
        public delegate void CacheRemoveEvent(string key, bool success);

        #endregion
    }
}