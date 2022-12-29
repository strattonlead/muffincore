using System;
using System.Linq;
using System.Security.Cryptography;

namespace Muffin.Common.Util
{
    public static class SecureRandom
    {
        public static long AbsLong()
        {
            return Math.Abs(SecureRandom.NextLong());
        }

        public static int NextInt()
        {
            var rng = new RNGCryptoServiceProvider();
            var rndBytes = new byte[4];
            rng.GetBytes(rndBytes);
            return BitConverter.ToInt32(rndBytes, 0);
        }

        public static int NextInt(int minValue, int maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentException("maxValue must be greater than minValue");

            if (minValue == maxValue)
                return minValue;

            maxValue += 1;
            Int64 diff = maxValue - minValue;

            var rng = new RNGCryptoServiceProvider();
            var _uint32Buffer = new byte[4];
            while (true)
            {
                rng.GetBytes(_uint32Buffer);
                UInt32 rand = BitConverter.ToUInt32(_uint32Buffer, 0);

                Int64 max = (1 + (Int64)UInt32.MaxValue);
                Int64 remainder = max % diff;
                if (rand < max - remainder)
                {
                    return (Int32)(minValue + (rand % diff));
                }
            }
        }

        public static uint NextUInt()
        {
            var rng = new RNGCryptoServiceProvider();
            var rndBytes = new byte[4];
            rng.GetBytes(rndBytes);
            return BitConverter.ToUInt32(rndBytes, 0);
        }

        public static long NextLong()
        {
            var rng = new RNGCryptoServiceProvider();
            var rndBytes = new byte[8];
            rng.GetBytes(rndBytes);
            return BitConverter.ToInt64(rndBytes, 0);
        }

        public static ulong NextULong()
        {
            var rng = new RNGCryptoServiceProvider();
            var rndBytes = new byte[8];
            rng.GetBytes(rndBytes);
            return BitConverter.ToUInt64(rndBytes, 0);
        }

        public static string NextHexString(int length)
        {
            if (length < 0)
                throw new ArgumentException("length < 0");

            const string pool = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var chars = Enumerable.Range(0, length)
                .Select(x => pool[new Random().Next(0, pool.Length)]);
            return new string(chars.ToArray());
        }

        public static string NextString(int length)
        {
            if (length < 0)
            {
                throw new ArgumentException("length < 0");
            }

            const string pool = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!\"§$%&/()=?+#'*.,;:-_";
            var chars = Enumerable.Range(0, length)
                .Select(x => pool[new Random().Next(0, pool.Length)]);
            return new string(chars.ToArray());
        }

        public static string NextAlphanumericString(int length)
        {
            if (length < 0)
            {
                throw new ArgumentException("length < 0");
            }

            const string pool = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var chars = Enumerable.Range(0, length)
                .Select(x => pool[new Random().Next(0, pool.Length)]);
            return new string(chars.ToArray());
        }
    }
}
