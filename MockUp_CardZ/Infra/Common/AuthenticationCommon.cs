using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Infra.Common
{
    public class AuthenticationCommon
    {
        //Simple: call Execute on the function editor to test
        //public static async ValueTask<object> Execute()
        //{
        //    //await IdpCaching.GetObjectAsync("Sys_Variables", "SMSOTPLIFETIME");
        //    return await IdpCaching.GetObjectAsync("Sys_Variables", "SMSOTPLIFETIME");
        //}
        //public static async Task AuthenTran(string AuthenType, string AuthenCode, ActivityExecutionContext context)
        //{
        //    string userId = context.GetVariable(VariableKey.UserId)?.ToString();
        //    string deviceId = context.GetVariable(VariableKey.DeviceId)?.ToString();

        //    AuthenticationResult authenticationResult;
        //    string isauthen = (await IdpCaching.GetObjectAsync("Sys_Variables", "ALLOW_AUTHENTRAN")).Value;


        //    if (isauthen != null && isauthen.ToLower() == "false")
        //    {
        //        Console.WriteLine("Authenticated transaction");
        //    }
        //    else
        //    {
        //        //HaiTx add thêm case EMAILOTP
        //        switch (AuthenType)
        //        {
        //            case Constant.AUTHENTYPE.SMSOTP:
        //                authenticationResult = await new SMSOTP().Authenticate(userId, AuthenCode);
        //                break;
        //            case Constant.AUTHENTYPE.EMAILOTP:
        //                authenticationResult = await new EMAIL().Authenticate(userId, AuthenCode);
        //                break;
        //            case Constant.AUTHENTYPE.SMARTOTP:
        //                authenticationResult = await new SmartOTP().Authenticate(userId, AuthenCode, deviceId);
        //                break;
        //            case Constant.AUTHENTYPE.BIOMETRIC:
        //                string serviceId = context.GetVariable(VariableKey.ChannelId)?.ToString();
        //                authenticationResult = await new BioMetricToken().Authenticate(userId, serviceId, deviceId, AuthenCode, context);
        //                break;
        //            default:
        //                throw new IdpException("unsupported_authen", $"Authen type {AuthenType} was not found");
        //        }
        //        if (authenticationResult.AuthenResult)
        //        {
        //            Console.WriteLine("Authenticated transaction");
        //        }
        //        else
        //        {
        //            //haiTX set Error return is fail
        //            context.SetVariable(VariableKey.ErrorCode, authenticationResult.AuthenErrorCode);
        //            context.SetVariable(VariableKey.ErrorDesc, authenticationResult.AuthenErrorDesc);
        //            context.SetVariable(VariableKey.ErrorDetail, authenticationResult.AuthenErrorDesc);
        //            throw new IdpException(authenticationResult.AuthenErrorCode, authenticationResult.AuthenErrorDesc);
        //        }
        //    }

        //}
        ////Fiona - 20221201 - Check authen for users have not login yet (userid not in context)
        //public static async Task AuthenTranForUserNotLogin(string AuthenType, string AuthenCode, string UserId, string DeviceId, ActivityExecutionContext context)
        //{
        //    string userId = (string)context.GetVariable(VariableKey.UserId);
        //    context.SetVariable(VariableKey.UserId, UserId);
        //    context.SetVariable(VariableKey.DeviceId, DeviceId);
        //    await AuthenTran(AuthenType, AuthenCode, context);
        //    if (!string.IsNullOrEmpty(userId))
        //        context.SetVariable(VariableKey.UserId, userId);
        //}
        ////Fiona - 20230824 - encode biometric token
        //public static async ValueTask<string> EncodeBioMetricToken(string deviceId, string biometricToken)
        //{
        //    deviceId = deviceId.Trim().ToUpper();
        //    biometricToken = biometricToken.Trim().ToUpper();
        //    return Utility.Encryption.EncryptPasswordPlantext("", string.Format("I{0}T{1}S", deviceId.Substring(0, 5), biometricToken)).Substring(0, 36).ToUpper();
        //}
        //public class AuthenticationResult
        //{
        //    public string AuthenType { get; set; }
        //    public bool AuthenResult { get; set; }
        //    public string AuthenErrorCode { get; set; }
        //    public string AuthenErrorDesc { get; set; }
        //}
        //public class SMSOTPResult
        //{
        //    public string OTPCode { get; set; }
        //    public string PhoneNo { get; set; }
        //    public string ExpiredTime { get; set; }
        //    public bool Result { get; set; }
        //}
        //public class SMSOTPRecord
        //{
        //    public string Code { get; set; }
        //    public int Retry { get; set; } = 0;
        //    public DateTime ExpiredDate { get; set; }
        //    public int ExpiredLifeTime { get; set; } = 0;

        //}

        //public class BioMetricToken
        //{
        //    public async ValueTask<AuthenticationResult> Authenticate(string userId, string serviceId, string deviceId, string biometricToken, ActivityExecutionContext context)
        //    {
        //        try
        //        {
        //            var rs = new AuthenticationResult
        //            {
        //                AuthenType = Constant.AUTHENTYPE.BIOMETRIC,
        //                AuthenResult = false,
        //                AuthenErrorCode = "invalid_biometrictoken",
        //                AuthenErrorDesc = "Invalid Biometric Token",
        //            };
        //            string loginId = (await IdpCaching.GetObjectAsync("Idp_UserLogin", userId)).LoginId;
        //            string tokenLoginInfo = (await IdpCaching.GetObjectAsync("Idp_LoginInfo", loginId, serviceId)).BiometricToken;
        //            string token = await EncodeBioMetricToken(deviceId, biometricToken);

        //            if (tokenLoginInfo.Equals(token))
        //            {
        //                rs.AuthenResult = true;
        //                rs.AuthenErrorCode = "success";
        //                rs.AuthenErrorDesc = "OK";
        //                return rs;

        //            }
        //            else
        //            {
        //                dynamic interServiceInfo = IdpCaching.GetObjectByString("SYS", SysCacheTableName.Idp_InterService);
        //                string protocol = interServiceInfo.Protocol;
        //                string host = interServiceInfo.Host;
        //                var bodyObject = new
        //                {
        //                    IdpHeader = new
        //                    {
        //                        ChannelId = context.GetVariable(VariableKey.ChannelId),
        //                        UserId = context.GetVariable(VariableKey.UserId),
        //                        ClientInfo = context.GetVariable(VariableKey.ClientInfo),
        //                        Lang = context.GetVariable(VariableKey.Lang),
        //                    },
        //                    sourceId = context.WorkflowInstance.Id,
        //                    param = new
        //                    {
        //                        loginId = loginId,
        //                        serviceId = serviceId
        //                    },
        //                    name = "SysClearBiometric"

        //                };

        //                string requestBody = JObject.FromObject(bodyObject).ToString();
        //                HttpClient _httpClient = new HttpClient();
        //                _httpClient.BaseAddress = new Uri(host);
        //                var request = new HttpRequestMessage(HttpMethod.Post, "/query");
        //                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //                request.Content = new StringContent(requestBody, Encoding.UTF8);
        //                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        //                request.Content.Headers.Add("X-Correlation-Id", context.CorrelationId);
        //                var response = await _httpClient.SendAsync(request);

        //                //clear token in logininfo
        //                // JObject joApprovalInput = new JObject(
        //                //           new JProperty("loginId", loginId),
        //                //           new JProperty("serviceId", serviceId)
        //                //           );
        //                // await FunctionExtensions.ExecFunction("SysClearBiometric", joApprovalInput, FunctionType.Function, context);
        //            }
        //            return rs;
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error(ex.ToString());
        //            throw new IdpException("unable_verifybiometric", ex.ToString());
        //        }
        //    }
        //}
        //public class SMSOTP
        //{
        //    public async ValueTask<SMSOTPRecord> GenarateOTP(string userId)
        //    {
        //        int otpLen = 6;
        //        int.TryParse((await IdpCaching.GetObjectAsync("Sys_Variables", "SMSOTPLEN")).Value, out otpLen);

        //        int otpLifeTime = 3;
        //        int.TryParse((await IdpCaching.GetObjectAsync("Sys_Variables", "SMSOTPLIFETIME")).Value, out otpLifeTime);

        //        Random ran = new Random();
        //        byte[] auCode = new byte[otpLen + 1];
        //        string strAuCode = string.Empty;
        //        for (int i = 0; i < otpLen; i++)
        //        {
        //            auCode[i] = (byte)ran.Next(10);
        //            strAuCode = strAuCode + auCode[i].ToString();
        //        }
        //        string cacheKey = $"SO_{userId}";
        //        var otpDetail = new SMSOTPRecord
        //        {
        //            Code = Utility.Encryption.EncryptPasswordPlantext(userId, strAuCode),
        //            Retry = 0,
        //            ExpiredDate = DateTime.Now.AddMinutes(otpLifeTime),
        //            ExpiredLifeTime = otpLifeTime
        //        };
        //        await IdpCaching.SetStringAsync(cacheKey, JsonConvert.SerializeObject(otpDetail).ToString());
        //        otpDetail.Code = strAuCode;
        //        return otpDetail;
        //    }

        //    public async ValueTask<AuthenticationResult> Authenticate(string userId, string authenCode)
        //    {
        //        try
        //        {
        //            int maxRetry = 5;
        //            int.TryParse((await IdpCaching.GetObjectAsync("Sys_Variables", "SMSOTPMAXRETRY")).Value, out maxRetry);
        //            var rs = new AuthenticationResult
        //            {
        //                AuthenType = Constant.AUTHENTYPE.SMSOTP,
        //                AuthenResult = false,
        //                AuthenErrorCode = "invalid_otp",
        //                AuthenErrorDesc = "Invalid OTP Code",
        //            };
        //            //Fiona - check with static OTP in advance
        //            var otpStatic = await IdpCaching.GetObjectAsync("Ctm_OtpWhiteList", userId);
        //            if (otpStatic != null && ((string)otpStatic.Code).Equals(authenCode))
        //            {
        //                rs.AuthenResult = true;
        //                rs.AuthenErrorCode = "success";
        //                rs.AuthenErrorDesc = "OK";
        //                return rs;
        //            }

        //            string cacheKey = $"SO_{userId}";
        //            var otprs = await IdpCaching.GetObjectAsync(cacheKey);
        //            if (otprs != null)
        //            {
        //                //if (Utility.Encryption.AESDecrypt(otprs.Code.ToString()).Equals(authenCode.Trim()))
        //                if (otprs.Code.Equals(Utility.Encryption.EncryptPasswordPlantext(userId, authenCode.Trim())))
        //                {
        //                    if (otprs.Retry <= maxRetry && otprs.ExpiredDate > DateTime.Now)
        //                    {
        //                        rs.AuthenResult = true;
        //                        rs.AuthenErrorCode = "success";
        //                        rs.AuthenErrorDesc = "OK";
        //                        await IdpCaching.SetStringAsync(cacheKey, "");
        //                        return rs;
        //                    }
        //                    else
        //                    {
        //                        rs.AuthenResult = false;
        //                        rs.AuthenErrorCode = "otp_expired";
        //                        rs.AuthenErrorDesc = "OTP expired";
        //                        return rs;
        //                    }
        //                }
        //                else
        //                {
        //                    otprs.Retry = otprs.Retry + 1;
        //                    await IdpCaching.SetStringAsync(cacheKey, JsonConvert.SerializeObject(otprs).ToString());
        //                }
        //            }
        //            return rs;
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error(ex.ToString());
        //            throw new IdpException("unable_verifyotpcode", ex.ToString());
        //        }
        //    }
        //}
        ////HaiTX add thêm otp với email
        //public class EMAIL
        //{
        //    public async ValueTask<SMSOTPRecord> GenarateOTP(string userId)
        //    {
        //        int otpLen = 6;
        //        int.TryParse((await IdpCaching.GetObjectAsync("Sys_Variables", "SMSOTPLEN")).Value, out otpLen);

        //        int otpLifeTime = 3;
        //        int.TryParse((await IdpCaching.GetObjectAsync("Sys_Variables", "SMSOTPLIFETIME")).Value, out otpLifeTime);

        //        Random ran = new Random();
        //        byte[] auCode = new byte[otpLen + 1];
        //        string strAuCode = string.Empty;
        //        for (int i = 0; i < otpLen; i++)
        //        {
        //            auCode[i] = (byte)ran.Next(10);
        //            strAuCode = strAuCode + auCode[i].ToString();

        //        }
        //        string cacheKey = $"SO_{userId}";
        //        var otpDetail = new SMSOTPRecord
        //        {
        //            Code = Utility.Encryption.EncryptPasswordPlantext(userId, strAuCode),
        //            Retry = 0,
        //            ExpiredDate = DateTime.Now.AddMinutes(otpLifeTime),
        //            ExpiredLifeTime = otpLifeTime
        //        };
        //        await IdpCaching.SetStringAsync(cacheKey, JsonConvert.SerializeObject(otpDetail).ToString());
        //        otpDetail.Code = strAuCode;
        //        return otpDetail;
        //    }

        //    public async ValueTask<AuthenticationResult> Authenticate(string userId, string authenCode)
        //    {
        //        try
        //        {
        //            int maxRetry = 5;
        //            int.TryParse((await IdpCaching.GetObjectAsync("Sys_Variables", "SMSOTPMAXRETRY")).Value, out maxRetry);
        //            var rs = new AuthenticationResult
        //            {
        //                AuthenType = Constant.AUTHENTYPE.SMSOTP,
        //                AuthenResult = false,
        //                AuthenErrorCode = "invalid_otp",
        //                AuthenErrorDesc = "Invalid OTP Code",
        //            };
        //            //Fiona - check with static OTP in advance

        //            var otpStatic = await IdpCaching.GetObjectAsync("Ctm_OtpWhiteList", userId);
        //            if (otpStatic != null && ((string)otpStatic.Code).Equals(authenCode))
        //            {
        //                rs.AuthenResult = true;
        //                rs.AuthenErrorCode = "success";
        //                rs.AuthenErrorDesc = "OK";
        //                return rs;
        //            }


        //            string cacheKey = $"SO_{userId}";
        //            var otprs = await IdpCaching.GetObjectAsync(cacheKey);
        //            if (otprs != null)
        //            {
        //                //if (Utility.Encryption.AESDecrypt(otprs.Code.ToString()).Equals(authenCode.Trim()))
        //                if (otprs.Code.Equals(Utility.Encryption.EncryptPasswordPlantext(userId, authenCode.Trim())))
        //                {
        //                    if (otprs.Retry <= maxRetry && otprs.ExpiredDate > DateTime.Now)
        //                    {
        //                        rs.AuthenResult = true;
        //                        rs.AuthenErrorCode = "success";
        //                        rs.AuthenErrorDesc = "OK";
        //                        await IdpCaching.SetStringAsync(cacheKey, "");
        //                        return rs;
        //                    }
        //                    else
        //                    {
        //                        rs.AuthenResult = false;
        //                        rs.AuthenErrorCode = "otp_expired";
        //                        rs.AuthenErrorDesc = "OTP expired";
        //                        return rs;
        //                    }
        //                }
        //                else
        //                {
        //                    otprs.Retry = otprs.Retry + 1;
        //                    await IdpCaching.SetStringAsync(cacheKey, JsonConvert.SerializeObject(otprs).ToString());
        //                }
        //            }
        //            return rs;
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error(ex.ToString());
        //            throw new IdpException("unable_verifyotpcode", ex.ToString());
        //        }
        //    }
        //}
        //public class SmartOTP
        //{
        //    public async ValueTask<AuthenticationResult> Authenticate(string userId, string authenCode, string deviceId)
        //    {
        //        try
        //        {
        //            var rs = new AuthenticationResult
        //            {
        //                AuthenType = Constant.AUTHENTYPE.SMARTOTP,
        //                AuthenResult = false,
        //                AuthenErrorCode = "invalid_otp",
        //                AuthenErrorDesc = "Invalid OTP Code",
        //            };
        //            string base32Secret = "";

        //            var userAuthenInfo = await IdpCaching.GetObjectAsync("Ctm_UserAuthen", userId, Constant.AUTHENTYPE.SMARTOTP);

        //            if (userAuthenInfo == null)
        //            {
        //                userAuthenInfo.AuthenErrorCode = "authen_notfound";
        //                userAuthenInfo.AuthenErrorDesc = "User haven't register authentication";
        //            }
        //            else if (!deviceId.Trim().Equals(userAuthenInfo.DeviceID))
        //            {
        //                userAuthenInfo.AuthenErrorCode = "authen_untrusteddevice";
        //                userAuthenInfo.AuthenErrorDesc = "Untrusted device, please logout and login authen to reative your device";
        //            }
        //            else
        //            {
        //                base32Secret = Utility.Encryption.GetSmartOTPSecret(deviceId, userAuthenInfo.ActiveCode);
        //                var secret = Base32Encoding.ToBytes(base32Secret);
        //                var totp = new Totp(secret);
        //                int allowedDelay = 5;
        //                int.TryParse((await IdpCaching.GetObjectAsync("Sys_Variables", "SMARTOTPALLOWEDDELAY")).Value, out allowedDelay);

        //                bool valid = totp.VerifyTotp(authenCode, out long timeStepMatched, new VerificationWindow(allowedDelay, allowedDelay));

        //                if (valid)
        //                {
        //                    rs.AuthenResult = true;
        //                    rs.AuthenErrorCode = "success";
        //                    rs.AuthenErrorDesc = "OK";
        //                }
        //                else
        //                {
        //                    userAuthenInfo.AuthenErrorCode = "authen_invalidsmartotp";
        //                    userAuthenInfo.AuthenErrorDesc = "Invalid authentication, please check and make sure your device has the right date time";
        //                }
        //            }
        //            return rs;
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error(ex.ToString());
        //            throw new IdpException("unable_verifyotpcode", ex.ToString());
        //        }
        //    }
        //}
        //public class PinPass
        //{
        //    public AuthenticationResult Authenticate(string userId, string authenCode)
        //    {
        //        return new AuthenticationResult();
        //    }
        //}
    }
}
