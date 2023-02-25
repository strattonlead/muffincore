using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Muffin.WebSockets.Server.V2.PubSub
{
    public interface IHasTopic
    {
        string Topic { get; }
    }

    public class Subscription : IEquatable<string>, IHasTopic, IEqualityComparer<Subscription>
    {
        #region Properties

        public long? TenantId { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();

        #endregion

        #region Channel ID

        public string Topic
        {
            get
            {
                if (TenantId.HasValue && Params != null && Params.Any())
                {
                    return $"{TenantId}/{Controller}/{Action}/{JsonConvert.SerializeObject(Params)}";
                }

                if (TenantId.HasValue)
                {
                    return $"{TenantId}/{Controller}/{Action}";
                }

                if (Params != null && Params.Any())
                {
                    return $"{Controller}/{Action}/{JsonConvert.SerializeObject(Params)}";
                }

                return $"{TenantId}/{Controller}/{Action}/{JsonConvert.SerializeObject(Params)}";
            }
        }

        #endregion

        #region IEquatable<string>

        public bool Equals(string other)
        {
            return Topic == other;
        }

        #endregion

        #region IEqualityComparer<Subscription>

        public bool Equals(Subscription x, Subscription y)
        {
            return x?.Topic == y?.Topic;
        }

        public int GetHashCode([DisallowNull] Subscription obj)
        {
            return obj.Topic.GetHashCode();
        }

        #endregion
    }
}
