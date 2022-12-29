using Microsoft.EntityFrameworkCore;
using Muffin.Common.Api.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Muffin.EntityFrameworkCore
{
    public static class DbContextExtensions
    {
        public static DbSet<T> GenericSet<T>(this DbContext context)
            where T : class
        {
            return (DbSet<T>)context.GenericSet(typeof(DbSet<T>));
        }

        public static object GenericSet<T>(this T context, Type propertyType)
            where T : DbContext
        {
            var propertyInfo = context
                ?.GetType()
                ?.GetProperties()
                ?.FirstOrDefault(x => x.PropertyType == propertyType);
            return propertyInfo?.GetValue(context);
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IEnumerable<TSource> query, string propertyName, SortOrder order)
        {
            if (order == SortOrder.Ascending)
            {
                return query.OrderBy(propertyName);
            }
            return query.OrderByDescending(propertyName);
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IEnumerable<TSource> query, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return query.AsQueryable().OrderBy(x => x);
            var entityType = typeof(TSource);

            //Create x=>x.PropName
            var propertyInfo = entityType.GetProperties().FirstOrDefault(x => string.Equals(x.Name, propertyName));
            ParameterExpression arg = Expression.Parameter(entityType, "x");
            MemberExpression property = Expression.Property(arg, propertyName);
            var selector = Expression.Lambda(property, new ParameterExpression[] { arg });

            //Get System.Linq.Queryable.OrderBy() method.
            var enumarableType = typeof(System.Linq.Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == "OrderBy" && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     var parameters = m.GetParameters().ToList();
                     //Put more restriction here to ensure selecting the right overload                
                     return parameters.Count == 2;//overload that has 2 parameters
                 }).Single();
            //The linq's OrderBy<TSource, TKey> has two generic types, which provided here
            var genericMethod = method
                 .MakeGenericMethod(entityType, propertyInfo.PropertyType);

            /*Call query.OrderBy(selector), with query and selector: x=> x.PropName
              Note that we pass the selector as Expression to the method and we don't compile it.
              By doing so EF can extract "order by" columns and generate SQL for it.*/
            var newQuery = (IOrderedQueryable<TSource>)genericMethod
                 .Invoke(genericMethod, new object[] { query, selector });
            return newQuery;
        }

        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IEnumerable<TSource> query, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return query.AsQueryable().OrderBy(x => x);
            var entityType = typeof(TSource);

            //Create x=>x.PropName
            var propertyInfo = entityType.GetProperties().FirstOrDefault(x => string.Equals(x.Name, propertyName));
            ParameterExpression arg = Expression.Parameter(entityType, "x");
            MemberExpression property = Expression.Property(arg, propertyName);
            var selector = Expression.Lambda(property, new ParameterExpression[] { arg });

            //Get System.Linq.Queryable.OrderBy() method.
            var enumarableType = typeof(System.Linq.Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == "ThenBy" && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     var parameters = m.GetParameters().ToList();
                     //Put more restriction here to ensure selecting the right overload                
                     return parameters.Count == 2;//overload that has 2 parameters
                 }).Single();
            //The linq's OrderBy<TSource, TKey> has two generic types, which provided here
            var genericMethod = method
                 .MakeGenericMethod(entityType, propertyInfo.PropertyType);

            /*Call query.OrderBy(selector), with query and selector: x=> x.PropName
              Note that we pass the selector as Expression to the method and we don't compile it.
              By doing so EF can extract "order by" columns and generate SQL for it.*/
            var newQuery = (IOrderedQueryable<TSource>)genericMethod
                 .Invoke(genericMethod, new object[] { query, selector });
            return newQuery;
        }

        public static IOrderedQueryable<TSource> OrderByDescending<TSource>(this IEnumerable<TSource> query, string propertyName)
        {
            var entityType = typeof(TSource);

            //Create x=>x.PropName
            var propertyInfo = entityType.GetProperties().FirstOrDefault(x => string.Equals(x.Name, propertyName));
            ParameterExpression arg = Expression.Parameter(entityType, "x");
            MemberExpression property = Expression.Property(arg, propertyName);
            var selector = Expression.Lambda(property, new ParameterExpression[] { arg });

            //Get System.Linq.Queryable.OrderBy() method.
            var enumarableType = typeof(System.Linq.Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == "OrderByDescending" && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     var parameters = m.GetParameters().ToList();
                     //Put more restriction here to ensure selecting the right overload                
                     return parameters.Count == 2;//overload that has 2 parameters
                 }).Single();
            //The linq's OrderBy<TSource, TKey> has two generic types, which provided here
            var genericMethod = method
                 .MakeGenericMethod(entityType, propertyInfo.PropertyType);

            /*Call query.OrderBy(selector), with query and selector: x=> x.PropName
              Note that we pass the selector as Expression to the method and we don't compile it.
              By doing so EF can extract "order by" columns and generate SQL for it.*/
            var newQuery = (IOrderedQueryable<TSource>)genericMethod
                 .Invoke(genericMethod, new object[] { query, selector });
            return newQuery;
        }

        public static IOrderedQueryable<TSource> ThenByDescending<TSource>(this IEnumerable<TSource> query, string propertyName)
        {
            var entityType = typeof(TSource);

            //Create x=>x.PropName
            var propertyInfo = entityType.GetProperties().FirstOrDefault(x => string.Equals(x.Name, propertyName));
            ParameterExpression arg = Expression.Parameter(entityType, "x");
            MemberExpression property = Expression.Property(arg, propertyName);
            var selector = Expression.Lambda(property, new ParameterExpression[] { arg });

            //Get System.Linq.Queryable.OrderBy() method.
            var enumarableType = typeof(System.Linq.Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == "ThenByDescending" && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     var parameters = m.GetParameters().ToList();
                     //Put more restriction here to ensure selecting the right overload                
                     return parameters.Count == 2;//overload that has 2 parameters
                 }).Single();
            //The linq's OrderBy<TSource, TKey> has two generic types, which provided here
            var genericMethod = method
                 .MakeGenericMethod(entityType, propertyInfo.PropertyType);

            /*Call query.OrderBy(selector), with query and selector: x=> x.PropName
              Note that we pass the selector as Expression to the method and we don't compile it.
              By doing so EF can extract "order by" columns and generate SQL for it.*/
            var newQuery = (IOrderedQueryable<TSource>)genericMethod
                 .Invoke(genericMethod, new object[] { query, selector });
            return newQuery;
        }

        // Paging
        public static PagedFilterResult<T> PagedFilter<T>(this IQueryable<T> set, PagedFilterRequest model)
            where T : class
        {
            return set.PagedFilter(model, null);
        }

        public static PagedFilterResult<T> PagedFilter<T>(this IQueryable<T> set, PagedFilterRequest model, Func<IQueryable<T>, IQueryable<T>> customQuery)
            where T : class
        {
            T[] data;
            if (model != null)
            {
                var predicate = model.GetFilterPredicate<T>();
                var query = set.Where(predicate);

                if (customQuery != null)
                {
                    query = customQuery(query);
                }

                if (model.Sort != null && !string.IsNullOrWhiteSpace(model.Sort.Property))
                {
                    query = query.OrderBy(model.Sort.Property, model.Sort.Order);
                }

                var page = model.Page;
                if (model.Page != null)
                {
                    query = query.Skip(model.Page.Skip)
                        .Take(model.Page.PageSize);

                    model.Page.TotalItemCount = set.Count(predicate);
                    model.Page.TotalPagesCount = model.Page.TotalItemCount / model.Page.PageSize;
                }

                data = query.ToArray();
                if (page == null)
                {
                    page = new PageInfo()
                    {
                        TotalItemCount = data.Length,
                        TotalPagesCount = 1
                    };
                }

                return new PagedFilterResult<T>()
                {
                    Items = data,
                    Page = page,
                    Sort = model.Sort
                };
            }

            data = set.ToArray();
            return new PagedFilterResult<T>()
            {
                Items = data,
                Page = new PageInfo()
                {
                    PageSize = data.Length,
                    Start = 0,
                    TotalItemCount = data.Length,
                    TotalPagesCount = 1
                }
            };
        }
    }
}
