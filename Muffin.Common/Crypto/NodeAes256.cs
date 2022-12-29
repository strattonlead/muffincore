using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Muffin.Common.Crypto
{
    public static class NodeAes256
    {
        public static string Decrypt(string keyString, string ivString, string hexEncodedByteArrayData)
        {
            byte[] key = Encoding.ASCII.GetBytes(keyString);
            byte[] iv = Encoding.ASCII.GetBytes(ivString);

            using (var rijndaelManaged = new RijndaelManaged { Key = key, IV = iv, Mode = CipherMode.CBC })
            {
                rijndaelManaged.BlockSize = 128;
                rijndaelManaged.KeySize = 256;
                var bytes = HexStringToByteArray(hexEncodedByteArrayData);
                using (var memoryStream = new MemoryStream(bytes))
                using (var cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                {
                    return new StreamReader(cryptoStream).ReadToEnd();
                }
            }
        }

        public static string Encrypt(string keyString, string ivString, string data)
        {
            byte[] key = Encoding.ASCII.GetBytes(keyString);
            byte[] iv = Encoding.ASCII.GetBytes(ivString);

            var bytesToBeEncrypted = Encoding.UTF8.GetBytes(data);
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged { Key = key, IV = iv, Mode = CipherMode.CBC })
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    var encryptedBytes = ms.ToArray();
                    return ByteArrayToHexString(encryptedBytes);
                }
            }
        }

        public static T DecryptJson<T>(string keyString, string ivString, string base64EncodedEncryptedData)
        {
            var decrypted = Decrypt(keyString, ivString, base64EncodedEncryptedData);
            return JsonConvert.DeserializeObject<T>(decrypted);
        }

        public static string EncryptJson<T>(string keyString, string ivString, T obj)
        {
            var serialized = JsonConvert.SerializeObject(obj);
            return Encrypt(keyString, ivString, serialized);
        }

        public static string ByteArrayToHexString(byte[] Bytes)
        {
            StringBuilder Result = new StringBuilder(Bytes.Length * 2);
            string HexAlphabet = "0123456789ABCDEF";

            foreach (byte B in Bytes)
            {
                Result.Append(HexAlphabet[(int)(B >> 4)]);
                Result.Append(HexAlphabet[(int)(B & 0xF)]);
            }

            return Result.ToString();
        }

        public static byte[] HexStringToByteArray(string Hex)
        {
            byte[] Bytes = new byte[Hex.Length / 2];
            int[] HexValue = new int[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05,
       0x06, 0x07, 0x08, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
       0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };

            for (int x = 0, i = 0; i < Hex.Length; i += 2, x += 1)
            {
                Bytes[x] = (byte)(HexValue[Char.ToUpper(Hex[i + 0]) - '0'] << 4 |
                                  HexValue[Char.ToUpper(Hex[i + 1]) - '0']);
            }

            return Bytes;
        }
    }
}
