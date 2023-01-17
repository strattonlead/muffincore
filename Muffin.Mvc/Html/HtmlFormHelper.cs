using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using System;
using System.Linq.Expressions;

namespace Muffin.Mvc.Html
{
    public static class HtmlFormHelper
    {
        public static HtmlString DisplayDescriptionFor<TModel, TValue>(this IHtmlHelper<TModel> self, Expression<Func<TModel, TValue>> expression)
        {
            var modelExplorer = ExpressionMetadataProvider.FromLambdaExpression(expression, self.ViewData, self.MetadataProvider);
            var metadata = modelExplorer.Metadata;
            var description = metadata.Description;

            return new HtmlString(description);
        }
    }
}
