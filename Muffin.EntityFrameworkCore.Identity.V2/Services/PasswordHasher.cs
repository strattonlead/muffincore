using System;
using System.Security.Cryptography;
using System.Text;

namespace Muffin.EntityFrameworkCore.Identity.V2.Services
{
    public class PasswordHasher
    {
        #region Properties

        #endregion

        #region Constructor

        #endregion

        #region Actions

        public string ComputeSecureHash(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var result = ComputeSecureHash(bytes);
            string hashValue = BitConverter.ToString(result);
            return hashValue.Replace("-", "").ToLower();
        }

        public byte[] ComputeSecureHash(byte[] input)
        {
            var sha = SHA256.Create();
            return sha.ComputeHash(input);
        }

        #endregion
    }
}
