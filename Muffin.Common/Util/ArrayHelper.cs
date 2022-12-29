using System;
using System.IO;

namespace Muffin.Common.Util
{
    public static class ArrayHelper
    {
        public static T[] Initialize<T>(int size, Func<int, T> func)
        {
            if (size < 0)
            {
                throw new ArgumentException("Size must be greater than 0");
            }
            var result = new T[size];
            if (func != null)
            {
                for (var i = 0; i < result.Length; i++)
                {
                    result[i] = func(i);
                }
            }
            return result;
        }

        public static Stream ToStream(this byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            var mem = new MemoryStream(bytes);
            mem.Seek(0, SeekOrigin.Begin);
            return mem;
        }
    }
}
