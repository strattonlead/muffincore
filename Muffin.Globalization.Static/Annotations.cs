using System;
using System.Linq;
using System.Reflection;

namespace Muffin.Globalization.Static
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class GermanAttribute : DefaultValueAttribute
    {
        public override string LanguageCode => "de";
        public GermanAttribute(string value) : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EnglishAttribute : DefaultValueAttribute
    {
        public override string LanguageCode => "en";
        public EnglishAttribute(string value) : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FrenchAttribute : DefaultValueAttribute
    {
        public override string LanguageCode => "fr";
        public FrenchAttribute(string value) : base(value) { }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ItalianAttribute : DefaultValueAttribute
    {
        public override string LanguageCode => "it";
        public ItalianAttribute(string value) : base(value) { }
    }

    public abstract class DefaultValueAttribute : Attribute
    {
        public string Value { get; set; }
        public abstract string LanguageCode { get; }
        public virtual string TwoLetterIsoLanguageCode { get => LanguageCode?.ToUpper(); }

        public DefaultValueAttribute(string value)
        {
            Value = value;
        }
    }

    public static class DefaultValueAnnotationExtensions
    {
        public static DefaultValueAttribute[] GetLanguageAttributes(this MemberInfo memberInfo)
        {
            var baseType = typeof(DefaultValueAttribute);
            return memberInfo
                .GetCustomAttributes()
                .Where(x => baseType.IsAssignableFrom(x.GetType()) && !x.GetType().IsAbstract)
                .Cast<DefaultValueAttribute>()
                .ToArray();
        }
    }
}
