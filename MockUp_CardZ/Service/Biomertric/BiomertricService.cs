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
using System.Drawing;
using Microsoft.EntityFrameworkCore;

namespace MockUp_CardZ.Service.Biomertric
{
    public class BiomertricService : IBiomertricService
    {
        private readonly AppDbContext _context;
        //private static VideoCapture vs;
        //private static Mat outputFrame;
        private static object lockObj = new object();
        private static bool verifyFlag = false;
        private static DateTime timeoutStart;
        private static int recordId;
        private static List<double> faceData;
        public BiomertricService(AppDbContext context)
        {
            _context = context;
        }
        public async ValueTask<object> RegisterBiomertric(string imagePath, string userId, string personalInfo, string exportFilePath)
        {
            string faceData = FaceEncode(imagePath, userId).Replace(" ", "");
            //faceData = Compress(faceData);
            string publicKeyPem = string.Empty;
            var keypem = await _context.ApiEncryptionTypes
            .Where(x => x.EncryptId == "IDPE")
            .Select(x => x.ParamData)
            .FirstOrDefaultAsync();

            if (keypem == null)
            {
                return "";
            }
            JObject dataParam = JsonConvert.DeserializeObject<JObject>(keypem);
            if (dataParam["DefaultValue"] != null)
            {
                JObject values = JsonConvert.DeserializeObject<JObject>(dataParam["DefaultValue"].ToString());
                try
                {
                    publicKeyPem = values.SelectToken("PublicKey").Value<string>();
                }
                catch
                {
                    publicKeyPem = string.Empty;
                }
            }
            //mã hóa aes bằng key 
            var aesEncrypt = Encryption.AESEncrypt(personalInfo);
            //lấy chuỗi mã hóa băm bằng sha256 
            //var sha512Hash = Encryption.Sha512(aesEncrypt);
            //mã hóa chuỗi băm bằng RSA 
            string message = $"{faceData}.{userId}.{aesEncrypt}";
            string aesPassEn = string.Empty;
            using (RSACryptoServiceProvider _serverRsa = new RSACryptoServiceProvider())
            {
                _serverRsa.ImportFromPem(publicKeyPem);
                aesPassEn = Encryption.RSAEncrypt(message, _serverRsa.ExportParameters(false));
            }
            string signature = Sign(message);
            message = $"{aesPassEn}.{signature}";
            return message;
            //chuyển thành chuỗi byte 
            //thêm thông tin header và sửa lỗi 
            //thêm thành phần mặt nạ

            // Generate QR code(có thể trả về cho client tạo qr)
            //string exportFileName = $"{userId}.png";
            //QRCodeGenerator qrGenerator = new QRCodeGenerator();
            //QRCodeData qrCodeData = qrGenerator.CreateQrCode(message, QRCodeGenerator.ECCLevel.L);
            //QRCode qrCode = new QRCode(qrCodeData);
            //Bitmap qrCodeImage = qrCode.GetGraphic(20);

            //qrCodeImage = new Bitmap(qrCodeImage, new Size(820, 820));

            //string secname = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.png";
            //string exportFilePathWithName = Path.Combine(exportFilePath, secname);
            //qrCodeImage.Save(exportFilePathWithName, ImageFormat.Png);

            //return exportFilePathWithName;
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
        public async ValueTask<object> PositivelyBiomertric(string image,string dataQR)
        {
            //từ data qr kiểm tra chữ ký sô 
            //giải mã RSA => chuỗi băm 

            //khuôn mặt 
            //mã hóa aes bằng key 
            //lấy chuỗi mã hóa băm bằng sha256 

            //so sánh với chuỗi băm của QR

            vs = new VideoCapture(0); // Mở camera

            while (true)
            {
                Mat frame = new Mat();
                vs.Read(frame); // Đọc khung hình từ camera

                if (!verifyFlag)
                {
                    Mat im = new Mat();
                    Cv2.CvtColor(frame, im, ColorConversionCodes.BGR2GRAY); // Chuyển đổi khung hình sang màu xám
                    BarcodeReader reader = new BarcodeReader();
                    var result = reader.Decode(im.ToBitmap()); // Giải mã QR code

                    if (result != null)
                    {
                        // Giải mã dữ liệu QR code
                        string[] qrData = result.Text.Split('.');
                        string faceDataStr = qrData[0];
                        string publicData = qrData[1];
                        string encryptedPersonalInfo = qrData[2];
                        string signature = qrData[3];
                        string messages = $"{faceDataStr}.{publicData}.{encryptedPersonalInfo}";

                        // Kiểm tra chữ ký số
                        if (VerifySign(messages, signature))
                        {
                            string personalInfo = AESDecrypt(encryptedPersonalInfo); // Giải mã thông tin cá nhân
                            Cv2.PutText(frame, publicData, new Point(result.ResultPoints[0].X, result.ResultPoints[0].Y), HersheyFonts.HersheySimplex, 1, new Scalar(0, 0, 255), 2);

                            // Lưu ảnh xác thực QR code
                            string secname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "static", "checkin_image", $"{DateTimeOffset.Now.ToUnixTimeSeconds()}.png");
                            Cv2.ImWrite(secname, frame);
                            Console.WriteLine("QR Verified!");

                            // Lưu thông tin vào cơ sở dữ liệu (giả sử có hàm SaveCheckinToDatabase)
                            recordId = SaveCheckinToDatabase(secname, "verifying");

                            // Lưu thời gian bắt đầu xác thực và dữ liệu khuôn mặt
                            timeoutStart = DateTime.Now;
                            faceData = Uncompress(faceDataStr).Split(',').Select(double.Parse).ToList();
                            verifyFlag = true; // Đặt cờ xác thực là true
                        }
                        else
                        {
                            Console.WriteLine("SIGNATURE FALSE....");
                        }
                    }
                }
                else
                {
                    // Kiểm tra nếu thời gian timeout đã vượt quá 30 giây
                    if ((DateTime.Now - timeoutStart).TotalSeconds > 30)
                    {
                        verifyFlag = false;

                        // Cập nhật trạng thái trong cơ sở dữ liệu thành "failed" nếu xác thực thất bại do quá thời gian
                        UpdateCheckinStatus(recordId, "Unknow", "Unknow", "static/images/user.png", "failed");
                        Console.WriteLine("Verify Timeout!");
                    }

                    // Chuyển đổi khung hình từ BGR sang RGB
                    Mat rgb = new Mat();
                    Cv2.CvtColor(frame, rgb, ColorConversionCodes.BGR2RGB);

                    // Tìm các khuôn mặt trong khung hình
                    var boxes = FaceRecognition.FaceLocations(rgb);

                    // Tạo các mã hóa (encodings) cho các khuôn mặt được tìm thấy
                    var encodings = FaceRecognition.FaceEncodings(rgb, boxes);

                    // Duyệt qua từng mã hóa khuôn mặt được tìm thấy
                    foreach (var encoding in encodings)
                    {
                        // Tính khoảng cách khuôn mặt giữa mã hóa hiện tại và dữ liệu khuôn mặt lưu trữ
                        var faceDistances = FaceRecognition.FaceDistance(faceData, encoding);
                        for (int i = 0; i < faceDistances.Count; i++)
                        {
                            var distance = faceDistances[i];

                            // Nếu khoảng cách nhỏ hơn hoặc bằng 0.4, tức là khuôn mặt được xác thực thành công
                            if (distance <= 0.4)
                            {
                                // Lưu ảnh khuôn mặt xác thực thành công vào tệp tin
                                string fsecname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "static", "checkin_image", $"{DateTimeOffset.Now.ToUnixTimeSeconds()}.png");
                                Cv2.ImWrite(fsecname, frame);

                                // Cập nhật trạng thái trong cơ sở dữ liệu thành "success"
                                UpdateCheckinStatus(recordId, publicData, personalInfo, fsecname, "success");
                                verifyFlag = false;
                                Console.WriteLine("Success!");
                                break;
                            }
                            else
                            {
                                // Nếu khoảng cách lớn hơn 0.4, tức là khuôn mặt không khớp
                                Console.WriteLine("False!");
                            }
                        }
                    }
                }

                // Resize khung hình và hiển thị thời gian hiện tại
                Cv2.Resize(frame, frame, new Size(1080, 0));
                Mat gray = new Mat();
                Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.GaussianBlur(gray, gray, new Size(7, 7), 0);
                string timestamp = DateTime.Now.ToString("dddd dd MMMM yyyy hh:mm:ss tt");
                Cv2.PutText(frame, timestamp, new Point(10, frame.Rows - 10), HersheyFonts.HersheySimplex, 0.35, new Scalar(0, 0, 255), 1);

                lock (lockObj)
                {
                    outputFrame = frame.Clone(); // Lưu khung hình hiện tại
                }
            }
        }
        private static string FaceEncode(string imagePath, string name)
        {
            // Implement face encoding logic here
            // This is a placeholder for actual face encoding
            return Convert.ToBase64String(File.ReadAllBytes(imagePath));
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
    }
}
