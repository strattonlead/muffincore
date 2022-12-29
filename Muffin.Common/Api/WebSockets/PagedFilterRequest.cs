using Muffin.Common.Util;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Muffin.Common.Api.WebSockets
{
    public class PagedFilterRequest
    {
        #region Properties

        [JsonProperty(PropertyName = "filter", NullValueHandling = NullValueHandling.Ignore)]
        public FilterInfo Filter { get; set; }

        [JsonProperty(PropertyName = "sort", NullValueHandling = NullValueHandling.Ignore)]
        public SortInfo Sort { get; set; }

        [JsonProperty(PropertyName = "page", NullValueHandling = NullValueHandling.Ignore)]
        public PageInfo Page { get; set; }

        #endregion

        #region Helper

        public Expression<Func<T, bool>> GetFilterPredicate<T>()
        {
            if (Filter == null)
            {
                return x => true;
            }

            var pi = Filter.GetPropertyInfo<T>();
            if (pi == null)
            {
                return x => true;
            }

            if (!Filter.IsValueAsList)
            {
                var value = Convert.ChangeType(Filter.Value, pi.PropertyType);
                return LambdaHelper.MakeFilter<T>(Filter.Property, value, Filter.RelationalOperator);
            }

            var arrayType = pi.PropertyType.MakeArrayType();
            var arrayList = (IEnumerable)JsonConvert.DeserializeObject(Filter.Value, arrayType);
            return LambdaHelper.MakeListFilter<T>(Filter.Property, arrayList, pi.PropertyType);
        }

        #endregion
    }

    public class PagedFilterRequest<T> : PagedFilterRequest
    {
        public PagedFilterRequest<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> property, bool ascending = true)
        {
            var memberExpression = property as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Only first level members are allowed");

            Sort.Property = memberExpression.Member.Name;
            Sort.Order = ascending ? SortOrder.Ascending : SortOrder.Descending;
            return this;
        }

        public PagedFilterRequest<T> Search(string searchValue)
        {
            Filter.Value = searchValue;
            return this;
        }

        public PagedFilterRequest<T> ByProperty<TProperty, TValue>(Expression<Func<T, TProperty>> property, TValue value)
        {
            var memberExpression = property as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Only first level members are allowed");

            Filter.Property = memberExpression.Member.Name;
            Filter.Value = value != null ? value.ToString() : null;

            return this;
        }

        public PagedFilterRequest<T> ByProperty<TProperty, TValue>(Expression<Func<T, TProperty>> property, IEnumerable<TValue> values)
        {
            var memberExpression = property as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Only first level members are allowed");

            Filter.Property = memberExpression.Member.Name;
            Filter.Value = JsonConvert.SerializeObject(values);

            return this;
        }

        public PagedFilterRequest<T> ById(long id)
        {
            Filter.Property = "Id";
            Filter.Value = id.ToString();
            return this;
        }
    }

    public class SortInfo
    {
        [JsonProperty(PropertyName = "property")]
        public string Property { get; set; }

        [JsonProperty(PropertyName = "order")]
        public SortOrder Order { get; set; } = SortOrder.Ascending;
    }

    public class PageInfo
    {
        [JsonProperty(PropertyName = "start")]
        public int Start { get; set; }

        [JsonProperty(PropertyName = "pageSize")]
        public int PageSize { get; set; }

        [JsonProperty(PropertyName = "totalItemsCount", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalItemCount { get; set; }

        [JsonProperty(PropertyName = "totalPagesCount", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalPagesCount { get; set; }

        [JsonIgnore]
        public int Skip { get { return Start * PageSize; } }
    }

    public enum ConjonctionType
    {
        Unknown = 0,
        And = 1,
        Or = 2
    }

    public enum SortOrder
    {
        Ascending = 0,
        Descending = 1
    }
}
