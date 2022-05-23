using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using Chilkat;
using System.Configuration;
using System.Security.Cryptography;

namespace Web.Custom
{
    public class RSAEncryptAndDecrypt
    {
        public string EncryptandDecryptStrings(string plainText)
        {
            Chilkat.Rsa rsa = new Chilkat.Rsa();

            bool success = rsa.UnlockComponent("Anything for 30-day trial");

            if (success != true)
            {
                //Console.WriteLine("RSA component unlock failed");
                return string.Empty;
            }

            //  This example also generates the public and private
            //  keys to be used in the RSA encryption.
            //  Normally, you would generate a key pair once,
            //  and distribute the public key to your partner.
            //  Anything encrypted with the public key can be
            //  decrypted with the private key.  The reverse is
            //  also true: anything encrypted using the private
            //  key can be decrypted using the public key.

            //  Generate a 1024-bit key.  Chilkat RSA supports
            //  key sizes ranging from 512 bits to 4096 bits.
            success = rsa.GenerateKey(1024);
            if (success != true)
            {
                //Console.WriteLine(rsa.LastErrorText);
                return string.Empty;
            }

            //  Keys are exported in XML format:
            string publicKey = rsa.ExportPublicKey();
            string privateKey = rsa.ExportPrivateKey();             

            //  Start with a new RSA object to demonstrate that all we
            //  need are the keys previously exported:
            Chilkat.Rsa rsaEncryptor = new Chilkat.Rsa();

            //  Encrypted output is always binary.  In this case, we want
            //  to encode the encrypted bytes in a printable string.
            //  Our choices are "hex", "base64", "url", "quoted-printable".
            rsaEncryptor.EncodingMode = "hex";

            //  We'll encrypt with the public key and decrypt with the private
            //  key.  It's also possible to do the reverse.
            success = rsaEncryptor.ImportPublicKey(publicKey);

            bool usePrivateKey = false;
            string encryptedStr = rsaEncryptor.EncryptStringENC(plainText, usePrivateKey);

            //Console.WriteLine(encryptedStr);

            //  Now decrypt:
            Chilkat.Rsa rsaDecryptor = new Chilkat.Rsa();

            rsaDecryptor.EncodingMode = "hex";
            success = rsaDecryptor.ImportPrivateKey(privateKey);

            usePrivateKey = true;
            string decryptedStr = rsaDecryptor.DecryptStringENC(encryptedStr, usePrivateKey);

            //Console.WriteLine(decryptedStr);

            return decryptedStr;
        }

        public string Base64Encode(string inputString, ref string outString)
        {
            Chilkat.Crypt2 crypt = new Chilkat.Crypt2();

            //  Any string argument automatically begins the 30-day trial.
            bool success;
            success = crypt.UnlockComponent(ConfigurationManager.AppSettings["CYPT"]);
            if (success != true)
            {
                return crypt.LastErrorText;
            }

            //  Indicate that no encryption should be performed,
            //  only encoding/decoding.
            crypt.CryptAlgorithm = "none";
            crypt.EncodingMode = "base64";

            //  Other possible EncodingMode settings are:
            //  "quoted-printable", "hex", "uu", "base32", and "url"
            outString = crypt.EncryptStringENC(inputString);

            return "";
        }

        public string Base64Decode(string inputString, ref string outString)
        {
            Chilkat.Crypt2 crypt = new Chilkat.Crypt2();

            //  Any string argument automatically begins the 30-day trial.
            bool success;
            success = crypt.UnlockComponent(ConfigurationManager.AppSettings["CYPT"]);
            if (success != true)
            {
                return crypt.LastErrorText;
            }

            //  Indicate that no encryption should be performed,
            //  only encoding/decoding.
            crypt.CryptAlgorithm = "none";
            crypt.EncodingMode = "base64";

            //  Other possible EncodingMode settings are:
            //  "quoted-printable", "hex", "uu", "base32", and "url"
            outString = crypt.DecryptStringENC(inputString);

            return "";
        }

        public string AESStringEncryption(string passPhrase, string inputString, ref string outString)
        {
            Chilkat.Crypt2 crypt = new Chilkat.Crypt2();

            bool success;
            success = crypt.UnlockComponent(ConfigurationManager.AppSettings["CYPT"]);
            if (success != true)
            {
                return crypt.LastErrorText;
            }

            crypt.CryptAlgorithm = "aes";
            crypt.CipherMode = "cbc";
            crypt.KeyLength = 128;

            //  Generate a binary secret key from a password string
            //  of any length.  For 128-bit encryption, GenEncodedSecretKey
            //  generates the MD5 hash of the password and returns it
            //  in the encoded form requested.  The 2nd param can be
            //  "hex", "base64", "url", "quoted-printable", etc.
            string hexKey;
            hexKey = crypt.GenEncodedSecretKey(passPhrase, "hex");
            crypt.SetEncodedKey(hexKey, "hex");

            crypt.EncodingMode = "base64";

            //  Encrypt a string and return the binary encrypted data
            //  in a base-64 encoded string.
            outString = crypt.EncryptStringENC(inputString);

            return "";
        }

