using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Muffin.Common.Util
{
    public static class AnnotationHelper
    {
        public static string DisplayValue<T>(this T enumValue)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException();
            return DisplayValue(enumValue as Enum);
        }

        public static string DisplayValue(this Enum enumValue)
        {
            var displayAttribute = GetDisplayAttribute(enumValue);
            if (displayAttribute != null)
                return displayAttribute.Name;
            return enumValue.ToString();
        }

        public static DisplayAttribute GetDisplayAttribute<T>(this T enumValue)
             where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException();
            return GetDisplayAttribute(enumValue as Enum);
        }

        public static DisplayAttribute GetDisplayAttribute(this Enum enumValue)
        {
            var memberInfo = enumValue.GetType()
                        .GetMember(enumValue.ToString())
                        .FirstOrDefault();
            if (memberInfo != null)
                return memberInfo.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute;
            return null;
        }
    }
}
