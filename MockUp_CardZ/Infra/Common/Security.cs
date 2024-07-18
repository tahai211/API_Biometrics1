using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Infra.Common
{
    public static class Security
    {
        public static string plainText;
        public static string passPhrase = "Pas5pr@se";
        public static string saltValue = "s@1tValue";
        public static string hashAlgorithm = "MD5";
        public static int passwordIterations = 2;
        public static string initVector = "@1B2c3D4e5F6g7H8";
        public static int keySize = 256;
        public static string Encrypt(string toEncrypt, string key, bool useHashing = true)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        public static string Decrypt(string toDecrypt, string key, bool useHashing = true)
        {
            var rt = "";
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

                if (useHashing)
                {
                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                }
                else
                    keyArray = UTF8Encoding.UTF8.GetBytes(key);

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                rt = UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch
            {
                //return "";
            }
            return rt;
        }

        public static string ConvertKey(string toDecrypt, string key)
        {
            var encryptedTicket = Decrypt(toDecrypt, key + "_keyauthen");
            return encryptedTicket;
        }
        public static string EncryptKey(string EncryptKey)
        {
            using (MD5 md5hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5hash.ComputeHash(Encoding.UTF8.GetBytes(EncryptKey));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

    }

    //IDP
    public class SecuritiBodyRequest
    {
        public static (string, string) DecryptionBody(string body, string serviceId, string encryptBodyType, JObject paramData)
        {
            string publicKeyFromClient = string.Empty;
            string strBodyRequest = body;
            //EnableEncryptBodyByChannel(serviceId)
            if (true)
            {
                switch (encryptBodyType)
                {
                    case "NONE":

                        break;
                    case "IDPE":
                        string publicKeyDefault;
                        try
                        {
                            publicKeyDefault = paramData.SelectToken("PublicKey").Value<string>();
                        }
                        catch
                        {
                            publicKeyDefault = string.Empty;
                        }
                        var result = EE2E.ServerDecrypt(body.Trim('"'), paramData.SelectToken("PrivateKey").Value<string>());
                        publicKeyFromClient = string.IsNullOrEmpty(result.Item3) ? publicKeyDefault : result.Item3;
                        strBodyRequest = string.IsNullOrEmpty(result.Item1) ? null : result.Item1;
                        break;
                }
            }
            return (publicKeyFromClient, strBodyRequest);
        }
    }


    public class EE2E
    {
        /// <summary>
        /// Simple 20230518 End to End server decrypt message
        /// </summary>
        /// <param name="data">Original data send from client</param>
        /// <param name="privateKey">The private that stored on the server side</param>
        /// <returns>Item1: decrypted request, Item2: content type, Item3: Client RSA key</returns>
        public static (string, string, string) ServerDecrypt(string data, string privateKey)
        {
            int passRsaLen = Convert.ToInt32(data.Substring(0, 8), 16);
            string passRsa = data.Substring(8, passRsaLen);
            using (RSACryptoServiceProvider _serverRsa = new RSACryptoServiceProvider())
            {
                _serverRsa.ImportFromPem(privateKey);
                string passAes = Encryption.RSADecrypt(passRsa, _serverRsa.ExportParameters(true));
                string dataEn = data.Substring(8 + passRsaLen);
                string dataSum = Encryption.AESDecryptWithPassword(dataEn, passAes);

                int rsaClientLen = Convert.ToInt32(dataSum.Substring(0, 8), 16);
                int contentTypeLen = Convert.ToInt32(dataSum.Substring(8, 2), 16);

                string rsaClient = dataSum.Substring(10, rsaClientLen);
                string contentType = dataSum.Substring(10 + rsaClientLen, contentTypeLen);
                string content = dataSum.Substring(10 + rsaClientLen + contentTypeLen);

                return (content, contentType, rsaClient);
            }
        }
        /// <summary>
        /// Simple 20230518 End to End server encrypt message
        /// </summary>
        /// <param name="sinput">the response message will be transfer to client</param>
        /// <param name="contentType">http content type</param>
        /// <param name="publicKey">the public key that received from client</param>
        /// <returns></returns>
        public static string ServerEncrypt(string sinput, string contentType, string publicKey)
        {
            string aesPass = Guid.NewGuid().ToString();
            string dataSum = contentType + sinput;
            string enDataSum = Encryption.AESEncryptWithPassword(dataSum, aesPass);
            using (RSACryptoServiceProvider _serverRsa = new RSACryptoServiceProvider())
            {
                _serverRsa.ImportFromPem(publicKey);
                string aesPassEn = Encryption.RSAEncrypt(aesPass, _serverRsa.ExportParameters(false));

                string dataEnSum = $"{aesPassEn.Length.ToString("x8")}{contentType.Length.ToString("x2")}{aesPassEn}{enDataSum}";
                return dataEnSum;
            }
        }
    }

    public static class Encryption
    {
        #region RSA
        public static string RSAEncrypt(string Data, RSAParameters RSAKey, bool DoOAEPPadding = false)
        {
            var arrData = Encoding.UTF8.GetBytes(Data);
            var arrEnData = RSAEncrypt(arrData, RSAKey);
            return Convert.ToBase64String(arrEnData);
        }
        public static byte[] RSAEncrypt(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding = false)
        {
            try
            {
                byte[] encryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAKey);
                    encryptedData = RSA.Encrypt(Data, DoOAEPPadding);
                }
                return encryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }
        public static string RSADecrypt(string Data, RSAParameters RSAKey, bool DoOAEPPadding = false)
        {
            var arrEnData = Convert.FromBase64String(Data);
            var arrDeData = RSADecrypt(arrEnData, RSAKey);
            return Encoding.UTF8.GetString(arrDeData);
        }
        public static byte[] RSADecrypt(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding = false)
        {
            try
            {
                byte[] decryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAKey);
                    decryptedData = RSA.Decrypt(Data, DoOAEPPadding);
                }
                return decryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());
                throw e;
            }
        }
        #endregion
        #region AES
        static string salt = "ZGgIddJOLDv7l1fH7BCKplG2j4iAnAQ3";
        static readonly byte[] saltAES = Encoding.ASCII.GetBytes("yO3NV1mhssvqWNMbG3atWie5V6PcLlTc");
        static string defaultAESPass = "bNDPWqKCrTFjgyOiDJ47v7Babl4vhpE3oGBrUxqog625mb8vX7p3oOE6G7JLHpMt";
        public static string GenaratePassword(int len, bool onlyDigit = true)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, len)
                .Select(s => s[(new Random()).Next(s.Length)]).ToArray());
        }
        public static string EncryptPasswordPlantext(string loginId, string passwordPlantext)
        {
            return EncryptPassword(loginId, Sha256(passwordPlantext));
        }
        public static string EncryptPassword(string loginId, string password)
        {
            if (password.Length != 64)
                throw new Exception("Password must be encrypted as SHA256");
            string mixSatl = string.Concat(salt.Zip(password.Substring(20, 32), (a, b) => new[] { a, b }).SelectMany(c => c));
            return Sha256((loginId + mixSatl + password).ToLower());
        }
        public static string AESEncrypt(string input)
        {
            return AESEncryptWithPassword(input, defaultAESPass);
        }
        public static string AESEncryptWithPassword(string input, string password)
        {
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.

            using (MemoryStream ms = new MemoryStream())
            {
                using (Aes AES = Aes.Create())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltAES, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return Convert.ToBase64String(encryptedBytes);
        }
        public static string AESDecrypt(string input)
        {
            return AESDecryptWithPassword(input, defaultAESPass);
        }
        public static string AESDecryptWithPassword(string input, string password)
        {
            byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.

            using (MemoryStream ms = new MemoryStream())
            {
                using (Aes AES = Aes.Create())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltAES, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        #endregion

        #region SHA
        public static string Sha256(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        #endregion

        #region smart otp
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string GetSmartOTPSecret(string deviceId, string secretKey)
        {
            for (int i = 0; i < deviceId.Length; i++)
            {
                int pos = Convert.ToInt32(i * 3.14);
                while (pos > 256)
                {
                    pos = pos - 256;
                }
                secretKey = secretKey.Insert(pos, deviceId[i].ToString());
            }
            return Sha256(secretKey);
        }
        #endregion
    }

    public static class SysConfig
    {
        public static string ServerPrivateKey = "-----BEGIN RSA PRIVATE KEY-----\r\nMIICXAIBAAKBgQDD4wsRYFjwwy7hJ7eTiMJ88sxB6HR3KNV9D0+TuetWAyr1fNYo\r\nYYMPBDzleHteJQBCI+hDkizf4ls8gucvDO2Yl/FJnUJ7TG+G5ZtynhgxKkSDMjn5\r\nU3E5RNVfqlkmJB2joFeiHusKccMJl+SNZwIpL+zwzmgcyrKViO6bA4V9xQIDAQAB\r\nAoGAQYPLLNmbhZLSOA3PkTF5Us7lz9lOMphepsAbLl5NPmw6zvYcrk7GxZv2Pg1I\r\nvZkEeBqIUSkvhwWu3BMv/QWG5R9VEdra0G+t9+pUTkftfoiWoDoBfLZ1392sIY+3\r\nY6faWnbSFrAuGsjR++Wa5TgFW+Z2Uot/sHL3J04SEtJjmVUCQQDr6xhnmtIfcUdU\r\nfVTvL2SYix7kNBYzUxc4ctfH5xB3wiihkV9i5SC4StVgeIxltf4lJfcmrY+ZSGip\r\nD4inECDDAkEA1I+fAhgqLiUKSzbMi0FA0x12eHIfiCCU9IweSBGA7wjkV08sA9SL\r\ncxSp/vpapSn4zK/ojAjB1gFHnhy0eh5+1wJBAOKEBxmrCGdSL6fK8tr5CUCCC3YX\r\nIN3EwP3dHpv8ms4x6StAVnKxBxqMF5YaCMu70zo92rETYlZNtNfmZ1Ho5VsCQA0h\r\nEeE/PNu8zfX3XIr9bNOdbkPiSLBP0uOIBBbPffdFxVTfPwi9iB+Dlzx4mkC5ZvS9\r\nqM99evuM+K97dmpUl+ECQHozKD0YCR6UEpKwWhbI+0B31c1Wju3JtoPjL0l/akbG\r\ncALjSq3TbJScrO5G1bkeBVSbPHxY7clLC9Lkqij4okM=\r\n-----END RSA PRIVATE KEY-----";
        public static string ServerPublicKey = "-----BEGIN PUBLIC KEY-----\r\nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDD4wsRYFjwwy7hJ7eTiMJ88sxB\r\n6HR3KNV9D0+TuetWAyr1fNYoYYMPBDzleHteJQBCI+hDkizf4ls8gucvDO2Yl/FJ\r\nnUJ7TG+G5ZtynhgxKkSDMjn5U3E5RNVfqlkmJB2joFeiHusKccMJl+SNZwIpL+zw\r\nzmgcyrKViO6bA4V9xQIDAQAB\r\n-----END PUBLIC KEY-----";

        public static RSACryptoServiceProvider RSAClient = new RSACryptoServiceProvider();
        public static int PrefixLenght = 4;
    }
}
