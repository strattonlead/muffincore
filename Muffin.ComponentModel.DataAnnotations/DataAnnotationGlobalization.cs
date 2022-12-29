using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Muffin.ComponentModel.DataAnnotations
{
    public static class DataAnnotationGlobalization
    {
        public static bool EnableLocalizedDataAnnotations { get; private set; }

        public static void AddLocalizedDataAnnotations(this IServiceCollection services, Action<string[]> keyAction)
        {
            EnableLocalizedDataAnnotations = true;

            var keys = new string[] { MinValueAttribute.ERROR_MESSAGE_KEY };
            keyAction?.Invoke(keys);
        }
    }
}
