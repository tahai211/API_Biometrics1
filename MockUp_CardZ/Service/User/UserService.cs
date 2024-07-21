using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MockUp_CardZ.Data;
using MockUp_CardZ.DTO.Entity;
using MockUp_CardZ.DTO.ResponseDTO;
using MockUp_CardZ.Infra.Common;
using MockUp_CardZ.Infra.Common.HttpCustom;
using MockUp_CardZ.Infra.Constans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Service.User
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }
        public async ValueTask<LoginResponseDTO> Authentication(string userName, string passWord, string serviceId)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passWord))
            {

            }
            var service = await _context.SysService.Where(x => x.ServiceId == serviceId).FirstOrDefaultAsync();
            var timeCheckUserAtion = service.CheckUserAction;
            var timeRevokeToken = service.TimeRevokeToken;
            var timeShowCountDown = service.TimeShowCountDown;
            var timeNow = DateTime.Now;
            bool changePwAge = false;
            var loginInfo = await _context.SysLoginInfos.AsQueryable()
             .Where(x => x.LoginName == userName && x.ServiceId == serviceId && x.Status != UserStatus.Delete)
             .Join(_context.SysUserLogins.AsQueryable(), login => login.LoginId, user => user.LoginId, (login, user) => new { login, user })
             .FirstOrDefaultAsync();
            if (loginInfo == null) 
            {  
                throw new SysException("accountnotsupported", "Your account is not supported by this service, please contact the nearest bank for more support.");
            }
            if (((string)loginInfo.login.Status) != UserStatus.Active)
            {
                throw new SysException("userloginfailnotactive", "Login failed, user is not active");
            }

            #region Check Policy
            var policy = await _context.SysPolicies.Where(m => m.PolicyId == loginInfo.login.PolicyId).SingleOrDefaultAsync();
            if (DateTime.Compare(timeNow, policy.EfFrom ?? timeNow) == -1 || DateTime.Compare(policy.EfTo ?? timeNow, timeNow) == -1)
            {
                throw new SysException("expiredpolicy", "Please contact to Bank");
            }
            if (TimeSpan.Compare(policy.AccessTimeFrom, timeNow.TimeOfDay) == 1 || TimeSpan.Compare(timeNow.TimeOfDay, policy.AccessTimeTo) == 1)
            {
                throw new SysException("time_invalid_from_to", "You can only login from {timeFrom} to {timeTo}.Please try later", new { timeFrom = policy.AccessTimeFrom, timeTo = policy.AccessTimeTo });
            }
            // ngăn việc login thất bại quá nhiều lần...
            if (loginInfo.login.FailNumber != 0 && loginInfo.login.FailNumber >= policy.FailAccessNumber)
            {
                if (policy.ResetPwTime == 0 || loginInfo.login.LastLoginFail == null)
                {
                    throw new SysException("accounthavelocked", "Your account has been disabled because of failed login times is over regulations of Bank");
                }
                else
                {
                    TimeSpan peroidTime = (timeNow - (DateTime)loginInfo.login.LastLoginFail);
                    var remainingTime = policy.ResetPwTime * 60 - peroidTime.TotalSeconds;
                    if (remainingTime > 0)
                    {
                        throw new SysException("fail_number_excess_at_time", "Plese try again after {duration} seconds", new { duration = (int)remainingTime });

                    }
                }
            }
            #endregion

            #region Check login password Biometric
            var login = await _context.SysLoginInfos
                   .Where(m => m.LoginId == loginInfo.login.LoginId && m.ServiceId == serviceId)
                   .SingleOrDefaultAsync();
            // check token (nếu token truyền vào k giống trong DB => fail)    
            //if (!string.IsNullOrEmpty(BiometricToken))
            //{
            //    AuthenticationUtility.AuthenticationResult authenticationResult = await new AuthenticationUtility.BioMetricToken().Authenticate(loginInfo.user.UserId, serviceId, deviceId, BiometricToken, context);
            //    if (!authenticationResult.AuthenResult)
            //    {
            //        login.FailNumber += 1;
            //        login.LastLoginFail = DateTime.Now;
            //        login.BiometricToken = string.Empty; //Fiona - clear biometric when biotoken is fail
            //        await digitalContext.SaveChangesAsync();
            //        throw new IdpException("unauthorizationbiometric", "Biometric method is invalid on your device. Please login by your password.");
            //    }
            //}
            if (!string.IsNullOrEmpty(passWord))
            {
                var passString = Encryption.EncryptPassword(loginInfo.login.LoginId, passWord);
                var pass = await _context.SysPasswords.AsQueryable()
                    .Where(x => x.LoginId == loginInfo.login.LoginId && x.ServiceId == serviceId
                        && x.Type == loginInfo.login.AuthenType && x.Password == passString)
                    .SingleOrDefaultAsync();

                if (pass == null)
                {
                    login.FailNumber += 1;
                    login.LastLoginFail = DateTime.Now;
                    await _context.SaveChangesAsync();
                    throw new SysException("unauthorization", "Login information is not correct");
                }
            } 
            else
            {
                throw new SysException("unauthorization", "Login information is not correct");
            }
                #endregion

            #region check password age

            var PwLastChange = _context.SysPasswordHis
                .Where(x => x.LoginId == loginInfo.user.LoginId && x.Type.ToUpper() == loginInfo.login.AuthenType && x.ServiceId == serviceId)
                .OrderByDescending(x => x.ChangeTimeNumber)
                .FirstOrDefault();
            if (policy.PwAge > 0 && PwLastChange != null)
            {
                if (PwLastChange.DateModified == null) changePwAge = true;
                else
                {
                    TimeSpan peroidTimes = (timeNow - (DateTime)PwLastChange.DateModified);
                    changePwAge = (policy.PwAge - Math.Floor(peroidTimes.TotalDays)) <= 0;
                }
            }
            #endregion

            #region Get Info User
            var user = await _context.SysUsers.AsQueryable()
                    .Where(x => x.UserId == loginInfo.user.UserId && x.Status == UserStatus.Active)
                    .SingleOrDefaultAsync();
            //var branchInfo = (await IdpCaching.GetObjectAsync("Idp_Branch", user.BranchCode, bankCode));
            //if (branchInfo != null) branchName = branchInfo.BranchName;
            // log thông tin đăng nhập vào historyLogin & loginInfor
            login.FailNumber = 0;
            login.LastLoginTime = DateTime.Now;
            bool checksendMail = false; //checkNewDevice ? true : false;
            //bool checkOTP = isMBIBBO(serviceId) && isNewDevice(deviceId, login.DeviceId);
            //bool checkNewDevice = isNewDevice(deviceId, login.DeviceId);
            //var checkOldDevice = login.DeviceId;
            //ImgAvatar = user.Avatar == null ? (await IdpCaching.GetObjectAsync("Sys_Variables", "IMG_AVATAR_DEFAIL")).Value : user.Avatar;
            var loginhisInfo = new SysLoginHistory();
            loginhisInfo.UserId = "userid";
            loginhisInfo.LoginTime = DateTime.Now;
            loginhisInfo.ServiceId = serviceId;
            loginhisInfo.DeviceInfo = "deviceInfo";
            loginhisInfo.IpAddress = "address";
            loginhisInfo.Location = "Not Found";
            _context.SysLoginHistories.Add(loginhisInfo);
            _context.SaveChanges();
            await _context.SaveChangesAsync();
            return new LoginResponseDTO
            {
                Result = true,
                UserId = loginInfo.user.UserId,
                TempToken = false,
                UserInfo = new UserInfoDTO
                {
                    CheckSendMail = checksendMail,
                    PhoneNo = user.PhoneNo,
                    FullName = user.FullName,
                    Email = user.Email,
                    Image = "ImgAvatar",
                    NeedOTP = false,
                    LastLogin = loginInfo.login.LastLoginTime,
                    HavePin = false,
                    Biometric = false,
                    FirstLogin = (bool)loginInfo.login.NewLogin || changePwAge,
                    IsNewDevice = false,
                    TimeCheckUserAction = timeCheckUserAtion, // Đảm bảo rằng biến này đã được khai báo và gán giá trị
                    TimeRevokeToken = timeRevokeToken, // Đảm bảo rằng biến này đã được khai báo và gán giá trị
                    TimeShowCountDown = timeShowCountDown // Đảm bảo rằng biến này đã được khai báo và gán giá trị
                }
            };

            #endregion

            #region Todo Check Device
            //var userInfo = await IdpCaching.GetObjectAsync("Ctm_Users", loginInfo.user.UserId);
            //if (!((string)userInfo.Status).Equals("A"))
            //    throw new IdpException("userloginfailnotactive", "Login failed, user is not active");
            //var custInfo = await IdpCaching.GetObjectAsync("Ctm_CustInfo", (string)userInfo.CustId);
            //if (!((string)custInfo.Status).Equals("A"))
            //    throw new IdpException("customerstatusisvalid", "Customer status is invalid");

            ////GiapTT 20240410 Trả thêm BranchName
            //var branchInfo = (await IdpCaching.GetObjectAsync("Idp_Branch", custInfo.BranchId, bankCode));
            //if (branchInfo != null) branchName = branchInfo.BranchName;
            //if (Authentication != null)
            //{
            //    string AuthenType = Authentication.AuthenType;
            //    string AuthenCode = Authentication.AuthenCode;
            //    string DeviceIdAuthen = userInfo.PhoneNo;
            //    login.DeviceId = deviceId;
            //    await AuthenticationUtility.AuthenTranForUserNotLogin(AuthenType, AuthenCode, loginInfo.user.UserId, DeviceIdAuthen, context);
            //    addLoginHis(digitalContext, loginInfo.user.UserId, serviceId, clientInfo["UserAgent"].ToString(), clientInfo["IpAddress"].ToString(), string.Empty);
            //    await digitalContext.SaveChangesAsync();
            //}
            //ImgAvatar = userInfo.Image == null ? (await IdpCaching.GetObjectAsync("Sys_Variables", "IMG_AVATAR_DEFAIL")).Value : userInfo.Image;
            //// first login or new device
            //bool checkOTP = isMBIBBO(serviceId) && isNewDevice(deviceId, login.DeviceId);
            ////BaoNT 21112023 Sửa checkChagepass check thêm changePwAge
            //bool checkChagepass = (bool)loginInfo.login.NewLogin || changePwAge;
            //bool checkNewDevice = isNewDevice(deviceId, login.DeviceId);
            //var checkOldDevice = login.DeviceId;
            //checksendMail = Authentication != null ? true : false;

            ////check role hunglt
            //var userInRole = await digitalContext.IdpUserInRoles.Where(x => x.UserId == loginInfo.user.UserId)
            //        .Join(digitalContext.IdpRoles.Where(c => c.ServiceId == serviceId), user => user.RoleId, role => role.RoleId, (user, role) => new { role }).FirstOrDefaultAsync();
            //if (userInRole == null) throw new IdpException("usernothaveservicerole", "Your user is not supported by this banking service. Please contact the nearest bank for further support.");

            ////
            //if (loginInfo.login.NewLogin == true || isNewDevice(deviceId, login.DeviceId))
            //{
            //    bool otp = isMulti == true ? false : checkOTP;
            //    if (serviceId == "MB" && otp)
            //    {
            //        login.BiometricToken = "";
            //    }
            //    // log thông tin đăng nhập vào historyLogin & loginInfor
            //    login.FailNumber = 0;
            //    login.LastLoginTime = DateTime.Now;

            //    // addLoginHis(digitalContext, loginInfo.user.UserId, serviceId, clientInfo["UserAgent"].ToString(), clientInfo["IpAddress"].ToString(), string.Empty);
            //    await digitalContext.SaveChangesAsync();
            //    return new
            //    {
            //        result = true,
            //        userId = loginInfo.user.UserId,
            //        companyId = string.Empty,
            //        tempToken = true,
            //        flagNewDevice = true,
            //        userInfo = new
            //        {
            //            infoLogin = new
            //            {
            //                ip = clientInfo["IpAddress"].ToString(),
            //                logintime = (DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"),
            //                channel = service.ServiceName,
            //                device = clientInfo["UserAgent"].ToString(),

            //            },
            //            custType = custInfo.CtmTypeId,
            //            //GiapTT 04032024 - Thêm BranchCode và BranchName
            //            branchCode = custInfo.BranchId,
            //            branchName = branchName,
            //            allowPincode = AllowPincode,
            //            checksendmail = checksendMail,
            //            tempToken = true,
            //            phoneNo = userInfo.PhoneNo,
            //            fullName = userInfo.FullName,
            //            image = ImgAvatar,
            //            imageUrl = imgUrl + "|" + ImgAvatar,
            //            needOTP = isMulti == true ? false : checkOTP,
            //            firstLogin = checkChagepass,
            //            lastLogin = loginInfo.login.LastLoginTime,
            //            isNewDevice = checkNewDevice,
            //            old_device = checkOldDevice,
            //            //multiProfile = isMulti,
            //            email = userInfo.Email,
            //            //HaiTX trả thêm default account và company 
            //            DefaultAcct = new
            //            {
            //                AccountNo = userInfo.DefaultAccount,
            //                companyId = (await IdpCaching.GetObjectAsync("Ctm_CustAccount", (string)userInfo.CustId, (string)userInfo.DefaultAccount)) != null ? (string)(await IdpCaching.GetObjectAsync("Ctm_CustAccount", (string)userInfo.CustId, (string)userInfo.DefaultAccount)).CompanyId : string.Empty,
            //            },
            //            //GiapTT - 04-08-2023 trả ra list Profile
            //            lstProfile = genlistUserInfo(digitalContext, usermulti.Select(x => x.UserId).ToArray()),

            //            //GiapTT them biến time check
            //            timeCheckUserAtion = timeCheckUserAtion,
            //            timeRevokeToken = timeRevokeToken,
            //            timeShowCountDown = timeShowCountDown,
            //            //serviceName = serviceId,
            //        },
            //    };
            //}
            //else//HaiTX gộp trường hợp k newLogin and newDevice
            //{

            //    addLoginHis(digitalContext, loginInfo.user.UserId, serviceId, clientInfo["UserAgent"].ToString(), clientInfo["IpAddress"].ToString(), string.Empty);
            //    login.FailNumber = 0;
            //    login.LastLoginTime = DateTime.Now;
            //    await digitalContext.SaveChangesAsync();
            //    return new
            //    {
            //        result = true,
            //        userId = loginInfo.user.UserId,
            //        companyId = string.Empty,
            //        tempToken = loginInfo.login.NewLogin,
            //        userInfo = new
            //        {
            //            infoLogin = new
            //            {
            //                ip = clientInfo["IpAddress"].ToString(),
            //                logintime = (DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"),
            //                channel = service.ServiceName,
            //                device = clientInfo["UserAgent"].ToString(),

            //            },
            //            custType = custInfo.CtmTypeId,
            //            //GiapTT 04032024 - Thêm BranchCode và BranchName
            //            branchCode = custInfo.BranchId,
            //            branchName = branchName,
            //            allowPincode = AllowPincode,
            //            checksendmail = checksendMail,
            //            tempToken = loginInfo.login.NewLogin,
            //            phoneNo = userInfo.PhoneNo,
            //            fullName = userInfo.FullName,
            //            email = userInfo.Email,
            //            image = ImgAvatar,
            //            imageUrl = imgUrl + "|" + ImgAvatar,
            //            needOTP = isMulti == true ? false : checkOTP,
            //            firstLogin = checkChagepass,
            //            lastLogin = loginInfo.login.LastLoginTime,
            //            isNewDevice = checkNewDevice,
            //            //serviceName = serviceId,
            //            //current_device = deviceId,
            //            old_device = checkOldDevice,
            //            //isMBIBBO = isMBIBBO(serviceId),
            //            authentypeUser = AuthentypeUser,
            //            havePin = HavePin,
            //            changePwAge = changePwAge,
            //            //HaiTX trả thêm default account và company 
            //            DefaultAcct = new
            //            {
            //                AccountNo = userInfo.DefaultAccount,
            //                companyId = (await IdpCaching.GetObjectAsync("Ctm_CustAccount", (string)userInfo.CustId, (string)userInfo.DefaultAccount)) != null ? (string)(await IdpCaching.GetObjectAsync("Ctm_CustAccount", (string)userInfo.CustId, (string)userInfo.DefaultAccount)).CompanyId : string.Empty,
            //            },
            //            //multiProfile = isMulti,
            //            flagNewDevice = true,
            //            lstProfile = genlistUserInfo(digitalContext, usermulti.Select(x => x.UserId).ToArray()),
            //            //GiapTT them biến time check
            //            timeCheckUserAtion = timeCheckUserAtion,
            //            timeRevokeToken = timeRevokeToken,
            //            timeShowCountDown = timeShowCountDown,

            //        },
            //    };
            //}
            #endregion
        }
        public async ValueTask<object> UpdateRefreshToken(RefreshTokenEntity refreshToken, string userId, string serviceId)
        {
            var refreshtoken = await _context.SysUserAccessTokens.Where(x => x.UserId == userId && x.Service == serviceId && x.Status == UserStatus.Active).FirstOrDefaultAsync();
            if(refreshtoken != null)
            {
                _context.SysUserAccessTokens.Remove(refreshtoken);
                await _context.SaveChangesAsync();

            }
            return true;
        }

        public async ValueTask<object> AddRefreshToken(RefreshTokenEntity refreshToken, string userId, string serviceId)
        {
            var refreshtoken = await _context.SysUserAccessTokens.Where(x => x.UserId == userId && x.Service == serviceId && x.Status == UserStatus.Active).ToListAsync();
            foreach(var toke in refreshtoken)
            {
                _context.SysUserAccessTokens.Remove(toke);
                await _context.SaveChangesAsync();
            }
            SysUserAccessToken tokennew = new SysUserAccessToken();
            tokennew.Data = "";
            tokennew.UserId = userId;
            tokennew.Service = serviceId;
            tokennew.Token = refreshToken.RefreshToken;
            tokennew.ExpiredDate = refreshToken.ExpiryDate;
            tokennew.Status = UserStatus.Active;
            return new { };
        }

        public async ValueTask<RefreshTokenEntity> GetRefreshToken(string token, string userId, string serviceId)
        {

            return new RefreshTokenEntity();
        }

        public async ValueTask<SysUser> GetUserById(string userId)
        {
            return new SysUser();
        }

    }
}