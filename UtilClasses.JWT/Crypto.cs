using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace UtilClasses.JWT
{
    public static class Crypto
    {
        public  class ParameterPasswordFinder : IPasswordFinder
        {
            private readonly string _passwd;
            public ParameterPasswordFinder(string passwd)
            {
                _passwd = passwd;
            }

            public char[] GetPassword()
            {
                return _passwd.ToCharArray();
            }
        }
        public static RsaSecurityKey GetKey(string pemContent, string password)
        {

            using (var stringReader = new StringReader(pemContent))
            {
                var pemReader = new PemReader(stringReader, new ParameterPasswordFinder(password));
                var obj = pemReader.ReadObject();
                var keyParams = obj as AsymmetricKeyParameter;
                if (keyParams != null)
                {
                    var rsaKeyParams = keyParams as RsaPrivateCrtKeyParameters;
                    var rsaParameters = DotNetUtilities.ToRSAParameters(rsaKeyParams);
                    var rsa = new RSACryptoServiceProvider(4096);
                    rsa.ImportParameters(rsaParameters);
                    return new RsaSecurityKey(rsa);
                }
                var cert = obj as X509Certificate;
                if (cert != null)
                {
                    var cert1 = DotNetUtilities.ToX509Certificate(cert);
                    var cert2 = new X509Certificate2(cert1);
                    var provider = cert2.PublicKey.Key as RSACryptoServiceProvider;
                    return new RsaSecurityKey(provider);
                }
                throw new Exception("Could not find, parse, cast, trim and polish an RsaSecurityKey from the provided information...");
            }
        }
    }
}
