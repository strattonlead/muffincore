using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Muffin.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MuffinMvc.Html
{
    public static class HtmlCheckBoxHelper
    {
        public static HtmlString CheckBoxList<TModel, TObject>(this IHtmlHelper<TModel> htmlHelper, IEnumerable<TObject> objects, IEnumerable<TObject> selected)
        {
            var aObjects = objects.ToArray();
            var aSelected = selected.ToArray();
            var lSelected = new List<bool>();
            for (int i = 0; i < aObjects.Length; i++)
                lSelected.Add(aObjects.Contains(aObjects[i]));

            return htmlHelper.CheckBoxList(aObjects, lSelected);
        }

        public static HtmlString CheckBoxList<TModel, TObject>(this IHtmlHelper<TModel> htmlHelper, IEnumerable<TObject> objects, IEnumerable<bool> selected)
        {
            var aObjects = objects.ToArray();
            var aSelected = selected.ToArray();
            var sb = new StringBuilder();
            for (int i = 0; i < aObjects.Length; i++)
            {
                sb.AppendLine(htmlHelper.HiddenFor(x => aObjects[i]).ToString());
                sb.AppendLine(htmlHelper.CheckBoxFor(x => aSelected[i]).ToString());
            }
            return new HtmlString(sb.ToString());
        }

        public static HtmlString CheckBoxListFor<TModel, TObject>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, CheckBoxDescription<TObject>>> expression)
        {
            var model = (TModel)htmlHelper.ViewData.Model;
            var boxModel = expression.Compile()(model);
            return htmlHelper.CheckBoxList(boxModel);
        }

        public static HtmlString CheckBoxListFor<TModel, TObject>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, IEnumerable<CheckBoxDescription<TObject>>>> expression)
        {
            var model = (TModel)htmlHelper.ViewData.Model;
            var boxModels = expression.Compile()(model);

            var sb = new StringBuilder();
            foreach (var boxModel in boxModels)
                sb.AppendLine(htmlHelper.CheckBoxList(boxModel).ToString());
            return new HtmlString(sb.ToString());
        }

        public static HtmlString CheckBoxListFor<TModel, TObject>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, IEnumerable<TObject>>> expression, Expression<Func<TModel, IEnumerable<TObject>>> selected)
        {
            var model = (TModel)htmlHelper.ViewData.Model;
            return htmlHelper.CheckBoxList(expression.Compile()(model), selected.Compile()(model));
        }

        public static HtmlString CheckBoxListFor<TModel, TObject>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, IEnumerable<TObject>>> expression, Expression<Func<TModel, IEnumerable<bool>>> selected)
        {
            var model = (TModel)htmlHelper.ViewData.Model;
            return htmlHelper.CheckBoxList(expression.Compile()(model), selected.Compile()(model));
        }

        public static HtmlString CheckBoxList<TObject>(this IHtmlHelper htmlHelper, CheckBoxDescription<TObject> checkBoxDescription)
        {
            var sb = new StringBuilder();
            sb.AppendLine(htmlHelper.Hidden(checkBoxDescription.Name, checkBoxDescription.Value).ToString());
            sb.AppendLine(htmlHelper.CheckBox(checkBoxDescription.Name, checkBoxDescription.Selected).ToString() + " " + checkBoxDescription.DisplayName);
            return new HtmlString(sb.ToString());
        }

        /// <summary>
        /// Renders checkbox as one input (normal Html.CheckBoxFor renders two inputs: checkbox and hidden)
        /// </summary>
        public static HtmlString BasicCheckBoxFor<T>(this IHtmlHelper<T> htmlHelper, Expression<Func<T, bool>> expression, object htmlAttributes = null)
        {
            var tag = new TagBuilder("input");

            tag.Attributes["type"] = "checkbox";
            tag.Attributes["id"] = htmlHelper.IdFor(expression).ToString();
            tag.Attributes["name"] = htmlHelper.NameFor(expression).ToString();
            tag.Attributes["value"] = "true";

            // set the "checked" attribute if true
            var modelExplorer = ExpressionMetadataProvider.FromLambdaExpression(expression, htmlHelper.ViewData, htmlHelper.MetadataProvider);
            if (modelExplorer.Model != null)
            {
                bool modelChecked;
                if (Boolean.TryParse(modelExplorer.Model.ToString(), out modelChecked))
                {
                    if (modelChecked)
                    {
                        tag.Attributes["checked"] = "checked";
                        tag.Attributes["value"] = "true";
                    }
                }
            }

            // merge custom attributes
            tag.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));

            var tagString = tag.RenderSelfClosingTag();
            return new HtmlString(tagString.ToString());
        }

        public static HtmlString BasicCheckBox(this IHtmlHelper html, string name)
        {
            return html.BasicCheckBox(name, false);
        }

        public static HtmlString BasicCheckBox(this IHtmlHelper html, string name, bool isChecked)
        {
            return html.BasicCheckBox(name, isChecked, null);
        }

        public static HtmlString BasicCheckBox(this IHtmlHelper html, string name, object htmlAttributes)
        {
            return html.BasicCheckBox(name, false, htmlAttributes);
        }

        public static HtmlString BasicCheckBox(this IHtmlHelper html, string name, bool isChecked, object htmlAttributes)
        {
            var tag = new TagBuilder("input");

            tag.Attributes["type"] = "checkbox";
            tag.Attributes["id"] = name;
            tag.Attributes["name"] = name;
            tag.Attributes["value"] = "true";

            if (isChecked)
            {
                tag.Attributes["checked"] = "checked";
                tag.Attributes["value"] = "true";
            }

            tag.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));

            var tagString = tag.RenderSelfClosingTag();
            return new HtmlString(tagString.ToString());
        }

        public static IHtmlContent TextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, bool disabled, object htmlAttributes)
        {
            if (disabled)
            {
                var dynamic = htmlAttributes.ToDynamic() as IDictionary<string, object>;
                dynamic.Add("disabled", "disabled");
            }
            return htmlHelper.TextBoxFor(expression, htmlAttributes);
        }
    }

    public class CheckBoxDescription<T>
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool Selected { get; set; }
        public T Value { get; set; }
    }
}
