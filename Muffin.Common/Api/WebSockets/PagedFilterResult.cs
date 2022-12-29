using Newtonsoft.Json;
using System;
using System.Linq;

namespace Muffin.Common.Api.WebSockets
{
    public class PagedFilterResult<T>
    {
        public PagedFilterResult() { }

        [JsonProperty(PropertyName = "sort")]
        public SortInfo Sort { get; set; }

        [JsonProperty(PropertyName = "page")]
        public PageInfo Page { get; set; }

        [JsonProperty(PropertyName = "items")]
        public T[] Items { get; set; }

        public void CopyProperties<T2>(PagedFilterResult<T2> result)
        {
            Sort = result.Sort;
            Page = result.Page;
        }
    }

    public static class PagedFilterResultHelper
    {
        public static PagedFilterResult<TOut> Convert<TIn, TOut>(this PagedFilterResult<TIn> model, Func<TIn, TOut> selector)
        {
            var result = new PagedFilterResult<TOut>();
            result.CopyProperties(model);
            result.Items = model.Items?.Select(selector).ToArray();
            return result;
        }

        public static PagedFilterResult<TOut> Cast<TIn, TOut>(this PagedFilterResult<TIn> model)
        {
            if (model == null)
            {
                return null;
            }
            return new PagedFilterResult<TOut>()
            {
                Sort = model.Sort,
                Page = model.Page,
                Items = model.Items?.Cast<TOut>().ToArray()
            };
        }
    }
}