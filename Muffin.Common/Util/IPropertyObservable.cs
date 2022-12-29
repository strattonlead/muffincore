using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Muffin.Common.Util
{
    public interface IPropertyObservable
    {
        event PropertyChangedEvent OnPropertyChanged;
    }

    public delegate void PropertyChangedEvent<T>(object sender, PropertyChangedEventArgs<T> args);
    public delegate void PropertyChangedEvent(object sender, PropertyInfo propertyInfo, PropertyChangedEventArgs args);

    public class PropertyChangedEventArgs
    {
        public PropertyChangedEventArgs(object oldValue, object newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public virtual object OldValue { get; set; }
        public virtual object NewValue { get; set; }
    }

    public class PropertyChangedEventArgs<T> : PropertyChangedEventArgs
    {
        public PropertyChangedEventArgs(T oldValue, T newValue)
            : base(oldValue, newValue) { }

        public new T OldValue { get { return (T)base.OldValue; } set { base.OldValue = value; } }
        public new T NewValue { get { return (T)base.NewValue; } set { base.NewValue = value; } }
    }

    public static class PropertyObservableHelper
    {
        public static PropertyInfo GetPropertyInfo<T, TProperty>(this T obj, Expression<Func<T, TProperty>> expression)
            where T : IPropertyObservable
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("expression must be of type MemberExpression");
            }

            return (PropertyInfo)memberExpression.Member;
        }
    }
}
