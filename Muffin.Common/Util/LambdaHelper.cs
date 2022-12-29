using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Muffin.Common.Util
{
    public static class LambdaHelper
    {

        public static Expression<Func<T, bool>> MakeFilter<T>(string prop, object val, RelationalType relationalType)
        {
            ParameterExpression pe = Expression.Parameter(typeof(T), "x");
            PropertyInfo pi = typeof(T).GetProperty(prop);
            MemberExpression me = Expression.MakeMemberAccess(pe, pi);
            ConstantExpression ce = Expression.Constant(val);

            Expression be;
            switch (relationalType)
            {
                case RelationalType.LessThan:
                    be = Expression.LessThan(me, ce);
                    break;
                case RelationalType.LessThanOrEqual:
                    be = Expression.LessThanOrEqual(me, ce);
                    break;
                case RelationalType.GreaterThan:
                    be = Expression.GreaterThan(me, ce);
                    break;
                case RelationalType.GreaterThanOrEqual:
                    be = Expression.GreaterThanOrEqual(me, ce);
                    break;
                case RelationalType.Contains:
                    be = _doStringHelperCall<T>("Contains", val?.ToString(), prop);
                    break;
                case RelationalType.StartsWith:
                    be = _doStringHelperCall<T>("StartsWith", val?.ToString(), prop);
                    break;
                case RelationalType.EndsWith:
                    be = _doStringHelperCall<T>("EndsWith", val?.ToString(), prop);
                    break;
                case RelationalType.Equal:
                case RelationalType.Unknown:
                default:
                    be = Expression.Equal(me, ce);
                    break;
            }

            return Expression.Lambda<Func<T, bool>>(be, pe);
        }

        private static Expression _doStringHelperCall<T>(string methodName, string val, string prop)
        {
            ParameterExpression pe = Expression.Parameter(typeof(T), "x");
            var method = typeof(string).GetMethod(methodName, new[] { typeof(string) });
            var someValue = Expression.Constant(val, typeof(string));
            var propertyExp = Expression.Property(pe, prop);
            return Expression.Call(propertyExp, method, someValue);
        }

        public static Expression<Func<T, bool>> MakeFilter<T>(string prop, object val)
        {
            ParameterExpression pe = Expression.Parameter(typeof(T), "x");
            PropertyInfo pi = typeof(T).GetProperty(prop);
            MemberExpression me = Expression.MakeMemberAccess(pe, pi);
            Expression ce = Expression.Constant(val);
            if (val != null)
            {
                if (val.GetType() != pi.PropertyType)
                {
                    ce = Expression.Convert(Expression.Constant(val), pi.PropertyType);
                }
            }
            BinaryExpression be = Expression.Equal(me, ce);

            return Expression.Lambda<Func<T, bool>>(be, pe);
        }

        public static LambdaExpression MakeFilter(Type propertyType, string prop, object val)
        {
            ParameterExpression pe = Expression.Parameter(propertyType, "x");
            PropertyInfo pi = propertyType.GetProperty(prop);
            MemberExpression me = Expression.MakeMemberAccess(pe, pi);
            ConstantExpression ce = Expression.Constant(val, pi.PropertyType);
            BinaryExpression be = Expression.Equal(me, ce);

            return Expression.Lambda(be, pe);
        }

        public static LambdaExpression MakeFilterOrNull(Type propertyType, string prop, object val)
        {
            ParameterExpression pe = Expression.Parameter(propertyType, "x");
            PropertyInfo pi = propertyType.GetProperty(prop);
            MemberExpression me = Expression.MakeMemberAccess(pe, pi);
            ConstantExpression ce = Expression.Constant(val, pi.PropertyType);
            BinaryExpression be = Expression.Equal(me, ce);

            ConstantExpression ceNull = Expression.Constant(null, pi.PropertyType);
            BinaryExpression beNull = Expression.Equal(me, ceNull);

            var or = Expression.Or(be, beNull);

            return Expression.Lambda(or, pe);
        }

        public static LambdaExpression MakeTypesafeFilter<T>(Type propertyType, string prop, T val)
        {
            ParameterExpression pe = Expression.Parameter(propertyType, "x");
            PropertyInfo pi = propertyType.GetProperty(prop);
            MemberExpression me = Expression.MakeMemberAccess(pe, pi);
            ConstantExpression ce = Expression.Constant(val, typeof(T));
            BinaryExpression be = Expression.Equal(me, ce);

            return Expression.Lambda(be, pe);
        }

        public static Expression<Func<T, bool>> MakeFilterWithTypeCheck<T>(string prop, object val)
        {
            PropertyInfo pi = typeof(T).GetProperty(prop);
            var value = Convert.ChangeType(val, pi.PropertyType);

            return MakeFilter<T>(prop, value);
        }

        //public static Expression<Func<T, bool>> MakeListFilter<T, TKey>(string prop, IEnumerable<TKey> ids)
        //{
        //    var methodInfo = typeof(System.Linq.Enumerable)
        //          .GetMethods(BindingFlags.Static | BindingFlags.Public)
        //          .Where(mi => mi.Name.Equals("Contains"))
        //          .FirstOrDefault()
        //          .MakeGenericMethod(new Type[] { typeof(TKey) });

        //    ParameterExpression pe = Expression.Parameter(typeof(T), "x");
        //    PropertyInfo pi = typeof(T).GetProperty(prop);
        //    MemberExpression me = Expression.MakeMemberAccess(pe, pi);
        //    ConstantExpression ce = Expression.Constant(ids);
        //    MethodCallExpression mce = Expression.Call(methodInfo, ce, me);

        //    return Expression.Lambda<Func<T, bool>>(mce, pe);
        //}

        public static Expression<Func<T, bool>> MakeListFilter<T, TKey>(string prop, IEnumerable<TKey> ids)
        {
            return MakeListFilter<T>(prop, ids, typeof(TKey));
        }

        public static Expression<Func<T, bool>> MakeListFilter<T>(string prop, IEnumerable ids, Type keyType)
        {
            var methodInfo = typeof(System.Linq.Enumerable)
                  .GetMethods(BindingFlags.Static | BindingFlags.Public)
                  .Where(mi => mi.Name.Equals("Contains"))
                  .FirstOrDefault()
                  .MakeGenericMethod(new Type[] { keyType });

            ParameterExpression pe = Expression.Parameter(typeof(T), "x");
            PropertyInfo pi = typeof(T).GetProperty(prop);
            MemberExpression me = Expression.MakeMemberAccess(pe, pi);
            ConstantExpression ce = Expression.Constant(ids);
            MethodCallExpression mce = Expression.Call(methodInfo, ce, me);

            return Expression.Lambda<Func<T, bool>>(mce, pe);
        }

        public static Expression MemberSelector<T>(string prop)
        {
            ParameterExpression pe = Expression.Parameter(typeof(T), "x");
            PropertyInfo pi = typeof(T).GetProperty(prop);
            MemberExpression me = Expression.MakeMemberAccess(pe, pi);
            return Expression.Lambda(me, pe);
        }

        public static Expression<Func<object, object>> MemberSelectorAsObject(string prop)
        {
            ParameterExpression pe = Expression.Parameter(typeof(object), "x");
            PropertyInfo pi = typeof(object).GetProperty(prop);
            MemberExpression me = Expression.MakeMemberAccess(pe, pi);
            return Expression.Lambda<Func<object, object>>(me, pe);
        }

        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expresion '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }
    }

    public enum RelationalType
    {
        Unknown = 0,
        LessThan = 1,
        LessThanOrEqual = 2,
        Equal = 3,
        GreaterThanOrEqual = 4,
        GreaterThan = 5,
        Contains = 6,
        StartsWith = 7,
        EndsWith = 8
    }
}
