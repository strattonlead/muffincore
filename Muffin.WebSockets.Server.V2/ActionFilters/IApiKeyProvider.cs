using System.Linq;

namespace Muffin.WebSockets.Server.ActionFilters
{
    public interface IApiKeyProvider<T>
    {
        public IQueryable<T> GetApiKeys();
    }

    public interface IApiKeyProvider : IApiKeyProvider<string> { }
}
