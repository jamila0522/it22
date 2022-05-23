using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Prng;
using System.Xml;

namespace SCB.PMTGWT.Utils
{
    public class RSA
    {
        /// <summary>
        /// RsaCrypto
        /// </summary>
        /// <param name="plainTextData"></param>
        /// <returns></returns>
        public string RsaCrypto(string plainTextData)
        {
            //lets take a new CSP with a new 2048 bit rsa key pair
            var csp = new RSACryptoServiceProvider(2048);

            //how to get the private key
            var privKey = csp.ExportParameters(true);

            //and the public key ...
            var pubKey = csp.ExportParameters(false);

            //converting the public key into a string representation
            string pubKeyString;
            {
                //we need some buffer
                var sw = new System.IO.StringWriter();
                //we need a serializer
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //serialize the key into the stream
                xs.Serialize(sw, pubKey);
                //get the string from the stream
                pubKeyString = sw.ToString();
            }

            //converting it back
            {
                //get a stream from the string
                var sr = new System.IO.StringReader(pubKeyString);
                //we need a deserializer
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //get the object back from the stream
                pubKey = (RSAParameters)xs.Deserialize(sr);
            }

            //conversion for the private key is no black magic either ... omitted

            //we have a public key ... let's get a new csp and load that key
            csp = new RSACryptoServiceProvider();
            csp.ImportParameters(pubKey);

            //for encryption, always handle bytes...
            var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(plainTextData);

            //apply pkcs#1.5 padding and encrypt our data 
            var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);

            //we might want a string representation of our cypher text... base64 will do
            var cypherText = Convert.ToBase64String(bytesCypherText);

            /*
             * some transmission / storage / retrieval
             * 
             * and we want to decrypt our cypherText
             */

            //first, get our bytes back from the base64 string ...
            bytesCypherText = Convert.FromBase64String(cypherText);

            //we want to decrypt, therefore we need a csp and load our private key
            csp = new RSACryptoServiceProvider();
            csp.ImportParameters(privKey);

            //decrypt and strip pkcs#1.5 padding
            bytesPlainTextData = csp.Decrypt(bytesCypherText, false);

            //get our original plainText back...
            plainTextData = System.Text.Encoding.Unicode.GetString(bytesPlainTextData);

            return plainTextData;
        }

