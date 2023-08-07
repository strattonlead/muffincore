using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Muffin.Common.Util
{
    public static class DictionaryHelper
    {
        public static Dictionary<K, V> Update<K, V>(this Dictionary<K, V> me, IEnumerable<KeyValuePair<K, V>> other)
        {
            foreach (var x in other)
            {
                me[x.Key] = x.Value;
            }
            return me;
        }

        public static Dictionary<K, V> Merge<K, V>(this IEnumerable<IEnumerable<KeyValuePair<K, V>>> keyValuePairs)
        {
            var result = new Dictionary<K, V>();
            foreach (var keyValuePair in keyValuePairs)
            {
                result.Update(keyValuePair);
            }
            return result;
        }

        public static bool Equals<TKey, TValue>(IDictionary<TKey, TValue> dict, IDictionary<TKey, TValue> dict2)
        {
            bool equal = false;
            if (dict.Count == dict2.Count) // Require equal count.
            {
                equal = true;
                foreach (var pair in dict)
                {
                    if (dict2.TryGetValue(pair.Key, out TValue value))
                    {
                        if (value == null && pair.Value == null)
                        {
                            continue;
                        }

                        if ((value != null && pair.Value == null) || (value == null && pair.Value != null))
                        {
                            equal = false;
                            break;
                        }

                        if (!value.Equals(pair.Value))
                        {
                            equal = false;
                            break;
                        }
                    }
                    else
                    {
                        equal = false;
                        break;
                    }
                }
            }
            return equal;
        }

        public static Dictionary<string, object> ToDictionary(object obj)
        //where T : class
        {
            if (obj == null)
                return null;

            return obj.GetType()
                .GetProperties()
                .ToDictionary(x => x.Name, x => x.GetValue(obj));
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            return GetValueOrDefault(dict, key, default(TValue));
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        {
            TValue value;
            if (dict.TryGetValue(key, out value))
                return value;
            return defaultValue;
        }

        public static Dictionary<TKey, TValue> DictionaryIgnoringKeys<TKey, TValue>(this Dictionary<TKey, TValue> dict, params TKey[] keys)
        {
            return dict.Keys.Except(keys).ToDictionary(x => x, x => dict[x]);
        }

        public static Dictionary<TKey, TValue> CastDictionary<TKey, TValue>(this IDictionary dict)
        {
            return dict.Keys.Cast<TKey>().ToDictionary(x => x, x => (TValue)dict[x]);
        }

        public static bool GetBoolOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            var value = dict[key];
            return Convert.ToBoolean(value);
        }

        public static string GetStringOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            var value = dict[key];
            return Convert.ToString(value);
        }

        public static long GetLongOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            var value = dict[key];
            return Convert.ToInt64(value);
        }

        public static int GetIntOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            var value = dict[key];
            return Convert.ToInt32(value);
        }

        public static DateTime GetDateOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            var value = dict[key];
            return Convert.ToDateTime(value);
        }

        public static Dictionary<string, TValue> ReplaceKeys<TValue>(this Dictionary<string, TValue> dict, string str, string replace)
        {
            var keys = dict.Keys.Where(x => x.Contains(str)).ToArray();
            var result = new Dictionary<string, TValue>();
            foreach (var key in keys)
            {
                var item = dict[key];
                result.Add(key.Replace(str, replace), item);
            }
            return result;
        }

        public static Dictionary<TKey, object> Merge<TKey>(this Dictionary<TKey, object> target, Dictionary<TKey, object> source)
        {
            var result = target.ToDictionary(x => x.Key, x => x.Value);
            foreach (var key in source.Keys)
            {
                if (result.TryGetValue(key, out object targetValue))
                {
                    var innerSourceDictionary = source[key] as Dictionary<TKey, object>;
                    var innerTargetDictionary = targetValue as Dictionary<TKey, object>;

                    if (innerSourceDictionary != null && innerTargetDictionary != null)
                    {
                        result[key] = Merge(innerTargetDictionary, innerSourceDictionary);
                    }
                    else if (innerSourceDictionary == null && innerTargetDictionary == null)
                    {
                        result[key] = source[key];
                    }
                    else if (innerTargetDictionary != null)
                    {
                        innerTargetDictionary[key] = source[key];
                    }
                }
                else
                {
                    result[key] = source[key];
                }
            }

            return result;
        }
    }

    public class ExpirableDictionary<TKey, TValue>
    {
        #region Properties

        private ConcurrentDictionary<TKey, ExpirableEntry<TKey, TValue>> _dict = new ConcurrentDictionary<TKey, ExpirableEntry<TKey, TValue>>();
        public TimeSpan DefaultTimeout { get; private set; }

        #endregion

        #region Constructor

        public ExpirableDictionary()
           : this(TimeSpan.FromMinutes(1))
        { }

        public ExpirableDictionary(TimeSpan defaultTimeout)
        {
            if (defaultTimeout < TimeSpan.Zero)
            {
                throw new ArgumentException("Timespan must be greater than 0");
            }

            DefaultTimeout = defaultTimeout;
        }

        #endregion

        #region Dictionary

        public bool Any()
        {
            return _dict.Any();
        }

        public int Count
        {
            get
            {
                return _dict.Count;
            }
        }

        public TValue[] GetMany(IEnumerable<TKey> keys)
        {
            return GetMany(keys, out _);
        }

        public TValue[] GetMany(IEnumerable<TKey> keys, out TKey[] matchedKeys)
        {
            matchedKeys = keys
                .Where(x => _dict.ContainsKey(x))
                .ToArray();
            return matchedKeys
                .Select(x => _dict[x].Value)
                .ToArray();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_dict.TryGetValue(key, out ExpirableEntry<TKey, TValue> entry))
            {
                value = entry.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public void Add(TKey key, TValue value)
        {
            Add(key, value, DefaultTimeout);
        }

        public void Add(TKey key, TValue value, TimeSpan timeout)
        {
            var entry = new ExpirableEntry<TKey, TValue>(key, value, _dict, timeout);
            if (!_dict.TryAdd(key, entry))
            {
                _dict[key] = entry;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            if (_dict.TryRemove(key, out ExpirableEntry<TKey, TValue> entry))
            {
                value = entry.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public TValue this[TKey key]
        {
            get
            {
                return _dict[key] != null ? _dict[key].Value : default(TValue);
            }
            set
            {
                Add(key, value);
            }
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            return _dict.ToDictionary(x => x.Key, x => x.Value.Value);
        }

        #endregion

        #region Helpers

        public IEnumerable<TValue> Values => _dict.Values.Select(x => x.Value);

        #endregion
    }

    public class ExpirableEntry<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public System.Timers.Timer Timer { get; set; }
        public ConcurrentDictionary<TKey, ExpirableEntry<TKey, TValue>> RefOfValue { get; set; }

        public ExpirableEntry(TKey key, TValue value, ConcurrentDictionary<TKey, ExpirableEntry<TKey, TValue>> refOfValue, TimeSpan timeout)
        {
            Key = key;
            Value = value;
            RefOfValue = refOfValue;
            Timer = new System.Timers.Timer(timeout.TotalMilliseconds);
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var retry = 0;
            while (!RefOfValue.TryRemove(Key, out _) && retry < 10)
            {
                retry++;
            }
        }
    }

    public class Map<T1, T2>
    {
        private Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
        private Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();

        public Map()
        {
            this.Forward = new Indexer<T1, T2>(_forward);
            this.Reverse = new Indexer<T2, T1>(_reverse);
        }

        public class Indexer<T3, T4>
        {
            private Dictionary<T3, T4> _dictionary;
            public Indexer(Dictionary<T3, T4> dictionary)
            {
                _dictionary = dictionary;
            }
            public T4 this[T3 index]
            {
                get { return _dictionary[index]; }
                set { _dictionary[index] = value; }
            }
        }

        public void Add(T1 t1, T2 t2)
        {
            _forward.Add(t1, t2);
            _reverse.Add(t2, t1);
        }

        public Indexer<T1, T2> Forward { get; private set; }
        public Indexer<T2, T1> Reverse { get; private set; }

        public bool TryGetValue(T1 key, out T2 value)
        {
            return _forward.TryGetValue(key, out value);
        }

        public bool TryGetValue(T2 key, out T1 value)
        {
            return _reverse.TryGetValue(key, out value);
        }

        public T2 this[T1 index]
        {
            get { return Forward[index]; }
            set { Add(index, value); }
        }

        public T1 this[T2 index]
        {
            get { return Reverse[index]; }
            set { Add(value, index); }
        }
    }
}
