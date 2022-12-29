using Microsoft.VisualStudio.TestTools.UnitTesting;
using Muffin.RsaLicense;
using System;
using System.IO;
using System.Reflection;

namespace Muffin.Tests.Util
{
    [TestClass]
    public class SshKeyPairTests
    {
        [TestMethod]
        public void CreateSshKeysTest()
        {
            var keygen = new SshKeyGenerator.SshKeyGenerator(2048);

            var privateKey = keygen.ToPrivateKey();
            Console.WriteLine(privateKey);

            var publicSshKey = keygen.ToRfcPublicKey();
            Console.WriteLine(publicSshKey);

            var publicSshKeyWithComment = keygen.ToRfcPublicKey("user@domain.com");

            var pubPath = Path.Combine(Assembly.GetExecutingAssembly().Location, "..\\out.pubkey");
            var privPath = Path.Combine(Assembly.GetExecutingAssembly().Location, "..\\out.privkey");
            var privComPath = Path.Combine(Assembly.GetExecutingAssembly().Location, "..\\out.privkeycom");
            File.WriteAllText(pubPath, publicSshKey);
            File.WriteAllText(privPath, privateKey);
            File.WriteAllText(privComPath, publicSshKeyWithComment);
        }
    }

    public class __Lic { }

    public class __LicWrapper : ILicense<__Lic>
    {
        public __Lic Data { get; set; }
        public byte[] Signature { get; set; }
    }
}
