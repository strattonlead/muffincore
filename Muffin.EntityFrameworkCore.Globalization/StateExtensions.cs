using Muffin.EntityFrameworkCore.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace Muffin.StateManagement
{
    public static class StateExtensions
    {
        public static PartialAppState UpdateTranslation(this PartialAppState partialAppState, LocalizedStringEntity localizedStringEntity)
        {
            if (localizedStringEntity == null)
            {
                return partialAppState;
            }
            var translations = localizedStringEntity.LocalizedStringValues.ToDictionary(x => $"translations.{x.LanguageId}.{localizedStringEntity.KeyPath}", x => x.Value);
            partialAppState.UpdateTranslation(translations);
            return partialAppState;
        }

        public static PartialAppState UpdateTranslations(this PartialAppState partialAppState, IEnumerable<LocalizedStringEntity> localizedStringEntites)
        {
            foreach (var item in localizedStringEntites)
            {
                partialAppState.UpdateTranslation(item);
            }
            return partialAppState;
        }
    }
}
