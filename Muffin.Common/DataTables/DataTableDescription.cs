using Muffin.Common.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static Muffin.Common.DataTables.DataTableRequestModel;

namespace Muffin.Common.DataTables
{
    public class DataTableDescription<T>
    {
        private Dictionary<string, SingleColumnDescription> _selectors;
        //private Dictionary<string, string> _formats;
        public TimeZoneInfo TimeZone { get; set; }

        public SingleColumnDescription[] Columns
        {
            get
            {
                return _selectors?.Values.ToArray();
            }
        }

        public DataTableDescription()
        {
            TimeZone = TimeZoneInfo.Utc;
            _selectors = new Dictionary<string, SingleColumnDescription>();
            //_formats = new Dictionary<string, string>();
        }

        public DataTableDescription(TimeZoneInfo timeZone)
        {
            TimeZone = timeZone;
            _selectors = new Dictionary<string, SingleColumnDescription>();
            //_formats = new Dictionary<string, string>();
        }

        public SingleColumnDescription AddColumnDescriptionWithAutoName<TKey>(Expression<Func<T, TKey>> selector)
        {
            var name = PropertyHelper.GetPropertyDisplayName(selector);
            return AddColumnDescription(name, selector);
        }

        public SingleColumnDescription AddColumnDescriptionWithDifferentPropertyName<TKey, TKey2>(Expression<Func<T, TKey>> propertySelector, Expression<Func<T, TKey2>> selector)
        {
            var name = PropertyHelper.GetPropertyDisplayName(propertySelector);
            var result = AddColumnDescription(name, selector);
            result.Order = new ColumnOrderInfo()
            {
                ShouldSort = false,
            };
            return result;
        }

        public SingleColumnDescription AddColumnDescriptionWithAutoNameAndSort<TKey>(Expression<Func<T, TKey>> selector, bool ascending)
        {
            var name = PropertyHelper.GetPropertyDisplayName(selector);
            var result = AddColumnDescription(name, selector);
            result.Order = new ColumnOrderInfo()
            {
                ShouldSort = true,
                Ascending = ascending
            };
            return result;
        }

        public SingleColumnDescription AddColumnDescriptionWithAutoNameAndWithoutSort<TKey>(Expression<Func<T, TKey>> selector)
        {
            var name = PropertyHelper.GetPropertyDisplayName(selector);
            var result = AddColumnDescription(name, selector);
            result.Order = new ColumnOrderInfo()
            {
                ShouldSort = false,
            };
            return result;
        }

        public SingleColumnDescription AddColumnDescription<TKey>(string name, Expression<Func<T, TKey>> selector)
        {
            var column = new SingleColumnDescription(name, selector);
            _selectors.Add(name, column);
            return column;
        }

        public SingleColumnDescription AddColumnDescriptionWithoutSort<TKey>(string name, Expression<Func<T, TKey>> selector)
        {
            var column = new SingleColumnDescription(name, selector)
            {
                Order = new ColumnOrderInfo()
                {
                    ShouldSort = false
                }
            };
            _selectors.Add(name, column);
            return column;
        }

        public SingleColumnDescription AddColumnDescriptionWithSort<TEntity, TKey, TOrder>(string name, Expression<Func<T, TKey>> displaySelector, Expression<Func<TEntity, TOrder>> sortSelector)
        {
            var column = new SingleColumnDescription(name, displaySelector)
            {
                Order = new ColumnOrderInfo()
                {
                    SortExpression = sortSelector as LambdaExpression,
                    ShouldSort = sortSelector as LambdaExpression != null
                }
            };
            _selectors.Add(name, column);
            return column;
        }

        public SingleColumnDescription AddColumnDescription<TKey>(string name, Expression<Func<T, TKey>> selector, ColumnOrderInfo orderInfo)
        {
            var column = new SingleColumnDescription(name, selector) { Order = orderInfo };
            _selectors.Add(name, column);
            return column;
        }

        public SingleColumnDescription AddColumnDescription<TKey>(string name, string displayName, Expression<Func<T, TKey>> selector, ColumnOrderInfo orderInfo)
        {
            var column = new SingleColumnDescription(name, selector)
            {
                DispalyName = displayName,
                Order = orderInfo
            };
            _selectors.Add(name, column);
            return column;
        }

