using System;
using System.Collections;
using System.Collections.Generic;

namespace Muffin.Common.Util
{
    public static class BitHelper
    {
        public static long GetInt64(BitArray array)
        {
            var value = new long[1];
            array.CopyTo(value, 0);
            return value[0];
        }

        public static BitArray GetBitArray(long value)
        {
            return new BitArray(BitConverter.GetBytes(value));
        }

        public static long[] SplitPowerOf2(long value)
        {
            var values = GetBitArray(value);
            var result = new List<long>();
            for (int i = 0; i < values.Length; i++)
            {
                var v = values[i];
                if (v)
                    result.Add((long)Math.Pow(2, i));
            }
            return result.ToArray();
        }
    }
}
