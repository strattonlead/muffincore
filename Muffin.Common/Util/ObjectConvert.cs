using System;

namespace Muffin.Common.Util
{
    public static class ObjectConvert
    {
        public static object ChangeType(string str, Type type)
        {
            if (type == typeof(string))
            {
                return str;
            }

            if (type == typeof(byte))
            {
                return byte.Parse(str);
            }

            if (type == typeof(byte?))
            {
                return byte.Parse(str);
            }

            if (type == typeof(short))
            {
                return short.Parse(str);
            }

            if (type == typeof(short?))
            {
                return short.Parse(str);
            }

            if (type == typeof(int))
            {
                return int.Parse(str);
            }

            if (type == typeof(int?))
            {
                return int.Parse(str);
            }

            if (type == typeof(long))
            {
                return long.Parse(str);
            }

            if (type == typeof(long?))
            {
                return long.Parse(str);
            }

            if (type == typeof(double))
            {
                return double.Parse(str);
            }

            if (type == typeof(double?))
            {
                return double.Parse(str);
            }

            if (type == typeof(decimal))
            {
                return decimal.Parse(str);
            }

            if (type == typeof(decimal?))
            {
                return decimal.Parse(str);
            }

            try
            {
                return Convert.ChangeType(str, type);
            }
            catch { }

            return null;
        }
    }
}
