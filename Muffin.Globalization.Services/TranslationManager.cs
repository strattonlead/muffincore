using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Muffin.EntityFrameworkCore.Globalization;
using Muffin.Globalization.Services.Abstraction;

namespace Muffin.Globalization.Services
{
    public class TranslationManager<TContext> : ITranslationManager
        where TContext : DbContext, IGlobalizationDbContext
    {
        #region Properties

        private readonly TContext DbContext;

        #endregion

        #region Constructor

        public TranslationManager(IServiceProvider serviceProvider)
        {
            DbContext = serviceProvider.GetRequiredService<TContext>();
        }

        #endregion

        #region Actions

        public async Task<Dictionary<string, object>> GetTranslationsAsync()
        {
            return await Task.Run(() =>
            {
                return GetTranslations();
            });
        }

        public Dictionary<string, object> GetTranslations()
        {
            var localizedStrings = DbContext.LocalizedStrings.AsSplitQuery().AsNoTracking().ToArray();
            var languageCodes = localizedStrings.SelectMany(x => x.LocalizedStringValues).Select(x => x.LanguageId).Distinct().ToArray();

            var resultModel = new Dictionary<string, object>();
            foreach (var languageCode in languageCodes)
            {
                var flatDict = localizedStrings.SelectMany(x => x.LocalizedStringValues.Where(y => y.LanguageId == languageCode).Select(y => new { y, x })).ToDictionary(x => x.x.ToString(), x => x.y.Value);
                var nestedDict = TransformKeyPath(flatDict);
                resultModel[languageCode] = nestedDict;
            }

            return resultModel;
        }

        private Dictionary<string, object> TransformKeyPath(Dictionary<string, string> dict)
        {
            return TransformKeyPath(dict.ToDictionary(x => x.Key, x => (object)x.Value));
        }
        private Dictionary<string, object> TransformKeyPath(Dictionary<string, object> dict)
        {
            var result = new Dictionary<string, object>();
            var keys = dict.Keys.ToArray();
            var temp = keys.Select(x => new { Key = x, Parts = x.Split('.').ToList(), Value = dict[x] }).ToArray();
            var grouped = temp.GroupBy(x => x.Parts[0]);
            foreach (var group in grouped)
            {
                var t2 = new Dictionary<string, object>();
                var getDeeper = false;
                foreach (var item in group)
                {
                    if (item.Parts.Count > 1)
                    {
                        t2[string.Join(".", item.Parts.Skip(1))] = item.Value;
                        getDeeper = true;
                    }
                    else
                    {
                        t2[item.Parts[0]] = item.Value;
                    }
                }

                if (getDeeper)
                {
                    result[group.Key] = TransformKeyPath(t2);
                }
                else
                {
                    result[group.Key] = t2[group.Key];
                }
            }
            return result;
        }

        #endregion
    }

    public static class TranslationManagerExtensions
    {
        public static void AddTranslationManager<TContext>(this IServiceCollection services)
            where TContext : DbContext, IGlobalizationDbContext
        {
            services.AddScoped<ITranslationManager, TranslationManager<TContext>>();
        }
    }
}