        public string AESStringDecryption(string passPhrase, string inputString, ref string outString)
        {
            Chilkat.Crypt2 crypt = new Chilkat.Crypt2();

            bool success;
            success = crypt.UnlockComponent(ConfigurationManager.AppSettings["CYPT"]);
            if (success != true)
            {
                return crypt.LastErrorText;
            }

            crypt.CryptAlgorithm = "aes";
            crypt.CipherMode = "cbc";
            crypt.KeyLength = 128;

            //  Generate a binary secret key from a password string
            //  of any length.  For 128-bit encryption, GenEncodedSecretKey
            //  generates the MD5 hash of the password and returns it
            //  in the encoded form requested.  The 2nd param can be
            //  "hex", "base64", "url", "quoted-printable", etc.
            string hexKey;
            hexKey = crypt.GenEncodedSecretKey(passPhrase, "hex");
            crypt.SetEncodedKey(hexKey, "hex");

            crypt.EncodingMode = "base64";

            //  Encrypt a string and return the binary encrypted data
            //  in a base-64 encoded string.
            outString = crypt.DecryptStringENC(inputString);

            return "";
        }

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

        public static string Encryption(string strText)
        {
            var publicKey = "<RSAKeyValue><Modulus>21wEnTU+mcD2w0Lfo1Gv4rtcSWsQJQTNa6gio05AOkV/Er9w3Y13Ddo5wGtjJ19402S71HUeN0vbKILLJdRSES5MHSdJPSVrOqdrll/vLXxDxWs/U0UT1c8u6k/Ogx9hTtZxYwoeYqdhDblof3E75d9n2F0Zvf6iTb4cI7j6fMs=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

            var testData = Encoding.UTF8.GetBytes(strText);

            using (var rsa = new RSACryptoServiceProvider(1024))
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

        public static string Decryption(string strText)
        {
            var privateKey = "<RSAKeyValue><Modulus>21wEnTU+mcD2w0Lfo1Gv4rtcSWsQJQTNa6gio05AOkV/Er9w3Y13Ddo5wGtjJ19402S71HUeN0vbKILLJdRSES5MHSdJPSVrOqdrll/vLXxDxWs/U0UT1c8u6k/Ogx9hTtZxYwoeYqdhDblof3E75d9n2F0Zvf6iTb4cI7j6fMs=</Modulus><Exponent>AQAB</Exponent><P>/aULPE6jd5IkwtWXmReyMUhmI/nfwfkQSyl7tsg2PKdpcxk4mpPZUdEQhHQLvE84w2DhTyYkPHCtq/mMKE3MHw==</P><Q>3WV46X9Arg2l9cxb67KVlNVXyCqc/w+LWt/tbhLJvV2xCF/0rWKPsBJ9MC6cquaqNPxWWEav8RAVbmmGrJt51Q==</Q><DP>8TuZFgBMpBoQcGUoS2goB4st6aVq1FcG0hVgHhUI0GMAfYFNPmbDV3cY2IBt8Oj/uYJYhyhlaj5YTqmGTYbATQ==</DP><DQ>FIoVbZQgrAUYIHWVEYi/187zFd7eMct/Yi7kGBImJStMATrluDAspGkStCWe4zwDDmdam1XzfKnBUzz3AYxrAQ==</DQ><InverseQ>QPU3Tmt8nznSgYZ+5jUo9E0SfjiTu435ihANiHqqjasaUNvOHKumqzuBZ8NRtkUhS6dsOEb8A2ODvy7KswUxyA==</InverseQ><D>cgoRoAUpSVfHMdYXW9nA3dfX75dIamZnwPtFHq80ttagbIe4ToYYCcyUz5NElhiNQSESgS5uCgNWqWXt5PnPu4XmCXx6utco1UVH8HGLahzbAnSy6Cj3iUIQ7Gj+9gQ7PkC434HTtHazmxVgIR5l56ZjoQ8yGNCPZnsdYEmhJWk=</D></RSAKeyValue>";

            var testData = Encoding.UTF8.GetBytes(strText);

            using (var rsa = new RSACryptoServiceProvider(1024))
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

        private static bool _optimalAsymmetricEncryptionPadding = false;

        public EncryptorRSAKeys GenerateKeys(int keySize)
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

        public string EncryptText(string text, string publicKey)
        {
            int keySize = 0;
            string publicKeyXml = "";

            GetKeyFromEncryptionString(publicKey, out keySize, out publicKeyXml);

            var encrypted = Encrypt(Encoding.UTF8.GetBytes(text), keySize, publicKeyXml);
            return Convert.ToBase64String(encrypted);
        }

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

        public string DecryptText(string text, string privateKey)
        {
            int keySize = 0;
            string publicAndPrivateKeyXml = "";

            GetKeyFromEncryptionString(privateKey, out keySize, out publicAndPrivateKeyXml);

            var decrypted = Decrypt(Convert.FromBase64String(text), keySize, publicAndPrivateKeyXml);
            return Encoding.UTF8.GetString(decrypted);
        }

        private byte[] Decrypt(byte[] data, int keySize, string publicAndPrivateKeyXml)
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

        public int GetMaxDataLength(int keySize)
        {
            if (_optimalAsymmetricEncryptionPadding)
            {
                return ((keySize - 384) / 8) + 7;
            }
            return ((keySize - 384) / 8) + 37;
        }

        public bool IsKeySizeValid(int keySize)
        {
            return keySize >= 384 &&
                    keySize <= 16384 &&
                    keySize % 8 == 0;
        }

        private string IncludeKeyInEncryptionString(string publicKey, int keySize)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(keySize.ToString() + "!" + publicKey));
        }

        private void GetKeyFromEncryptionString(string rawkey, out int keySize, out string xmlKey)
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
                    catch (Exception e) { }
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
}