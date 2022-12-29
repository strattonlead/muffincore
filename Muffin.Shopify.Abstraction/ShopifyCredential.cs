using System;

namespace Muffin.Shopify.Abstraction
{
    public interface IShopifyCredential
    {
        DateTime? MinAccessDateUtc { get; set; }
        string ShopUrl { get; set; }
        string ApiPassword { get; set; }
        string SecretKey { get; set; }
        string AccessToken { get; set; }
    }
}
