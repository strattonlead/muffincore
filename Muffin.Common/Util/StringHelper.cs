using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Muffin.Common.Util
{
    public static class StringHelper
    {
        public const string KEY_PATTERN = @"\@\{.*?\}";
        public static readonly Regex KeyRegex = new Regex(KEY_PATTERN);

        /// <summary>
        /// {key} ist Das Format wie es im Source String angegeben werden muss.
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string ReplacePlaceholdersWithDictionary(string sourceString, Dictionary<string, string> model)
        {
            var result = string.Copy(sourceString);
            var matches = new Regex(@"\{.*?\}").Matches(result)
                .Cast<Match>()
                .Select(p => p.Value)
                .ToArray();

            foreach (var match in matches)
            {
                var key = match.Replace("{", "").Replace("}", "");
                var placeholder = "{" + key + "}";

                if (model.TryGetValue(key, out string value))
                {
                    result = result.Replace(placeholder, value);
                }
            }

            return result;
        }

        /// <summary>
        /// @{key:format} ist das Format
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ReplaceKeyPaths(string sourceString, object source)
        {
            var result = string.Copy(sourceString);
            var matches = KeyRegex.Matches(result)
                .Cast<Match>()
                .Select(p => p.Value)
                .ToArray();
            foreach (var match in matches)
            {
                var keyPath = match.Replace("@{", "").Replace("}", "");
                string format = null;
                if (keyPath.Contains(":")) // Format
                {
                    var parts = keyPath.Split(':');
                    keyPath = parts[0];
                    format = parts[1];
                }
                var value = PropertyHelper.GetPropertyValue(source, keyPath);
                string stringValue = null;

                if (value != null && value.GetType() == typeof(string))
                    stringValue = (string)value;
                if (value != null && value.GetType().IsEnum)
                    stringValue = ((Enum)value).DisplayValue();
                else if (value != null && !string.IsNullOrWhiteSpace(format) && FormattableDataTypes.Contains(value.GetType()))
                    stringValue = _doFormat(value, format);
                else if (value != null && stringValue == null)
                    stringValue = value.ToString();
                else if (stringValue == null)
                    stringValue = "";

                result = result.Replace(match, stringValue);
            }

            return result;
        }

        public static readonly Type[] FormattableDataTypes = new Type[] {
            typeof(byte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(DateTime),
            typeof(TimeSpan)
        };

        private static string _doFormat(object value, string format)
        {
            var type = value.GetType();
            if (type == typeof(byte))
                return ((byte)value).ToString(format);
            if (type == typeof(short))
                return ((short)value).ToString(format);
            if (type == typeof(int))
                return ((int)value).ToString(format);
            if (type == typeof(long))
                return ((long)value).ToString(format);
            if (type == typeof(float))
                return ((float)value).ToString(format);
            if (type == typeof(double))
                return ((double)value).ToString(format);
            if (type == typeof(decimal))
                return ((decimal)value).ToString(format);
            if (type == typeof(DateTime))
                return ((DateTime)value).ToString(format);
            if (type == typeof(TimeSpan))
                return ((TimeSpan)value).ToString(format);

            return value.ToString();
        }

        public static string FirstCharToLowerCase(this string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
                return str;

            return char.ToLower(str[0]) + str.Substring(1);
        }

        public static string ReplaceLastOccurrence(this string str, string toReplace, string replacement)
        {
            return Regex.Replace(str, $@"^(.*){Regex.Escape(toReplace)}(.*?)$", $"$1{Regex.Escape(replacement)}$2");
        }
    }
}