        /// <summary>
        /// Encryption
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static string Encryption(string strText)
        {
            var publicKey =
            //2048
            "<RSAKeyValue><Modulus>8Em/AITpT8Dj4rNUkqcMgf9L8DeiDtZTyoqdUcsqjnriKJJAoJj4rPvlFSDQsmBbS0dT73sYHCyn4Ay3dHZqYsSRt0KQJCRzV9rxfXKHLmQdBF49HmO55T+CxMgoV7ajDK2ui3BCjEfbpEnfkII2cVfTeIv3zMX68JzJd6OTwID/NJLxEG4+UCC3cR1C4s9gU6E4WfQ446BgL7gcIG773MCUJFff8fztLVIuoCMJkcXxBl1OGn+TWkh7vpRyp1K0tYiUcfDmPmyFT1JSXyrpYPDP8BKECMlhcXcLA9LlbJo2g93mWFVP4BpVFp5ENBAZ+DZt6haSHLHhWyLqC4SGgQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            //1024"<RSAKeyValue><Modulus>21wEnTU+mcD2w0Lfo1Gv4rtcSWsQJQTNa6gio05AOkV/Er9w3Y13Ddo5wGtjJ19402S71HUeN0vbKILLJdRSES5MHSdJPSVrOqdrll/vLXxDxWs/U0UT1c8u6k/Ogx9hTtZxYwoeYqdhDblof3E75d9n2F0Zvf6iTb4cI7j6fMs=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

            var testData = Encoding.UTF8.GetBytes(strText);

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    // client encrypting data with public key issued by server                    
                    rsa.FromXmlString(publicKey.ToString());

                    var encryptedData = rsa.Encrypt(testData, true);

                    var base64Encrypted = Convert.ToBase64String(encryptedData);

                    return base64Encrypted;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        /// <summary>
        /// Decryption
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static string Decryption(string strText)
        {
            var privateKey = //2048
                "<RSAKeyValue><Modulus>8Em/AITpT8Dj4rNUkqcMgf9L8DeiDtZTyoqdUcsqjnriKJJAoJj4rPvlFSDQsmBbS0dT73sYHCyn4Ay3dHZqYsSRt0KQJCRzV9rxfXKHLmQdBF49HmO55T+CxMgoV7ajDK2ui3BCjEfbpEnfkII2cVfTeIv3zMX68JzJd6OTwID/NJLxEG4+UCC3cR1C4s9gU6E4WfQ446BgL7gcIG773MCUJFff8fztLVIuoCMJkcXxBl1OGn+TWkh7vpRyp1K0tYiUcfDmPmyFT1JSXyrpYPDP8BKECMlhcXcLA9LlbJo2g93mWFVP4BpVFp5ENBAZ+DZt6haSHLHhWyLqC4SGgQ==</Modulus><Exponent>AQAB</Exponent><P>8l8UaDHs4bjrXDAhrCleVcsXzl7US5lDUNQK29xDmUer03stFBBFVOn4eTog4EuSjI9ETdX8L2JgsKGTqXT2FrjHybY2tMjpsHkSUY2HEIurVdR+PJwsLPRbLG6b7DYdqdTMRHVCXfH5OPDlQz0yjlTO3v4h6aFd/Qfc2uaVOAM=</P><Q>/cytSwY54ggQgqtEry+cYxXUtQNQsHHCQH1HA1lIt9Ioy/lDry2CWXC2ggRe4qeR/uRmxVvCyIhjjKVmixMGZg58C6l51lV7+HZU18/8e7HJ991u/D4B1AFBqQCOqSkGoNTSMJQVN7tMdofpJc7CBJivf2ijoFDVyK7FvUlsCis=</Q><DP>EuyDjmMfiLxEfC0x49xTBkXWNQHId6Ke0+fKdUIscQJckyWR7ngawbeG9Agq2vhrl9fz0FP93cFDpWboPZnpQPrqdoBooxLw1BuP55gyTINrPEdcF+VluZjWqZB1Uisjg8gertWs+eLTv7NnYymbEevjPxv+j9xOl1wOzXSlf/s=</DP><DQ>y2+o6J5Q/3Ffap5GK/aJ8JKrMX06zHQjNPY2WeTTeSUoNnfTXTOpbHAAmy9Nr3iKejoydrga5gsNYMuj6o6RbnTl0aNDWh6jhIG1eFfrFmtobRo9wEsjxABN1V3w/H86JOjyoqXQbB/iJpDCvd+BfZW19va1Uef5tScCsbuMsok=</DQ><InverseQ>lrqAYdiso7QUuBOzRF79wuNKykD+uGL7pyoJuDCdlcIG2/At6kPUj6WFGJiKKNnek4kTbLpcOS8h4KbMliiH/QsmQKCiRMvCcQM/UCRVAKPcZlPf1evRHXpHJ0JA714hcuVEE3LrFPaNBTGjqIvY72H4hHr14I8X8D+kPgWS3zU=</InverseQ><D>YUVZPrqPQCvTlJgH6n4ZQB66/Plobx0wOlO2r7/Q8HCqUMYzx09li5FFATKRiUQccPLk9ZMdBXrGUpqTQWAWXPk6C7e0Zg0jbDNDgwbz1kHMxVHo5/5y6Gy3YTWtryROCPneKLA/IE6OiYkmKXes1mE5SBlRLOIQXYr6u6XKUCifS3B42q7E3GGME79BJ7igOH0SrSaFeSHiEIHOsz8i5VljajniksjjL53FVQcltV2coq/nzQT7Zz9fkkcsUr8yURAfUSIgQWzFlUa7COJZf92lscajErDvc/O3sQAZt/k+itEX62QnFiCjEqlsDGZoWlx8WypYZes7AXK4sGYJKQ==</D></RSAKeyValue>";
            //1024"<RSAKeyValue><Modulus>21wEnTU+mcD2w0Lfo1Gv4rtcSWsQJQTNa6gio05AOkV/Er9w3Y13Ddo5wGtjJ19402S71HUeN0vbKILLJdRSES5MHSdJPSVrOqdrll/vLXxDxWs/U0UT1c8u6k/Ogx9hTtZxYwoeYqdhDblof3E75d9n2F0Zvf6iTb4cI7j6fMs=</Modulus><Exponent>AQAB</Exponent><P>/aULPE6jd5IkwtWXmReyMUhmI/nfwfkQSyl7tsg2PKdpcxk4mpPZUdEQhHQLvE84w2DhTyYkPHCtq/mMKE3MHw==</P><Q>3WV46X9Arg2l9cxb67KVlNVXyCqc/w+LWt/tbhLJvV2xCF/0rWKPsBJ9MC6cquaqNPxWWEav8RAVbmmGrJt51Q==</Q><DP>8TuZFgBMpBoQcGUoS2goB4st6aVq1FcG0hVgHhUI0GMAfYFNPmbDV3cY2IBt8Oj/uYJYhyhlaj5YTqmGTYbATQ==</DP><DQ>FIoVbZQgrAUYIHWVEYi/187zFd7eMct/Yi7kGBImJStMATrluDAspGkStCWe4zwDDmdam1XzfKnBUzz3AYxrAQ==</DQ><InverseQ>QPU3Tmt8nznSgYZ+5jUo9E0SfjiTu435ihANiHqqjasaUNvOHKumqzuBZ8NRtkUhS6dsOEb8A2ODvy7KswUxyA==</InverseQ><D>cgoRoAUpSVfHMdYXW9nA3dfX75dIamZnwPtFHq80ttagbIe4ToYYCcyUz5NElhiNQSESgS5uCgNWqWXt5PnPu4XmCXx6utco1UVH8HGLahzbAnSy6Cj3iUIQ7Gj+9gQ7PkC434HTtHazmxVgIR5l56ZjoQ8yGNCPZnsdYEmhJWk=</D></RSAKeyValue>";

            var testData = Encoding.UTF8.GetBytes(strText);

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    var base64Encrypted = strText;

                    // server decrypting data with private key                    
                    rsa.FromXmlString(privateKey);

                    var resultBytes = Convert.FromBase64String(base64Encrypted);
                    var decryptedBytes = rsa.Decrypt(resultBytes, true);
                    var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                    return decryptedData.ToString();
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        /// <summary>
        /// _optimalAsymmetricEncryptionPadding
        /// </summary>
        private static bool _optimalAsymmetricEncryptionPadding = false;

        /// <summary>
        /// GenerateKeys
        /// </summary>
        /// <param name="keySize"></param>
        /// <returns></returns>
        public static EncryptorRSAKeys GenerateKeys(int keySize)
        {
            if (keySize % 2 != 0 || keySize < 512)
                throw new Exception("Key should be multiple of two and greater than 512.");

            var response = new EncryptorRSAKeys();

            using (var provider = new RSACryptoServiceProvider(keySize))
            {
                var publicKey = provider.ToXmlString(false);
                var privateKey = provider.ToXmlString(true);

                var publicKeyWithSize = IncludeKeyInEncryptionString(publicKey, keySize);
                var privateKeyWithSize = IncludeKeyInEncryptionString(privateKey, keySize);

                response.PublicKey = publicKeyWithSize;
                response.PrivateKey = privateKeyWithSize;
            }

            return response;
        }

        /// <summary>
        /// EncryptText
        /// </summary>
        /// <param name="text"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public string EncryptText(string text, string publicKey)
        {
            int keySize = 0;
            string publicKeyXml = "";

            GetKeyFromEncryptionString(publicKey, out keySize, out publicKeyXml);

            var encrypted = Encrypt(Encoding.UTF8.GetBytes(text), keySize, publicKeyXml);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Encrypt
        /// </summary>
        /// <param name="data"></param>
        /// <param name="keySize"></param>
        /// <param name="publicKeyXml"></param>
        /// <returns></returns>
        private byte[] Encrypt(byte[] data, int keySize, string publicKeyXml)
        {
            if (data == null || data.Length == 0) throw new ArgumentException("Data are empty", "data");
            int maxLength = GetMaxDataLength(keySize);
            if (data.Length > maxLength) throw new ArgumentException(String.Format("Maximum data length is {0}", maxLength), "data");
            if (!IsKeySizeValid(keySize)) throw new ArgumentException("Key size is not valid", "keySize");
            if (String.IsNullOrEmpty(publicKeyXml)) throw new ArgumentException("Key is null or empty", "publicKeyXml");

            using (var provider = new RSACryptoServiceProvider(keySize))
            {
                provider.FromXmlString(publicKeyXml);
                return provider.Encrypt(data, _optimalAsymmetricEncryptionPadding);
            }
        }

        /// <summary>
        /// DecryptText
        /// </summary>
        /// <param name="text"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string DecryptText(string text, string privateKey)
        {
            int keySize = 0;
            string publicAndPrivateKeyXml = "";

            GetKeyFromEncryptionString(privateKey, out keySize, out publicAndPrivateKeyXml);

            var decrypted = Decrypt(Convert.FromBase64String(text), keySize, publicAndPrivateKeyXml);
            return Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// Decrypt
        /// </summary>
        /// <param name="data"></param>
        /// <param name="keySize"></param>
        /// <param name="publicAndPrivateKeyXml"></param>
        /// <returns></returns>
        private static byte[] Decrypt(byte[] data, int keySize, string publicAndPrivateKeyXml)
        {
            if (data == null || data.Length == 0) throw new ArgumentException("Data are empty", "data");
            if (!IsKeySizeValid(keySize)) throw new ArgumentException("Key size is not valid", "keySize");
            if (String.IsNullOrEmpty(publicAndPrivateKeyXml)) throw new ArgumentException("Key is null or empty", "publicAndPrivateKeyXml");

            using (var provider = new RSACryptoServiceProvider(keySize))
            {
                provider.FromXmlString(publicAndPrivateKeyXml);
                return provider.Decrypt(data, _optimalAsymmetricEncryptionPadding);
            }
        }

        /// <summary>
        /// GetMaxDataLength
        /// </summary>
        /// <param name="keySize"></param>
        /// <returns></returns>
        public static int GetMaxDataLength(int keySize)
        {
            if (_optimalAsymmetricEncryptionPadding)
            {
                return ((keySize - 384) / 8) + 7;
            }
            return ((keySize - 384) / 8) + 37;
        }

        /// <summary>
        /// IsKeySizeValid
        /// </summary>
        /// <param name="keySize"></param>
        /// <returns></returns>
        public static bool IsKeySizeValid(int keySize)
        {
            return keySize >= 384 &&
                    keySize <= 16384 &&
                    keySize % 8 == 0;
        }

        /// <summary>
        /// IncludeKeyInEncryptionString
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="keySize"></param>
        /// <returns></returns>
        private static string IncludeKeyInEncryptionString(string publicKey, int keySize)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(keySize.ToString() + "!" + publicKey));
        }

        /// <summary>
        /// GetKeyFromEncryptionString
        /// </summary>
        /// <param name="rawkey"></param>
        /// <param name="keySize"></param>
        /// <param name="xmlKey"></param>
        private static void GetKeyFromEncryptionString(string rawkey, out int keySize, out string xmlKey)
        {
            keySize = 0;
            xmlKey = "";

            if (rawkey != null && rawkey.Length > 0)
            {
                byte[] keyBytes = Convert.FromBase64String(rawkey);
                var stringKey = Encoding.UTF8.GetString(keyBytes);

                if (stringKey.Contains("!"))
                {
                    var splittedValues = stringKey.Split(new char[] { '!' }, 2);

                    try
                    {
                        keySize = int.Parse(splittedValues[0]);
                        xmlKey = splittedValues[1];
                    }
                    catch (Exception e) { throw e; }
                }
            }
        }
    }

    [Serializable]
    public class EncryptorRSAKeys
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }

