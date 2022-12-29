using Newtonsoft.Json;
using System;
using System.Text;

namespace Muffin.Common.Util
{
    public static class NumberHelper
    {
        public static string FormatFileSize(long size, int decimals = 0)
        {
            return FormatThousands(size, new string[] { "B", "KB", "MB", "GB", "TB" }, decimals);
        }

        public static string FormatFileSize(object obj)
        {
            return FormatFileSize(obj, Encoding.UTF8);
        }

        public static string FormatFileSize(object obj, Encoding encoding)
        {
            var size = encoding.GetByteCount(JsonConvert.SerializeObject(obj));
            return FormatThousands(size, new string[] { "B", "KB", "MB", "GB", "TB" });
        }

        public static string FormatHashSize(long size, int decimals)
        {
            return FormatThousands(size, new string[] { "H", "KH", "MH", "GH", "TH" }, decimals, 1000);
        }

        public static string FormatHashSize(long size)
        {
            return FormatHashSize(size, 2);
        }

        public static string FormatThousands(long size, string[] sizes, int decimals = 0, int factor = 1024)
        {
            if (decimals < 0)
                throw new ArgumentException("decimals < 0 !!");

            int order = 0;
            decimal _size = size;
            while (_size >= factor && order < sizes.Length - 1)
            {
                order++;
                _size = _size / factor;
            }

            var format = "{0:0";
            for (var i = 0; i < decimals; i++)
            {
                if (i == 0)
                    format += ".";
                format += "#";
            }
            format += "} {1}";

            return string.Format(format, _size, sizes[order]);
        }

        /// <summary>
        /// Formatiert eine Zahl mit tausender Trennzeichen
        /// </summary>
        /// <param name="number"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string FormatDelimiter(long number, int delimiterDecimals = 3, string delimiter = ".")
        {
            var temp = $"{number}";
            if (delimiterDecimals <= 0)
            {
                return temp;
            }

            var result = "";
            var cnt = 0;
            for (var i = temp.Length - 1; i >= 0; i--)
            {
                result = temp[i] + result;
                cnt++;
                if (cnt % delimiterDecimals == 0)
                {
                    result = delimiter + result;
                    cnt = 0;
                }
            }
            return result;
        }
    }
}
