using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Muffin.Common.Crypto
{
    public static class X509CertificateExtensions
    {
        public static string Encrypt(this X509Certificate2 cert, string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = cert.Encrypt(plainBytes);
            string encryptedText = Convert.ToBase64String(encryptedBytes);
            return encryptedText;
        }

        public static byte[] Encrypt(this X509Certificate2 cert, byte[] plainBytes)
        {
            var publicKey = (RSACryptoServiceProvider)cert.PublicKey.Key;
            return publicKey.Encrypt(plainBytes, false);
        }

        public static string Decrypt(this X509Certificate2 cert, string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] decryptedBytes = cert.Decrypt(encryptedBytes);
            string decryptedText = Encoding.UTF8.GetString(decryptedBytes);
            return decryptedText;
        }

        public static byte[] Decrypt(this X509Certificate2 cert, byte[] encryptedBytes)
        {
            var privateKey = (RSACryptoServiceProvider)cert.PrivateKey;
            return privateKey.Decrypt(encryptedBytes, false);
        }
    }
}