    public static class SecureCom
    {
        public static string[] Base64EncodeRSAKey()
        {
            var _GenerateKeys = RSA.GenerateKeys(512);
            string[] results = { _GenerateKeys.PublicKey, _GenerateKeys.PrivateKey };
            return results;
        }

        public static string Base64Encode(string inputString, ref string outString)
        {

            outString = RSA.Encryption(inputString);
            return "";
        }

        public static string Base64Decode(string inputString, ref string outString)
        {
            outString = RSA.Decryption(inputString);
            return "";
        }

        public static string AESStringEncryption(string passPhrase, string inputString, ref string outString)
        {

            outString = RSA.Encryption(inputString);

            return "";
        }

        public static string AESStringDecryption(string passPhrase, string inputString, ref string outString)
        {
            outString = RSA.Decryption(inputString);

            return "";
        }
    }

    public static class X509Certificate2Helper
    {
        //Public domain: No attribution required.

        /// <summary>
        /// Returns the base64 encoded Public Key Pinning hash of the certificate.
        /// </summary>
        /// <param name="Certificate"></param>
        /// <returns>A base-64 encoded string that represents the hash of the certifcate's SubjectPublicKeyInfo</returns>
        public static String GetPublicKeyPinningHash(this X509Certificate2 Certificate)
        {
            //Get the SubjectPublicKeyInfo member of the certificate
            Byte[] subjectPublicKeyInfo = GetSubjectPublicKeyInfoRaw(Certificate);

            //Take the SHA2-256 hash of the DER ASN.1 encoded value
            Byte[] digest;
            using (var sha2 = new SHA256Managed())
            {
                digest = sha2.ComputeHash(subjectPublicKeyInfo);
            }

            //Convert hash to base64
            String hash = Convert.ToBase64String(digest);

            return hash;
        }

