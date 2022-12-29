using System.Collections.Generic;
using System.Threading.Tasks;

namespace Muffin.Globalization.Services.Abstraction
{
    public interface ITranslationManager
    {
        //Dictionary<string, string> GetTranslationsWithKeyPath(string key);
        //Dictionary<string, Dictionary<string, string>> GetTranslationsWithKeyPaths(IEnumerable<string> keys);
        //Dictionary<string, object> GetTranslation(string key);
        //Dictionary<string, Dictionary<string, object>> GetTranslations(IEnumerable<string> keys);
        Task<Dictionary<string, object>> GetTranslationsAsync();
        Dictionary<string, object> GetTranslations();
    }
}