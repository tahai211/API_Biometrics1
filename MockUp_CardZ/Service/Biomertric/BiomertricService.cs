using MockUp_CardZ.Data;
using MockUp_CardZ.Infra.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Numerics;

namespace MockUp_CardZ.Service.Biomertric
{
    public class BiomertricService : IBiomertricService
    {
        private readonly AppDbContext _context;
        public BiomertricService(AppDbContext context)
        {
            _context = context;
        }
        public async ValueTask<object> RegisterBiomertric(string image)
        {
            string publicKeyPem = "";

            //mã hóa aes bằng key 
            var aesEncrypt = Encryption.AESEncrypt(image);
            //lấy chuỗi mã hóa băm bằng sha256 
            var sha512Hash = Encryption.Sha512(aesEncrypt);
            //mã hóa chuỗi băm bằng RSA 
            string aesPassEn = string.Empty;
            using (RSACryptoServiceProvider _serverRsa = new RSACryptoServiceProvider())
            {
                _serverRsa.ImportFromPem(publicKeyPem);
                aesPassEn = Encryption.RSAEncrypt(sha512Hash, _serverRsa.ExportParameters(false));
            }
            //ký 
            var sing = Sign(aesPassEn);
            //chuyển thành chuỗi byte 
            //thêm thông tin header và sửa lỗi 
            //thêm thành phần mặt nạ 
            //tạo QR
            return new {
                status =  "success",
                  matched = true,
                  confidence = 92.5,
                  userId = "67890"
                };
        }
        //var keypem = await digitalContext.ApiEncryptionTypes
        //    .Where(x => x.EncryptId == "IDPE")
        //    .Select(x => x.ParamData)
        //    .FirstOrDefaultAsync();

        //if (keypem == null)
        //{
        //    return "";
        //}
        //JObject dataParam = JsonConvert.DeserializeObject<JObject>(keypem);
        //if (dataParam["DefaultValue"] != null)
        //{
        //    JObject values = JsonConvert.DeserializeObject<JObject>(dataParam["DefaultValue"].ToString());
        //var result = DecryptionBody(pass, values);
        //    return result.Item2;
        //}
        //else
        //{
        //    return "";
        //}
        public static RSAParameters LoadRsaPublicKey(string publicKeyPem)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportFromPem(publicKeyPem);
                return rsa.ExportParameters(false); // false = public key
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">thông điệp cần xác thực</param>
        /// <param name="signatureHex">chuỗi chữ ký số</param>
        /// <returns></returns>
        public static bool VerifySign(string message, string signatureHex)
        {
            try
            {
                // Xác định đường dẫn đến tệp chứa thông tin bí mật
                string secretsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "creator", "static", "secrets.dat");
                Console.WriteLine("GET PATH Secrets.dat: " + secretsFilePath);

                // Đọc nội dung của tệp secrets.dat
                string data = File.ReadAllText(secretsFilePath);
                dynamic jsonData = JsonConvert.DeserializeObject(data);

                // Lấy các giá trị e và n từ tệp bí mật
                BigInteger e = BigInteger.Parse((string)jsonData.e);
                BigInteger n = BigInteger.Parse((string)jsonData.n);

                // Tính giá trị hash của thông điệp
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                byte[] hashBytes;
                using (SHA512 sha512 = SHA512.Create())
                {
                    hashBytes = sha512.ComputeHash(messageBytes);
                }
                BigInteger hash = new BigInteger(hashBytes.Reverse().ToArray());

                // Chuyển đổi chữ ký từ chuỗi hex sang BigInteger
                BigInteger signature = BigInteger.Parse(signatureHex, System.Globalization.NumberStyles.HexNumber);

                // Giải mã chữ ký và so sánh với hash của thông điệp
                BigInteger hashFromSignature = BigInteger.ModPow(signature, e, n);

                return hash.Equals(hashFromSignature);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
        public static string Sign(string message)
        {
            try
            {
                // Xác định đường dẫn đến tệp chứa thông tin bí mật
                string secretsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "static", "secrets.dat");
                Console.WriteLine("GET PATH Secrets.dat: " + secretsFilePath);

                // Đọc nội dung của tệp secrets.dat
                string data = File.ReadAllText(secretsFilePath);
                dynamic jsonData = JsonConvert.DeserializeObject(data);

                // Lấy các giá trị d và n từ tệp bí mật
                BigInteger d = BigInteger.Parse((string)jsonData.d);
                BigInteger n = BigInteger.Parse((string)jsonData.n);

                // Tính giá trị hash của thông điệp
                byte[] hashBytes;
                using (SHA512 sha512 = SHA512.Create())
                {
                    hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(message));
                }
                BigInteger hash = new BigInteger(hashBytes.Reverse().ToArray());

                // Tạo chữ ký bằng cách sử dụng thuật toán RSA
                byte[] signatureBytes;
                using (RSA rsa = RSA.Create())
                {
                    rsa.ImportParameters(new RSAParameters
                    {
                        D = d.ToByteArray(),
                        Modulus = n.ToByteArray(),
                        Exponent = new byte[] { 1, 0, 1 } // Exponent 65537, nếu cần
                    });

                    signatureBytes = rsa.SignHash(hashBytes, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                }

                // Chuyển đổi chữ ký thành chuỗi hex
                return BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private static byte[] BigIntegerToBytes(BigInteger bigInt)
        {
            return bigInt.ToByteArray();
        }
        public async ValueTask<object> PositivelyBiomertric(string image,string dataQR)
        {
            //từ data qr kiểm tra chữ ký sô 
            //giải mã RSA => chuỗi băm 

            //khuôn mặt 
            //mã hóa aes bằng key 
            //lấy chuỗi mã hóa băm bằng sha256 

            //so sánh với chuỗi băm của QR
            return new
            {
                status = "success",
                matched = true,
                confidence = 92.5,
                userId = "67890"
            };
        }
    }
}
