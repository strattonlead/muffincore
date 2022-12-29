using Muffin.Shopify.Abstraction;
using Newtonsoft.Json;
using System;

namespace Muffin.Shopify
{
    public class ShopifyCredential : IShopifyCredential
    {
        [JsonProperty(PropertyName = "minAccessDateUtc")]
        public DateTime? MinAccessDateUtc { get; set; }

        [JsonProperty(PropertyName = "shopUrl")]
        public string ShopUrl { get; set; }

        [JsonProperty(PropertyName = "apiPassword")]
        public string ApiPassword { get; set; }

        [JsonProperty(PropertyName = "secretKey")]
        public string SecretKey { get; set; }

        [JsonProperty(PropertyName = "accessToken")]
        public string AccessToken { get; set; }
    }
}
