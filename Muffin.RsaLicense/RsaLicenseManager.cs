using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Muffin.RsaLicense
{
    public interface ILicense<T>
    {
        T Data { get; set; }
        byte[] Signature { get; set; }
    }

    public static class ILicenseExtensions
    {
        public static byte[] GetData<T>(this ILicense<T> license)
        {
            var data = JsonConvert.SerializeObject(license.Data);
            return Encoding.UTF8.GetBytes(data);
        }
    }

    public class RSALicenseManager<TLicenseWrapper, TLicense>
        where TLicenseWrapper : ILicense<TLicense>
    {
        #region Properties

        public readonly X509RsaLicenseManagerOptions<TLicenseWrapper> Options;

        #endregion

        #region Constructor

        public RSALicenseManager(IServiceProvider serviceProvider)
        {
            Options = serviceProvider?.GetRequiredService<X509RsaLicenseManagerOptions<TLicenseWrapper>>();
        }

        #endregion

        #region Actions

        private string LicensePath => !string.IsNullOrWhiteSpace(Options.LicenseFilePath) ? Path.Combine(Options.LicenseFilePath, Options.LicenseFileName) : Options.LicenseFileName;

        public void AddOrUpdateLicenseJson(string licenseJson)
        {
            File.WriteAllText(LicensePath, licenseJson);
        }

        public string GetLicenseJson()
        {
            if (!File.Exists(LicensePath))
            {
                return null;
            }
            return File.ReadAllText(LicensePath);
        }

        private TLicenseWrapper _cachedLicense;
        public TLicenseWrapper GetLicenseCache()
        {
            if (_cachedLicense == null)
            {
                return GetLicense();
            }
            return _cachedLicense;
        }

        public TLicenseWrapper GetLicense()
        {
            var licenseJson = GetLicenseJson();
            if (string.IsNullOrWhiteSpace(licenseJson))
            {
                return default;
            }

            var license = JsonConvert.DeserializeObject<TLicenseWrapper>(licenseJson);

            var data = license.GetData();
            if (!Options.Rsa.VerifyData(data, license.Signature, Options.HashAlgorithmName, Options.RSASignaturePadding))
            {
                throw new Exception("Invalid License");
            }

            _cachedLicense = license;
            return license;
        }

        public void SaveLicense(ILicense<TLicense> license)
        {
            var licenseJson = JsonConvert.SerializeObject(license);
            File.WriteAllText(LicensePath, licenseJson);
        }

        public bool VerifyLicenseSignature(ILicense<TLicense> license)
        {
            var data = license.GetData();
            return Options.Rsa.VerifyData(data, license.Signature, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        }

        public RSACryptoServiceProvider CreateNewKeyPair()
        {
            return new RSACryptoServiceProvider(4096);
        }

        public void ExportKeyPair(RSACryptoServiceProvider rsa, string publicKeyFilePath, string privateKeyFilePath)
        {
            if (!string.IsNullOrWhiteSpace(publicKeyFilePath))
            {
                var pubKey = rsa.ExportRSAPublicKey();
                File.WriteAllBytes(publicKeyFilePath, pubKey);
            }

            if (!string.IsNullOrWhiteSpace(privateKeyFilePath))
            {
                var privKey = rsa.ExportRSAPrivateKey();
                File.WriteAllBytes(privateKeyFilePath, privKey);
            }
        }

        public byte[] CreateLicenseSignature(ILicense<TLicense> license)
        {
            var data = license.GetData();
            var signature = Options.Rsa.SignData(data, Options.HashAlgorithmName, Options.RSASignaturePadding);
            license.Signature = signature;
            return signature;
        }

        #endregion
    }

    public class X509RsaLicenseManagerOptions<T>
    {
        public HashAlgorithmName HashAlgorithmName { get; internal set; } = HashAlgorithmName.SHA512;
        public RSASignaturePadding RSASignaturePadding { get; internal set; } = RSASignaturePadding.Pkcs1;
        public RSA Rsa { get; private set; } = RSA.Create();
        public string LicenseFileName { get; internal set; } = $"{typeof(T).Name}.lic";
        public string LicenseFilePath { get; internal set; } = AppDomain.CurrentDomain.BaseDirectory;
    }

    public class X509RsaLicenseManagerOptionsBuilder<T>
    {
        internal X509RsaLicenseManagerOptions<T> Options = new X509RsaLicenseManagerOptions<T>();

        public X509RsaLicenseManagerOptionsBuilder<T> UsePublicKeyPath(string path)
        {
            var text = File.ReadAllText(path);
            return UsePublicKey(text);
        }

        public X509RsaLicenseManagerOptionsBuilder<T> UsePublicKey(byte[] bytes)
        {
            Options.Rsa.ImportRSAPublicKey(bytes, out _);
            return this;
        }

        public X509RsaLicenseManagerOptionsBuilder<T> UsePrivateKey(byte[] bytes)
        {
            Options.Rsa.ImportRSAPrivateKey(bytes, out _);
            return this;
        }

        public X509RsaLicenseManagerOptionsBuilder<T> UsePublicKey(string publicKey)
        {
            var pubKey = new string(publicKey);
            pubKey = pubKey.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "");
#warning TODO testen!
            var pubKeyBytes = Convert.FromBase64String(pubKey);
            return UsePublicKey(pubKeyBytes);
        }

        public X509RsaLicenseManagerOptionsBuilder<T> UseEmbeddedPublicKey(Assembly assembly, string name)
        {
            var stream = assembly.GetManifestResourceStream(name);
            using (var reader = new BinaryReader(stream))
            {
                var bytes = reader.ReadBytes((int)stream.Length);
                return UsePublicKey(bytes);
            }
        }

        public X509RsaLicenseManagerOptionsBuilder<T> UseEmbeddedPrivateKey(Assembly assembly, string name)
        {
            var stream = assembly.GetManifestResourceStream(name);
            using (var reader = new BinaryReader(stream))
            {
                var bytes = reader.ReadBytes((int)stream.Length);
                return UsePrivateKey(bytes);
            }
        }

        public X509RsaLicenseManagerOptionsBuilder<T> UseX509Certificate(X509Certificate2 certificate)
        {
            var keyProvider = (RSACryptoServiceProvider)certificate.PublicKey.Key;
            var publicKeyBytes = keyProvider.ExportRSAPublicKey();
            return UsePublicKey(publicKeyBytes);
        }

        public X509RsaLicenseManagerOptionsBuilder<T> UseLicenseFileName(string fileName)
        {
            Options.LicenseFileName = fileName;
            return this;
        }

        public X509RsaLicenseManagerOptionsBuilder<T> UseLicenseFilePath(string path)
        {
            Options.LicenseFilePath = path;
            return this;

        }
    }

    public static class RsaLicenseManagerExtensions
    {
        public static void AddRsaLicenseManager<TLicenseWrapper, TLicense>(this IServiceCollection services, Action<X509RsaLicenseManagerOptionsBuilder<TLicenseWrapper>> options)
            where TLicenseWrapper : ILicense<TLicense>
        {
            var builder = new X509RsaLicenseManagerOptionsBuilder<TLicenseWrapper>();
            options?.Invoke(builder);
            services.AddSingleton(builder.Options);
            services.AddSingleton<RSALicenseManager<TLicenseWrapper, TLicense>>();
        }
    }
}