        public SingleColumnDescription AddColumnDescription(string name, Expression<Func<T, DateTime>> selector, string format)
        {
            var column = new SingleColumnDescription(name, selector) { Format = format };
            _selectors.Add(name, column);
            return column;
        }

        public SingleColumnDescription AddColumnDescription(string name, Expression<Func<T, DateTime?>> selector, string format)
        {
            var column = new SingleColumnDescription(name, selector) { Format = format };
            _selectors.Add(name, column);
            return column;
        }

        public IEnumerable<SingleColumnDescription> GetHeaders()
        {
            return _selectors.Select(x => x.Value).ToArray();
        }

        public IEnumerable<string> GetRows(object source)
        {
            var result = new List<string>();
            var columnInfos = _selectors.ToArray();

            foreach (var pair in columnInfos)
            {
                var columnInfo = pair.Value;
                if (source == null)
                {
                    result.Add("");
                    continue;
                }

                var lambda = columnInfo.Selector;

                var func = lambda.Compile();
                var obj = func.DynamicInvoke(source);

                //string format = null;
                //if (_formats.ContainsKey(pair.Key))
                //    format = _formats[pair.Key];

                if (obj != null)
                {
                    if (obj.GetType() == typeof(DateTime))
                    {
                        if (string.IsNullOrWhiteSpace(columnInfo.Format))
                        {
                            result.Add(TimeZoneInfo.ConvertTimeFromUtc((DateTime)obj, TimeZone).ToString());
                        }
                        else
                        {
                            result.Add(TimeZoneInfo.ConvertTimeFromUtc((DateTime)obj, TimeZone).ToString(columnInfo.Format));
                        }
                    }
                    else if (obj.GetType() == typeof(DateTime?))
                    {
                        var temp = (DateTime?)obj;
                        if (!temp.HasValue)
                        {
                            result.Add("");
                        }
                        else
                        {
                            var dateTime = temp.Value;
                            if (string.IsNullOrWhiteSpace(columnInfo.Format))
                            {
                                result.Add(TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZone).ToString());
                            }
                            else
                            {
                                result.Add(TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZone).ToString(columnInfo.Format));
                            }
                        }
                    }
                    else
                    {
                        result.Add(obj.ToString());
                    }
                }
                else
                    result.Add("");
            }
            return result;
        }

        public string GetColumnName(SortInfo sortInfo)
        {
            if (sortInfo == null)
                return null;
            if (_selectors.Count < sortInfo.Column)
                return null;

            LambdaExpression expression;
            if (_selectors.ElementAt(sortInfo.Column).Value.Order != null
                && _selectors.ElementAt(sortInfo.Column).Value.Order.SortExpression != null)
            {
                expression = _selectors.ElementAt(sortInfo.Column).Value.Order.SortExpression;
            }
            else
                expression = _selectors.ElementAt(sortInfo.Column).Value.Selector;


            MemberExpression memberExpression = null;
            if (expression.Body is UnaryExpression)
            {
                var UnExp = (UnaryExpression)expression.Body;
                if (UnExp.Operand is MemberExpression)
                    memberExpression = (MemberExpression)UnExp.Operand;
                else
                    return null;
            }
            else if (expression.Body is MemberExpression)
                memberExpression = (MemberExpression)expression.Body;
            else
                return null;

            var memberInfo = memberExpression.Member;
            return memberInfo.Name;
        }
    }

    public class SingleColumnDescription
    {
        public string Name { get; set; }
        public string DispalyName { get; set; }
        [JsonIgnore]
        public LambdaExpression Selector { get; set; }
        public ColumnOrderInfo Order { get; set; }
        public string Format { get; set; }

        public SingleColumnDescription(string name)
        {
            Name = name;
            DispalyName = name;
        }

        public SingleColumnDescription(string name, LambdaExpression selector)
            : this(name)
        {
            Selector = selector;
        }
    }

    public sealed class ColumnOrderInfo
    {
        public LambdaExpression SortExpression { get; set; }
        public bool ShouldSort { get; set; }
        public bool Ascending { get; set; }
    }
}
