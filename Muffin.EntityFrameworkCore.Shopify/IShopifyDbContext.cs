using Microsoft.EntityFrameworkCore;
using Muffin.Shopify;
using Muffin.Shopify.Abstraction;

namespace Muffin.EntityFrameworkCore.Shopify
{
    public interface IShopifyDbContext<TShopifyCredential>
        where TShopifyCredential : class,IShopifyCredential
    {
        DbSet<TShopifyCredential> ShopifyCredentials { get; set; }
    }

    public interface IShopifyDbContext : IShopifyDbContext<ShopifyCredential>
    {
    }
}
