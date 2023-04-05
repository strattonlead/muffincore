using Microsoft.AspNetCore.Http;
using System;

namespace Muffin.WebSockets.Server.Models
{
    public class WebSocketConnectionId : IEquatable<WebSocketConnectionId>, IEquatable<string>
    {
        #region Properties

        public string Identity { get; set; }
        public string RandomId { get; set; }
        public bool IsAnonymous => string.IsNullOrWhiteSpace(Identity);
        public long? TenantId { get; set; }

        #endregion

        #region IEquatable

        public bool Equals(WebSocketConnectionId other)
        {
            return ToString() == other?.ToString();
        }

        public bool Equals(string other)
        {
            return ToString() == other;
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is WebSocketConnectionId)
            {
                return Equals(other as WebSocketConnectionId);
            }
            else if (other is string)
            {
                return Equals(other as string);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(WebSocketConnectionId obj1, WebSocketConnectionId obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            if (ReferenceEquals(obj1, null))
            {
                return false;
            }

            if (ReferenceEquals(obj2, null))
            {
                return false;
            }

            return obj1.Equals(obj2);
        }
        public static bool operator !=(WebSocketConnectionId obj1, WebSocketConnectionId obj2) => !(obj1 == obj2);

        public static bool operator ==(WebSocketConnectionId obj1, string obj2)
        {
            if (obj1 == (WebSocketConnectionId)null && obj2 != null)
            {
                return false;
            }

            if (ReferenceEquals(obj1, null) && ReferenceEquals(obj2, null))
            {
                return true;
            }

            if (ReferenceEquals(obj1, null))
            {
                return false;
            }

            if (ReferenceEquals(obj2, null))
            {
                return false;
            }

            return obj1.ToString().Equals(obj2);
        }
        public static bool operator !=(WebSocketConnectionId obj1, string obj2) => !(obj1 == obj2);

        #endregion

        #region Helper

        public static implicit operator WebSocketConnectionId(string s)
        {
            return Parse(s);
        }

        public static WebSocketConnectionId Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            if (s.Contains('-'))
            {
                var parts = s.Split('-');
                if (parts.Length == 2)
                {
                    return new WebSocketConnectionId()
                    {
                        Identity = parts[0],
                        RandomId = parts[1]
                    };
                }
            }

            return new WebSocketConnectionId()
            {
                RandomId = s
            };
        }

        public static WebSocketConnectionId NewConnectionId(HttpContext context, long? tenantId)
        {
            if (context?.User?.Identity != null && context.User.Identity.IsAuthenticated && !string.IsNullOrWhiteSpace(context.User.Identity.Name))
            {
                return new WebSocketConnectionId()
                {
                    RandomId = Guid.NewGuid().ToString().Substring(0, 8),
                    Identity = context.User.Identity.Name,
                    TenantId = tenantId
                };
            }

            return new WebSocketConnectionId()
            {
                RandomId = Guid.NewGuid().ToString().Substring(0, 8),
                Identity = context.User.Identity.Name,
                TenantId = tenantId
            };
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Identity))
            {
                return $"{Identity}-{RandomId}";
            }
            return RandomId;
        }

        #endregion
    }
}
