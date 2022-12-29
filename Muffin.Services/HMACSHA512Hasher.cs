using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Muffin.Services
{
    public class HMACSHA512Hasher
    {
        public void CreateHash(string key, out byte[] pinHash, out byte[] pinSalt)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "key");

            using (var hmac = new HMACSHA512())
            {
                pinSalt = hmac.Key;
                pinHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(key));
            }

            //var correct = VerifyHash(key, pinHash, pinSalt);
        }

        public bool VerifyHash(string key, byte[] storedHash, byte[] storedSalt)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "key");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of key hash (64 bytes expected).", "pinHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of key salt (128 bytes expected).", "pinHash");

            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(key));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }

    public static class HMACSHA512HasherExtensions
    {
        public static void AddHMACSHA512Hasher(this IServiceCollection services)
        {
            services.AddSingleton<HMACSHA512Hasher>();
        }
    }
}