        /// <summary>
        /// Returns the raw ASN.1 DER bytes of the certificate's SubjectPublicKeyInfo section
        /// </summary>
        /// <param name="Certificate"></param>
        /// <returns>A byte array containing ASN.1 DER encoded SubjectPublicKeyInfo</returns>
        public static Byte[] GetSubjectPublicKeyInfoRaw(this X509Certificate2 Certificate)
        {
            //Public domain: No attribution required.
            Byte[] rawCert = Certificate.GetRawCertData();

            /*
             Certificate is, by definition:

                Certificate  ::=  SEQUENCE  {
                    tbsCertificate       TBSCertificate,
                    signatureAlgorithm   AlgorithmIdentifier,
                    signatureValue       BIT STRING  
                }

               TBSCertificate  ::=  SEQUENCE  {
                    version         [0]  EXPLICIT Version DEFAULT v1,
                    serialNumber         CertificateSerialNumber,
                    signature            AlgorithmIdentifier,
                    issuer               Name,
                    validity             Validity,
                    subject              Name,
                    subjectPublicKeyInfo SubjectPublicKeyInfo,
                    issuerUniqueID  [1]  IMPLICIT UniqueIdentifier OPTIONAL, -- If present, version MUST be v2 or v3
                    subjectUniqueID [2]  IMPLICIT UniqueIdentifier OPTIONAL, -- If present, version MUST be v2 or v3
                    extensions      [3]  EXPLICIT Extensions       OPTIONAL  -- If present, version MUST be v3
                }

            So we walk to ASN.1 DER tree in order to drill down to the SubjectPublicKeyInfo item
            */
            Byte[] list = AsnNext(ref rawCert, true); //unwrap certificate sequence
            Byte[] tbsCertificate = AsnNext(ref list, false); //get next item; which is tbsCertificate
            list = AsnNext(ref tbsCertificate, true); //unwap tbsCertificate sequence

            Byte[] version = AsnNext(ref list, false); //tbsCertificate.Version
            Byte[] serialNumber = AsnNext(ref list, false); //tbsCertificate.SerialNumber
            Byte[] signature = AsnNext(ref list, false); //tbsCertificate.Signature
            Byte[] issuer = AsnNext(ref list, false); //tbsCertificate.Issuer
            Byte[] validity = AsnNext(ref list, false); //tbsCertificate.Validity
            Byte[] subject = AsnNext(ref list, false); //tbsCertificate.Subject        
            Byte[] subjectPublicKeyInfo = AsnNext(ref list, false); //tbsCertificate.SubjectPublicKeyInfo        

            return subjectPublicKeyInfo;
        }

