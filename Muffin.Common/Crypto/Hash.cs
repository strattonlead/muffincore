using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace Muffin.Common.Crypto
{
    public static class Hash
    {
        public static string SHA1(Stream s)
        {
            HashAlgorithm hash = new SHA1Managed();
            byte[] hashBytes = hash.ComputeHash(s);
            string hashValue = BitConverter.ToString(hashBytes);
            return hashValue.Replace("-", "").ToLower();
        }

        public static string SHA1(byte[] b)
        {
            HashAlgorithm hash = new SHA1Managed();
            byte[] hashBytes = hash.ComputeHash(b);
            string hashValue = BitConverter.ToString(hashBytes);
            return hashValue.Replace("-", "").ToLower();
        }

        public static string SHA1(string s)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(s);
            HashAlgorithm hash = new SHA1Managed();
            byte[] hashBytes = hash.ComputeHash(plainTextBytes);
            string hashValue = BitConverter.ToString(hashBytes);
            return hashValue.Replace("-", "").ToLower();
        }

        public static string MD5(string s)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(s);
            var hash = System.Security.Cryptography.MD5.Create();
            var hashBytes = hash.ComputeHash(plainTextBytes);
            return BitConverter.ToString(hashBytes)
                .Replace("-", "")
                .ToLower();
        }

        public static string SHA256(string s)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(s);
            using (var hash = new SHA256Managed())
            {
                var hashBytes = hash.ComputeHash(plainTextBytes);
                var hashValue = BitConverter.ToString(hashBytes);
                return hashValue.Replace("-", "").ToLower();
            }
        }

        public static byte[] SHA256(byte[] bytes)
        {
            using (var hash = new SHA256Managed())
                return hash.ComputeHash(bytes);
        }

        #region HMACSHA512

        public static string HMACSHA512(string s)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(s);
            using (HMACSHA512 hash = new HMACSHA512())
            {
                var hashBytes = hash.ComputeHash(plainTextBytes);
                var hashValue = BitConverter.ToString(hashBytes);
                return hashValue.Replace("-", "").ToLower();
            }
        }

        public static string HMACSHA512(string s, string key)
        {
            var byteKey = Encoding.UTF8.GetBytes(key);
            var plainTextBytes = Encoding.UTF8.GetBytes(s);
            using (HMACSHA512 hash = new HMACSHA512(byteKey))
            {
                var hashBytes = hash.ComputeHash(plainTextBytes);
                var hashValue = BitConverter.ToString(hashBytes);
                return hashValue.Replace("-", "").ToLower();
            }
        }

        #endregion

        #region HMACSHA384

        public static string HMACSHA384(string s)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(s);
            using (HMACSHA384 hash = new HMACSHA384())
            {
                var hashBytes = hash.ComputeHash(plainTextBytes);
                var hashValue = BitConverter.ToString(hashBytes);
                return hashValue.Replace("-", "").ToLower();
            }
        }

        public static string HMACSHA384(string s, string key)
        {
            var byteKey = Encoding.UTF8.GetBytes(key);
            var plainTextBytes = Encoding.UTF8.GetBytes(s);
            using (HMACSHA384 hash = new HMACSHA384(byteKey))
            {
                var hashBytes = hash.ComputeHash(plainTextBytes);
                var hashValue = BitConverter.ToString(hashBytes);
                return hashValue.Replace("-", "").ToLower();
            }
        }

        #endregion

        #region HMACSHA1

        public static string HMACSHA1(string s)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(s);
            using (HMACSHA1 hash = new HMACSHA1())
            {
                var hashBytes = hash.ComputeHash(plainTextBytes);
                var hashValue = BitConverter.ToString(hashBytes);
                return hashValue.Replace("-", "").ToLower();
            }
        }

        public static string HMACSHA1(string s, string key)
        {
            var byteKey = Encoding.UTF8.GetBytes(key);
            var plainTextBytes = Encoding.UTF8.GetBytes(s);
            using (HMACSHA1 hash = new HMACSHA1(byteKey))
            {
                var hashBytes = hash.ComputeHash(plainTextBytes);
                var hashValue = BitConverter.ToString(hashBytes);
                return hashValue.Replace("-", "").ToLower();
            }
        }

        #endregion
    }


}
