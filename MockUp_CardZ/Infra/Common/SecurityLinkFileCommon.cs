using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Infra.Common
{
    public class SecurityLinkFileCommon
    {
        public static object Execute(string plaintext)
        {
            return DecryptURL(plaintext);//GetLimitList();
        }
        ///
        /// plaintext =  id document  + token (client)
        public static string EncryptURL(string plaintext)
        {
            string password = "ABC@123*";
            return CompressString(plaintext, password);
        }

        public static string DecryptURL(string cipherText)
        {
            string password = "ABC@123*";
            return DecompressString(cipherText, password);
        }
        public static string CompressString(string text, string pasword)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            gZipBuffer = encrypt(gZipBuffer, pasword);
            return Base64UrlEncode(gZipBuffer);
        }

        static byte[] Base64UrlDecode(string arg)
        {
            string s = arg;
            s = s.Replace('-', '+').Replace('_', '/');  // 62nd char of encoding // 63rd char of encoding
            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: s += "=="; break; // Two pad chars
                case 3: s += "="; break; // One pad char
                default:
                    throw new System.Exception(
             "Illegal base64url string!");
            }
            return Convert.FromBase64String(s); // Standard base64 decoder
        }

        static private byte[] Decrypt(byte[] data, string password)
        {
            byte[] Keys = Encoding.ASCII.GetBytes(password);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(Keys[i % Keys.Length] ^ data[i]);
            }
            return data;
        }

        static string DecompressString(string compressedText, string pasword)
        {
            byte[] gZipBuffer = Base64UrlDecode(compressedText);
            gZipBuffer = Decrypt(gZipBuffer, pasword);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }

        static private byte[] encrypt(byte[] data, string password)
        {
            byte[] Keys = Encoding.ASCII.GetBytes(password);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ Keys[i % Keys.Length]);
            }
            return data;
        }

        static string Base64UrlEncode(byte[] arg)
        {
            string s = Convert.ToBase64String(arg); // Regular base64 encoder
            s = s.Split('=')[0].Replace('+', '-').Replace('/', '_');// 62nd char of encoding // 63rd char of encoding
            return s;
        }
    }
}
