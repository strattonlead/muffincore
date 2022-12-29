using Muffin.EntityFrameworkCore.Entity;
using Muffin.Shopify.Abstraction;
using System;

namespace Muffin.EntityFrameworkCore.Shopify
{
    public class ShopifyCredential : BaseEntity, IShopifyCredential
    {
        #region Properties

        public DateTime? MinAccessDateUtc { get; set; }
        public string ShopUrl { get; set; }
        public string ApiPassword { get; set; }
        public string SecretKey { get; set; }
        public string AccessToken { get; set; }

        #endregion

        #region Constructor

        public ShopifyCredential() { }

        #endregion
    }
}
