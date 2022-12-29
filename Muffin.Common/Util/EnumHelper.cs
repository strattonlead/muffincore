using System;
using System.Collections.Generic;
using System.Linq;

namespace Muffin.Common.Util
{
    public static class EnumHelper
    {
        public static object[] EnumList(Type enumType)
        {
            return Enum.GetValues(enumType).Cast<object>().ToArray();
        }

        public static T[] EnumList<T>() where T : struct, IConvertible
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }

        public static T[] SplitEnum<T>(this T input)
            where T : struct, IConvertible
        {
            var allValues = EnumList<T>();
            var result = new List<T>();
            foreach (var e in allValues)
            {
                int _e = (int)(object)e;
                int _i = (int)(object)input;
                if ((_e & _i) == _e)
                {
                    result.Add(e);
                }
            }
            return result.ToArray();
        }

        public static T ReduceEnum<T>(this T[] input)
            where T : struct, IConvertible
        {
            if (input == null || input.Length == 0)
            {
                return default(T);
            }
            int result = (int)(object)input.FirstOrDefault();
            for (var i = 1; i < input.Length; i++)
            {
                result |= (int)(object)input[i];
            }
            return (T)(object)result;
        }

        public static T[] FlagCombinations<T>(T neccesaryFlag)
            where T : struct, IConvertible
        {
            var flags = EnumList<T>()
                .Select(x => (int)(object)x)
                .ToArray();

            var temp = new List<T>();
            foreach (var i in flags)
            {
                var t = i | (int)(object)neccesaryFlag;
                foreach (var j in flags)
                {
                    t |= j;
                }
                temp.Add((T)(object)t);
            }

            var result = temp
                .Distinct()
                .ToArray();
            return result;
        }
    }
}
