using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Muffin.Common.Util
{
    public abstract class PersistentUniqueId { }
    public class PersistentUniqueId<T> : PersistentUniqueId
    {
        #region Properties

        public T Id { get; set; }
        public DateTime CreateDateUtc { get; set; }

        #endregion

        #region Singleton

        private PersistentUniqueId() { }

        private static Dictionary<Type, PersistentUniqueId> _ids = new Dictionary<Type, PersistentUniqueId>();
        private static object _instanceLock = new object();
        public static PersistentUniqueId<T> Current
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_ids.ContainsKey(typeof(T)))
                    {
                        return load();
                    }
                }
                return null;
            }
        }

        #endregion

        #region Loading and Generation

        private static string ID_FILE_NAME { get => $"unique_{typeof(T).Name}_id.json"; }
        private static string ID_FILE_PATH { get => AppDomain.CurrentDomain.BaseDirectory + ID_FILE_NAME; }

        private static PersistentUniqueId<T> load()
        {
            string data;
            if (File.Exists(ID_FILE_PATH))
            {
                data = File.ReadAllText(ID_FILE_PATH);
                return JsonConvert.DeserializeObject<PersistentUniqueId<T>>(data);
            }

            var id = new PersistentUniqueId<T>();

            if (typeof(T) == typeof(int))
            {
                id.Id = (T)(object)Math.Abs(SecureRandom.NextInt());
            }
            else if (typeof(T) == typeof(long))
            {
                id.Id = (T)(object)SecureRandom.AbsLong();
            }
            else if (typeof(T) == typeof(Guid))
            {
                id.Id = (T)(object)Guid.NewGuid();
            }
            else if (typeof(T) == typeof(string))
            {
                id.Id = (T)(object)SecureRandom.NextHexString(64);
            }

            id.CreateDateUtc = DateTime.UtcNow;

            data = JsonConvert.SerializeObject(id);
            File.WriteAllText(ID_FILE_PATH, data);

            return id;
        }

        #endregion

    }
}