        static Byte[] AsnNext(ref Byte[] buffer, Boolean unwrap)
        {
            //Public domain: No attribution required.
            Byte[] result;

            if (buffer.Length < 2)
            {
                result = buffer;
                buffer = new Byte[0];
                return result;
            }

            int index = 0;
            Byte entityType = buffer[index];
            index += 1;

            int length = buffer[index];
            index += 1;

            int lengthBytes = 1;
            if (length >= 0x80)
            {
                lengthBytes = length & 0x0F; //low nibble is number of length bytes to follow
                length = 0;

                for (int i = 0; i < lengthBytes; i++)
                {
                    length = (length << 8) + (int)buffer[2 + i];
                    index += 1;
                }
                lengthBytes++;
            }

            int copyStart;
            int copyLength;
            if (unwrap)
            {
                copyStart = 1 + lengthBytes;
                copyLength = length;
            }
            else
            {
                copyStart = 0;
                copyLength = 1 + lengthBytes + length;
            }
            result = new Byte[copyLength];
            Array.Copy(buffer, copyStart, result, 0, copyLength);

            Byte[] remaining = new Byte[buffer.Length - (copyStart + copyLength)];
            if (remaining.Length > 0)
                Array.Copy(buffer, copyStart + copyLength, remaining, 0, remaining.Length);
            buffer = remaining;

            return result;
        }

        [Obsolete]
        public static X509Certificate2 GenerateCertificate(string certName, bool isCheck)
        {
            var keypairgen = new RsaKeyPairGenerator();
            keypairgen.Init(new KeyGenerationParameters(new SecureRandom(new CryptoApiRandomGenerator()), 1024));

            var keypair = keypairgen.GenerateKeyPair();

            var gen = new X509V3CertificateGenerator();

            var CN = new X509Name("CN=" + certName);
            var SN = BigInteger.ProbablePrime(120, new Random());

            gen.SetSerialNumber(SN);
            gen.SetSubjectDN(CN);
            gen.SetIssuerDN(CN);
            gen.SetNotAfter(DateTime.MaxValue);
            gen.SetNotBefore(DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0)));
            gen.SetSignatureAlgorithm("MD5WithRSA");
            gen.SetPublicKey(keypair.Public);

            var newCert = gen.Generate(keypair.Private);

            return new X509Certificate2(DotNetUtilities.ToX509Certificate((Org.BouncyCastle.X509.X509Certificate)newCert));
        }

        public static string Encrypt(string plainText, X509Certificate2 cert)
        {
            RSACryptoServiceProvider publicKey = (RSACryptoServiceProvider)cert.PublicKey.Key;
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = publicKey.Encrypt(plainBytes, false);
            string encryptedText = Convert.ToBase64String(encryptedBytes);
            return encryptedText;
        }

        public static string Decrypt(string encryptedText, X509Certificate2 cert)
        {
            RSACryptoServiceProvider privateKey = (RSACryptoServiceProvider)cert.PrivateKey;
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] decryptedBytes = privateKey.Decrypt(encryptedBytes, false);
            string decryptedText = Encoding.UTF8.GetString(decryptedBytes);
            return decryptedText;
        }

        static byte[] Sign(string text, string certSubject)

        {

            // Access Personal (MY) certificate store of current user

            X509Store my = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            my.Open(OpenFlags.ReadOnly);


            // Find the certificate we'll use to sign

            RSACryptoServiceProvider csp = null;

            foreach (X509Certificate2 cert in my.Certificates)

            {

                if (cert.Subject.Contains(certSubject))

                {

                    // We found it.

                    // Get its associated CSP and private key

                    csp = (RSACryptoServiceProvider)cert.PrivateKey;

                }

            }

            if (csp == null)

            {

                throw new Exception("No valid cert was found");

            }


            // Hash the data

            SHA1Managed sha1 = new SHA1Managed();

            UnicodeEncoding encoding = new UnicodeEncoding();

            byte[] data = encoding.GetBytes(text);

            byte[] hash = sha1.ComputeHash(data);


            // Sign the hash

            return csp.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));

        }

        static bool Verify(string text, byte[] signature, string certPath)

        {

            // Load the certificate we'll use to verify the signature from a file

            X509Certificate2 cert = new X509Certificate2(certPath);

            // Note:

            // If we want to use the client cert in an ASP.NET app, we may use something like this instead:

            // X509Certificate2 cert = new X509Certificate2(Request.ClientCertificate.Certificate);


            // Get its associated CSP and public key

            RSACryptoServiceProvider csp = (RSACryptoServiceProvider)cert.PublicKey.Key;


            // Hash the data

            SHA1Managed sha1 = new SHA1Managed();

            UnicodeEncoding encoding = new UnicodeEncoding();

            byte[] data = encoding.GetBytes(text);

            byte[] hash = sha1.ComputeHash(data);


            // Verify the signature with the hash

            return csp.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), signature);

        }

        public static string DecryptUsingPublic(string dataEncryptedBase64, string publicKey)
        {
            if (dataEncryptedBase64 == null) throw new ArgumentNullException("dataEncryptedBase64");
            if (publicKey == null) throw new ArgumentNullException("publicKey");
            try
            {
                RSAParameters _publicKey = LoadRsaPublicKey(publicKey, false);
                RSACryptoServiceProvider rsa = InitRSAProvider(_publicKey);

                byte[] bytes = Convert.FromBase64String(dataEncryptedBase64);
                byte[] decryptedBytes = rsa.Decrypt(bytes, false);

                // I assume here that the decrypted data is intended to be a
                // human-readable string, and that it was UTF8 encoded.
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                return null;
            }
        }

        private static RSAParameters LoadRsaPublicKey(String publicKeyFilePath, Boolean isFile)
        {
            RSAParameters RSAKeyInfo = new RSAParameters();
            byte[] pubkey = null;
            //ReadFileKey(publicKeyFilePath, "PUBLIC KEY", isFile);
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];
            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            MemoryStream mem = new MemoryStream(pubkey);
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;

            try
            {

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();   //advance 2 bytes
                else
                    return RSAKeyInfo;

                seq = binr.ReadBytes(15);       //read the Sequence OID
                if (!CompareBytearrays(seq, SeqOID))    //make sure Sequence for OID is correct
                    return RSAKeyInfo;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8203)
                    binr.ReadInt16();   //advance 2 bytes
                else
                    return RSAKeyInfo;

                bt = binr.ReadByte();
                if (bt != 0x00)     //expect null byte next
                    return RSAKeyInfo;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();   //advance 2 bytes
                else
                    return RSAKeyInfo;

                twobytes = binr.ReadUInt16();
                byte lowbyte = 0x00;
                byte highbyte = 0x00;

                if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                    lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                else if (twobytes == 0x8202)
                {
                    highbyte = binr.ReadByte(); //advance 2 bytes
                    lowbyte = binr.ReadByte();
                }
                else
                    return RSAKeyInfo;
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                int modsize = BitConverter.ToInt32(modint, 0);

                byte firstbyte = binr.ReadByte();
                binr.BaseStream.Seek(-1, SeekOrigin.Current);

                if (firstbyte == 0x00)
                {   //if first byte (highest order) of modulus is zero, don't include it
                    binr.ReadByte();    //skip this null byte
                    modsize -= 1;   //reduce modulus buffer size by 1
                }

                byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                    return RSAKeyInfo;
                int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                byte[] exponent = binr.ReadBytes(expbytes);


                RSAKeyInfo.Modulus = modulus;
                RSAKeyInfo.Exponent = exponent;

                return RSAKeyInfo;
            }
            catch (Exception)
            {
                return RSAKeyInfo;
            }

            finally { binr.Close(); }
            //return RSAparams;

        }

        private static RSACryptoServiceProvider InitRSAProvider(RSAParameters rsaParam)
        {
            //
            // Initailize the CSP
            //   Supresses creation of a new key
            //
            CspParameters csp = new CspParameters();
            //csp.KeyContainerName = "RSA Test (OK to Delete)";

            const int PROV_RSA_FULL = 1;
            csp.ProviderType = PROV_RSA_FULL;

            const int AT_KEYEXCHANGE = 1;
            // const int AT_SIGNATURE = 2;
            csp.KeyNumber = AT_KEYEXCHANGE;
            //
            // Initialize the Provider
            //
            RSACryptoServiceProvider rsa =
              new RSACryptoServiceProvider(csp);
            rsa.PersistKeyInCsp = false;

            //
            // The moment of truth...
            //
            rsa.ImportParameters(rsaParam);
            return rsa;
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)     //expect integer
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();    // data size in next byte
            else
                if (bt == 0x82)
            {
                highbyte = binr.ReadByte(); // data size in next 2 bytes
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;     // we already have the data size
            }

            while (binr.ReadByte() == 0x00)
            {   //remove high order zeros in data
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);       //last ReadByte wasn't a removed zero, so back up a byte
            return count;
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        public static Boolean VerifyXml(XmlDocument Doc, System.Security.Cryptography.RSA Key)
        {
            // Check arguments.
            if (Doc == null)
                throw new ArgumentException("Doc");
            if (Key == null)
                throw new ArgumentException("Key");

            // Create a new SignedXml object and pass it
            // the XML document class.
            SignedXml signedXml = new SignedXml(Doc);

            // Find the "Signature" node and create a new
            // XmlNodeList object.
            XmlNodeList nodeList = Doc.GetElementsByTagName("Signature");

            // Throw an exception if no signature was found.
            if (nodeList.Count <= 0)
            {
                throw new CryptographicException("Verification failed: No Signature was found in the document.");
            }

            // This example only supports one signature for
            // the entire XML document.  Throw an exception 
            // if more than one signature was found.
            if (nodeList.Count >= 2)
            {
                throw new CryptographicException("Verification failed: More that one signature was found for the document.");
            }

            // Load the first <signature> node.  
            signedXml.LoadXml((XmlElement)nodeList[0]);

            // Check the signature and return the result.
            return signedXml.CheckSignature(Key);
        }

        public static void SignXml(XmlDocument xmlDoc, System.Security.Cryptography.RSA key)
        {
            // Check arguments.
            if (xmlDoc == null)
                throw new ArgumentException("xmlDoc");
            if (key == null)
                throw new ArgumentException("key");

            // Create a SignedXml object.
            SignedXml signedXml = new SignedXml(xmlDoc);

            // Add the key to the SignedXml document.
            signedXml.SigningKey = key;

            // Create a reference to be signed.
            Reference reference = new Reference();
            reference.Uri = "";

            // Add an enveloped transformation to the reference.
            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Compute the signature.
            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save
            // it to an XmlElement object.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            // Append the element to the XML document.
            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
        }

        public static string SignXmlX509Certificate2(XmlDocument Document, X509Certificate2 cert)
        {
            SignedXml signedXml = new SignedXml(Document);
            signedXml.SigningKey = cert.PrivateKey;

            // Create a reference to be signed.
            Reference reference = new Reference();
            reference.Uri = "";

            // Add an enveloped transformation to the reference.            
            XmlDsigEnvelopedSignatureTransform env =
               new XmlDsigEnvelopedSignatureTransform(true);
            reference.AddTransform(env);

            //canonicalize
            XmlDsigC14NTransform c14t = new XmlDsigC14NTransform();
            reference.AddTransform(c14t);

            KeyInfo keyInfo = new KeyInfo();
            KeyInfoX509Data keyInfoData = new KeyInfoX509Data(cert);
            KeyInfoName kin = new KeyInfoName();
            kin.Value = "Public key of certificate";
            RSACryptoServiceProvider rsaprovider = (RSACryptoServiceProvider)cert.PublicKey.Key;
            RSAKeyValue rkv = new RSAKeyValue(rsaprovider);
            keyInfo.AddClause(kin);
            keyInfo.AddClause(rkv);
            keyInfo.AddClause(keyInfoData);
            signedXml.KeyInfo = keyInfo;

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Compute the signature.
            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save 
            // it to an XmlElement object.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            Document.DocumentElement.AppendChild(
                Document.ImportNode(xmlDigitalSignature, true));

            return Document.OuterXml;
        }
        // Sign an XML file and save the signature in a new file. This method does not  
        // save the public key within the XML file.  This file cannot be verified unless  
        // the verifying code has the key with which it was signed.
        public static void SignXmlFile(XmlDocument doc, string SignedFileName, System.Security.Cryptography.RSA Key)
        {
            // Create a SignedXml object.
            SignedXml signedXml = new SignedXml(doc);

            // Add the key to the SignedXml document. 
            signedXml.SigningKey = Key;

            // Create a reference to be signed.
            Reference reference = new Reference();
            reference.Uri = "";

            // Add an enveloped transformation to the reference.
            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Compute the signature.
            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save
            // it to an XmlElement object.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            // Append the element to the XML document.
            doc.DocumentElement.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

            if (doc.FirstChild is XmlDeclaration)
            {
                doc.RemoveChild(doc.FirstChild);
            }

            // Save the signed XML document to a file specified
            // using the passed string.
            XmlTextWriter xmltw = new XmlTextWriter(SignedFileName, new UTF8Encoding(false));
            doc.WriteTo(xmltw);
            xmltw.Close();
        }
        // Verify the signature of an XML file against an asymetric 
        // algorithm and return the result.
        public static Boolean VerifyXmlFile(String Name, System.Security.Cryptography.RSA Key)
        {
            // Create a new XML document.
            XmlDocument xmlDocument = new XmlDocument();

            // Load the passed XML file into the document. 
            xmlDocument.Load(Name);

            // Create a new SignedXml object and pass it
            // the XML document class.
            SignedXml signedXml = new SignedXml(xmlDocument);

            // Find the "Signature" node and create a new
            // XmlNodeList object.
            XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Signature");

            // Load the signature node.
            signedXml.LoadXml((XmlElement)nodeList[0]);

            // Check the signature and return the result.
            return signedXml.CheckSignature(Key);
        }
        // Create example data to sign.
        public static void CreateSomeXml(string FileName)
        {
            // Create a new XmlDocument object.
            XmlDocument document = new XmlDocument();

            // Create a new XmlNode object.
            XmlNode node = document.CreateNode(XmlNodeType.Element, "", "MyElement", "samples");

            // Add some text to the node.
            node.InnerText = "Example text to be signed.";

            // Append the node to the document.
            document.AppendChild(node);

            // Save the XML document to the file name specified.
            XmlTextWriter xmltw = new XmlTextWriter(FileName, new UTF8Encoding(false));
            document.WriteTo(xmltw);
            xmltw.Close();
        }
        public static X509Certificate2 GenerateCertificate(string subject)
        {

            var random = new SecureRandom();
            var certificateGenerator = new X509V3CertificateGenerator();

            var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            certificateGenerator.SetIssuerDN(new X509Name($"C=NL, O=SomeCompany, CN={subject}"));
            certificateGenerator.SetSubjectDN(new X509Name($"C=NL, O=SomeCompany, CN={subject}"));
            certificateGenerator.SetNotBefore(DateTime.UtcNow.Date);
            certificateGenerator.SetNotAfter(DateTime.UtcNow.Date.AddYears(1));

            const int strength = 2048;
            var keyGenerationParameters = new KeyGenerationParameters(random, strength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);

            var subjectKeyPair = keyPairGenerator.GenerateKeyPair();
            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            var issuerKeyPair = subjectKeyPair;
            const string signatureAlgorithm = "SHA256WithRSA";
            var signatureFactory = new Asn1SignatureFactory(signatureAlgorithm, issuerKeyPair.Private);
            var bouncyCert = certificateGenerator.Generate(signatureFactory);

            // Lets convert it to X509Certificate2
            X509Certificate2 certificate;

            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            store.SetKeyEntry($"{subject}_key", new AsymmetricKeyEntry(subjectKeyPair.Private), new[] { new X509CertificateEntry(bouncyCert) });
            string exportpw = Guid.NewGuid().ToString("x");

            using (var ms = new System.IO.MemoryStream())
            {
                store.Save(ms, exportpw.ToCharArray(), random);
                certificate = new X509Certificate2(ms.ToArray(), exportpw, X509KeyStorageFlags.Exportable);
            }

            //Console.WriteLine($"Generated cert with thumbprint {certificate.Thumbprint}");
            return certificate;
        }
        // Verify the signature of an XML file and return the result.
        public static Boolean VerifyXmlFile(String Name)
        {
            try
            {
                // Check the arguments.  
                if (Name == null)
                    throw new ArgumentNullException("Name");

                // Create a new XML document.
                XmlDocument xmlDocument = new XmlDocument();

                // Format using white spaces.
                xmlDocument.PreserveWhitespace = true;

                // Load the passed XML file into the document. 
                xmlDocument.Load(Name);

                // Create a new SignedXml object and pass it
                // the XML document class.
                SignedXml signedXml = new SignedXml(xmlDocument);

                // Find the "Signature" node and create a new
                // XmlNodeList object.
                XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Signature");

                // Load the signature node.
                signedXml.LoadXml((XmlElement)nodeList[0]);

                // Check the signature and return the result.
                return signedXml.CheckSignature();
            }
            catch (Exception exc)
            {
                Console.Write("Error:" + exc);
                return false;
            }
        }
    }
}
