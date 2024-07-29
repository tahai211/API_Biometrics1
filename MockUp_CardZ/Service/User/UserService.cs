using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using MockUp_CardZ.Data;
using MockUp_CardZ.DTO.Entity;
using MockUp_CardZ.DTO.ResponseDTO;
using MockUp_CardZ.Infra.Common;
using MockUp_CardZ.Infra.Common.HttpCustom;
using MockUp_CardZ.Infra.Constans;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
            var service = await _context.SysServices.Where(x => x.ServiceId == serviceId).FirstOrDefaultAsync();
            if(service == null)
            {
                throw new SysException("serviceisnull", "Service is null");
            }
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
            SysUserAccessToken tokenInfo = await _context.SysUserAccessTokens.Where(x => x.UserId == userId && x.Token == token 
            && x.Service == serviceId && x.Status == "A").FirstOrDefaultAsync();
            RefreshTokenEntity refreshToke = new RefreshTokenEntity();
            refreshToke.RefreshToken = tokenInfo.Token;
            refreshToke.UserId = tokenInfo.UserId;
            refreshToke.ExpiryDate = tokenInfo.ExpiredDate;
            return new RefreshTokenEntity();
        }

        public async ValueTask<SysUser> GetUserById(string userId)
        {
            SysUser userInfo = await _context.SysUsers.Where(x => x.UserId == userId).FirstOrDefaultAsync();
            if(userInfo == null)
            {
                throw new SysException("userisnot", "");
            }
            return userInfo;
        }
        public async ValueTask<object> GetListUser(string? serviceId, string? status, string? userName, string? name, string? branch, string? email, string? phoneNo, string? companyId, int pageIndex = 1, int pageSize = 0)
        {
            
            int skip = ((pageIndex - 1) * pageSize);
            //HaiTX Sửa  loại bỏ service id trong kq để ra 1 dòng khi k truyền service Id 
            var lstUsers = digitalContext.IdpUsers.AsQueryable()
        .Where(x => (string.IsNullOrEmpty(status) || x.Status == status) && (x.Status != "D") && (string.IsNullOrEmpty(branch) || x.BranchCode == branch))
        .Join(
            digitalContext.IdpUserLogins.AsQueryable()
            .Join(
                digitalContext.IdpLoginInfos.AsQueryable().Where(userlogin => (string.IsNullOrEmpty(serviceId) || userlogin.ServiceId == serviceId) && userlogin.Status != "D"),
                userlogin => userlogin.LoginId,
                login => login.LoginId,
                (userlogin, login) => new { userlogin.UserId, login.LoginName, login.ServiceId }
            ),
            a => a.UserId,
            b => b.UserId,
            (a, b) => new
            {
                UserId = a.UserId,
                UserName = b.LoginName,
                BranchCode = a.BranchCode,
                BranchName = digitalContext.IdpBranches.AsQueryable().Any(code => code.BranchCode == a.BranchCode) ?
                digitalContext.IdpBranches.AsQueryable().Where(code => code.BranchCode == a.BranchCode).FirstOrDefault().BranchName : a.BranchCode,
                Email = a.Email,
                Portal = string.IsNullOrEmpty(serviceId) ? "" : (digitalContext.IdpServices.AsQueryable().Any(code => code.ServiceId == b.ServiceId) ?
                digitalContext.IdpServices.AsQueryable().Where(code => code.ServiceId == b.ServiceId).FirstOrDefault().ServiceName : b.ServiceId),
                PhoneNo = a.PhoneNo,
                FirstName = a.FirstName,
                MiddleName = a.MiddleName,
                LastName = a.LastName,
                FullName = a.FullName,
                Status = a.Status,
                CompanyId = a.CompanyId,
                date = a.DateCreated, // Convert DateCreated to the desired format
                DateCreated = "",
                StatusStr = digitalContext.IdpCodes.AsQueryable().Any(code => code.VarGroup.Equals("User_Status") && code.VarKey.Equals(a.Status)) ?
                    digitalContext.IdpCodes.AsQueryable().Where(code => code.VarGroup.Equals("User_Status") && code.VarKey.Equals(a.Status)).FirstOrDefault().VarValue : a.Status
            }
        )
        .Where(x => x.UserName.Contains(userName.Trim()) && x.FullName.Contains(name.Trim())
            && (String.IsNullOrEmpty(branch) || x.BranchCode == branch)
            && (String.IsNullOrEmpty(email) || x.Email == email)
        && (String.IsNullOrEmpty(companyId) || x.CompanyId == companyId)
            && (String.IsNullOrEmpty(email) || x.Email == email)
            && (String.IsNullOrEmpty(phoneNo) || x.PhoneNo == phoneNo))
        .Distinct();
            var formattedUsers = lstUsers.Select(temp => new
            {
                UserId = temp.UserId,
                UserName = temp.UserName,
                BranchCode = temp.BranchCode,
                BranchName = temp.BranchName,
                Email = temp.Email,
                Portal = temp.Portal,
                PhoneNo = temp.PhoneNo,
                FirstName = temp.FirstName,
                MiddleName = temp.MiddleName,
                LastName = temp.LastName,
                FullName = temp.FullName,
                Status = temp.Status,
                CompanyId = temp.CompanyId,
                DateCreated = ((DateTime)temp.date).ToString("dd/MM/yyyy HH:mm:ss"),
                StatusStr = temp.StatusStr
            }).ToList();

            var total = formattedUsers.Count();
            if (pageSize != 0)
            {
                var data = formattedUsers.Skip(skip).Take(pageSize);
                int pages = formattedUsers.Count() % pageSize >= 1 ? formattedUsers.Count() / pageSize + 1 : formattedUsers.Count() / pageSize;
                return new { data = data.ToList(), pages = pages, total = total, pageIndex = pageIndex };
            }
            else
            {
                return new { data = formattedUsers.ToList(), pages = 0, total = total, pageIndex = 1 };
            }
        }
        public async ValueTask<object> GetDetailUser(string userId)
        {
            var user = digitalContext.IdpUsers.AsQueryable()
            .Select(a => new
            {
                UserId = a.UserId,
                PhoneNo = a.PhoneNo,
                BranchCode = a.BranchCode,
                PolicyId = a.PolicyId,
                FirstName = a.FirstName,
                MiddleName = a.MiddleName,
                CompanyId = a.CompanyId,
                LastName = a.LastName,
                Gender = a.Gender,
                Address = a.Address,
                Email = a.Email,
                //HaiTX lấy SourceId để phân biệt tài khaonr LDAP hay k 
                SourceId = a.SourceId,
                Birthday = ((DateTime)a.Birthday).ToString("MM/dd/yyyy"),
                Status = a.Status,
                UserType = a.UserType,
                EmployeeId = a.EmployeeId,
                //UserHaveCore = !string.IsNullOrEmpty(a.Option),
                //haitx trả thêm username core 
                CoreInfo = string.IsNullOrEmpty(a.CbsInfo) ? new { } : Decode(a.CbsInfo)

            }).SingleOrDefault(m => m.UserId == userId);
            var userRoles = new Object();
            if (string.IsNullOrEmpty(userId))
            {
                userRoles = digitalContext.IdpRoles.AsQueryable().Where(x => x.ServiceId == "BO" || x.ServiceId == "RPT").Select(y => new { value = y.RoleId });
            }
            else
            {
                userRoles = digitalContext.IdpUserInRoles.AsQueryable().Where(x => x.UserId == userId).Select(y => new { value = y.RoleId });
            }

            var userLogin = digitalContext.IdpUserLogins.AsQueryable().Where(x => x.UserId == userId).Join(digitalContext.IdpLoginInfos.AsQueryable(),
                ul => ul.LoginId, l => l.LoginId, (ul, l) => new { ul.UserId, l.LoginName, l.ServiceId });
            var userGroup = digitalContext.IdpRoles.AsQueryable().Where(x => userLogin.Where(u => u.ServiceId == x.ServiceId).Count() > 0).ToList()
                .GroupJoin(digitalContext.IdpUserInRoles.AsQueryable().Where(x => x.UserId == userId), r => r.RoleId, u => u.RoleId,
                (r, u) => new
                {
                    shortcut = "",
                    label = r.RoleName,
                    RoleName = r.RoleName,
                    value = r.RoleId,
                    RoleId = r.RoleId,
                    serviceId = r.ServiceId,
                    check = u.Count() > 0
                });

            return new { User = user, UserGroup = userGroup, Login = userLogin, UserRole = userRoles };
        }
        public async ValueTask<object> UpdateUser(string userName, string password, string firstName, string middleName, string lastName,
        string gender, string email, string phoneNo, string birthday, string address, string branch, int policy, dynamic rolesBO, dynamic rolesRPT, dynamic roleThirdparty, string companyId, string id,
         string userType, bool autoGenPass, string typesend, string actionType, string userNameCbs,  string employeeId, string serviceId, string sourceId = "", string tokencbs = "")
        {
            var userId = id;
            var serviceIds = companyId == "IDP" ? "BO" : serviceId;
            if (string.IsNullOrEmpty(actionType) && !actionType.Equals("ADD") && !actionType.Equals("EDIT"))
                throw new SysException("page_action_invalid", "Invalid action");

            SysUser user = await _context.SysUsers.Where(m => m.UserId == userId).FirstOrDefaultAsync();
            //string userPerform = (string)_context.GetVariable(VariableKey.UserId);
            if (actionType == "EDIT")
            {
                if (user == null) throw new SysException("User Name is not existed", "");
                //user = digitalContext.IdpUsers.AsQueryable.SingleOrDefault(m => m.UserId == userId);
                user.UserModified = "userPerform";
                user.DateModified = DateTime.Now;
                sourceId = user.SourceId;
            }
            else if (actionType == "ADD")
            {
                if (user != null) throw new SysException("bo_user_name", "User is existed");
                var check = _context.SysLoginInfos.Where(m => m.LoginName == userName && m.Status != "D" && (m.ServiceId == "BO" || m.ServiceId == "RPT" || m.ServiceId == serviceId)).Join(_context.SysUserLogins,
                     login => login.LoginId, userlogin => userlogin.LoginId, (login, userlogin) => new { login.LoginId }).Count();
                var checkthirdparty = _context.SysLoginInfos.Where(m => m.LoginName == userName && m.Status != "D" && m.ServiceId == "TP").Join(_context.SysUserLogins,
                   login => login.LoginId, userlogin => userlogin.LoginId, (login, userlogin) => new { login.LoginId }).Count();
                if (check > 0 || checkthirdparty > 0)
                {
                    throw new SysException("bo_user_name", "User Name is existed");
                }
                user = new SysUser();
                user.UserId = Guid.NewGuid().ToString("N");
                user.UserCreated = "userPerform";
                user.DateCreated = DateTime.Now;
                user.Status = "A";

            }
            user.UserType = userType;
            user.PhoneNo = phoneNo;
            user.FirstName = firstName;
            user.MiddleName = middleName;
            user.LastName = lastName;
            user.FullName = firstName + " " + middleName + " " + lastName;
            user.Gender = gender;
            user.Address = address;
            user.Email = email;
            user.Birthday = birthday == "" ? null : Convert.ToDateTime(birthday);
            user.BranchCode = branch;
            user.CompanyId = companyId;
            

            user.PolicyId = policy;
            if (actionType == "ADD")
            {
                digitalContext.IdpUsers.Add(user);
                List<IdpPassword> ltsPassword = new List<IdpPassword>();
                List<string> ltsService = new List<string>();
                var LoginId = Guid.NewGuid().ToString();
                ltsService.Add(companyId == "IDP" ? "BO" : serviceId);
                if (rolesRPT != null && rolesRPT.Count > 0)
                {
                    ltsService.Add("RPT");
                }
                if (autoGenPass) password = Utility.Encryption.GenaratePassword(8, true);
                for (int i = 0; i < ltsService.Count(); i++)
                {
                    IdpLoginInfo loginInfo = new IdpLoginInfo();
                    loginInfo.LoginId = LoginId;
                    loginInfo.LoginName = userName;
                    loginInfo.LoginType = "USERNAME";
                    loginInfo.AuthenType = "PASSWORD";
                    loginInfo.FailNumber = 0;
                    loginInfo.NewLogin = true;
                    if (ltsService[i] != "BO")
                        loginInfo.NewLogin = false;
                    loginInfo.PolicyId = policy;
                    loginInfo.DateExpired = DateTime.Now.AddYears(20);
                    loginInfo.Status = "A";
                    loginInfo.UserCreated = userId;
                    loginInfo.DateCreated = DateTime.Now;
                    loginInfo.ServiceId = ltsService[i];
                    digitalContext.IdpLoginInfos.Add(loginInfo);
                    IdpPassword pass = new IdpPassword();
                    pass.LoginId = loginInfo.LoginId;
                    pass.Type = "PASSWORD";
                    // if (typesend.ToLower() == "email" && autoGenPass == true)
                    // {
                    //     if (string.IsNullOrEmpty(email)) throw new IdpException("Email is not null", "");
                    //     emailinfo.sourceId = context.WorkflowInstance.Id;
                    //     emailinfo.to = email;
                    //     emailinfo.cc = "";
                    //     emailinfo.title = "Pass/Pin ";
                    //     emailinfo.body = "New pass: " + password;
                    //     emailinfo.attachment = "";
                    // }
                    //HaiTX check nếu LDAP thì k add pass
                    if (sourceId != "LDAP")
                    {
                        if (autoGenPass)
                        {
                            pass.Password = Utility.Encryption.EncryptPasswordPlantext(loginInfo.LoginId, password);
                            pass.PasswordTemp = Utility.Encryption.AESEncrypt(password);
                        }
                        else
                        {
                            pass.Password = Utility.Encryption.EncryptPassword(loginInfo.LoginId, password);
                        }
                        pass.ServiceId = ltsService[i];
                        digitalContext.IdpLoginInfos.Add(loginInfo);
                        digitalContext.IdpPasswords.Add(pass);
                    }
                }
                IdpUserLogin login = new IdpUserLogin();
                login.UserId = user.UserId;
                login.LoginId = LoginId;
                // login.ServiceId = companyId != "IDP" ? "TP" : "BO";
                login.Status = "A";
                digitalContext.IdpUserLogins.Add(login);
            }
            else
            {
                var userGroup = digitalContext.IdpUserInRoles.AsQueryable().Where(m => m.UserId == user.UserId)
                    .Join(digitalContext.IdpRoles.AsQueryable().Where(m => m.ServiceId == "BO" || m.ServiceId == "RPT" || m.ServiceId == "TP" || m.ServiceId == serviceId), a => a.RoleId,
                    b => b.RoleId, (a, b) => new { a, b });
                foreach (var g in userGroup)
                {
                    digitalContext.IdpUserInRoles.Remove(g.a);
                }
                string loginId = (await digitalContext.IdpUserLogins.Where(m => m.UserId == user.UserId).FirstOrDefaultAsync()).LoginId;
                IdpPassword idpPass = await digitalContext.IdpPasswords.Where(m => m.LoginId == loginId && m.ServiceId != "RPT").FirstOrDefaultAsync();
                if (rolesRPT != null && rolesRPT.Count > 0 && !(await digitalContext.IdpLoginInfos.AnyAsync(m => m.LoginId == loginId && m.ServiceId == "RPT")))
                {
                    if (sourceId != "LDAP")
                    {
                        if (!(await digitalContext.IdpPasswords.AnyAsync(m => m.LoginId == loginId && m.ServiceId == "RPT" && m.Type == "PASSWORD")))
                        {
                            IdpPassword pass = new IdpPassword();
                            pass.LoginId = loginId;
                            pass.Type = "PASSWORD";
                            pass.Password = idpPass.Password;
                            pass.PasswordTemp = idpPass.PasswordTemp;
                            pass.ServiceId = "RPT";
                            digitalContext.IdpPasswords.Add(pass);
                        }
                        else if (await digitalContext.IdpPasswords.AnyAsync(m => m.LoginId == loginId && m.Type == "PASSWORD"))
                        {
                            IdpPassword pass = await digitalContext.IdpPasswords.Where(m => m.LoginId == loginId && m.ServiceId == "RPT").FirstOrDefaultAsync();
                            pass.Password = idpPass.Password;
                            pass.PasswordTemp = idpPass.PasswordTemp;
                        }
                    }
                    // IdpUserLogin loginRpt = new IdpUserLogin();
                    // loginRpt.UserId = user.UserId;
                    // loginRpt.LoginId = loginId;
                    // loginRpt.ServiceId = "RPT";
                    // loginRpt.Status = "A";
                    // digitalContext.IdpUserLogins.Add(loginRpt);
                    IdpLoginInfo loginRpt = new IdpLoginInfo();
                    loginRpt = await digitalContext.IdpLoginInfos.Where(x => x.LoginId == loginId).FirstOrDefaultAsync();
                    loginRpt.ServiceId = "RPT";
                    loginRpt.Status = "A";
                    loginRpt.UserCreated = userPerform;
                    loginRpt.DateCreated = DateTime.Now;
                    digitalContext.IdpLoginInfos.Add(loginRpt);
                }
                if (roleThirdparty != null && roleThirdparty.Count > 0 && !(await digitalContext.IdpLoginInfos.AnyAsync(m => m.LoginId == loginId && m.ServiceId == "TP")))
                {
                    if (sourceId != "LDAP")
                    {
                        if (!(await digitalContext.IdpPasswords.AnyAsync(m => m.LoginId == loginId && m.ServiceId == "TP" && m.Type == "PASSWORD")))
                        {
                            IdpPassword pass = new IdpPassword();
                            pass.LoginId = loginId;
                            pass.Type = "PASSWORD";
                            pass.Password = idpPass.Password;
                            pass.PasswordTemp = idpPass.PasswordTemp;
                            pass.ServiceId = "TP";
                            digitalContext.IdpPasswords.Add(pass);
                        }
                        else if (await digitalContext.IdpPasswords.AnyAsync(m => m.LoginId == loginId && m.Type == "PASSWORD"))
                        {
                            IdpPassword pass = await digitalContext.IdpPasswords.Where(m => m.LoginId == loginId && m.ServiceId == "TP").FirstOrDefaultAsync();
                            pass.Password = idpPass.Password;
                            pass.PasswordTemp = idpPass.PasswordTemp;
                        }
                    }

                    // IdpUserLogin loginThirdparty = new IdpUserLogin();
                    // loginThirdparty.UserId = user.UserId;
                    // loginThirdparty.LoginId = loginId;
                    // loginThirdparty.ServiceId = "TP";
                    // loginThirdparty.Status = "A";
                    // digitalContext.IdpUserLogins.Add(loginThirdparty);
                    IdpLoginInfo loginThirdparty = new IdpLoginInfo();
                    loginThirdparty = await digitalContext.IdpLoginInfos.Where(x => x.LoginId == loginId).FirstOrDefaultAsync();
                    loginThirdparty.ServiceId = "TP";
                    loginThirdparty.Status = "A";
                    loginThirdparty.UserCreated = userPerform;
                    loginThirdparty.DateCreated = DateTime.Now;
                    digitalContext.IdpLoginInfos.Add(loginThirdparty);
                }
                var loginInfo = await digitalContext.IdpLoginInfos.AsQueryable().Where(x => x.Status == "A")
                        .Join(digitalContext.IdpUserLogins.AsQueryable().Where(x => x.UserId == userId),
                        login => login.LoginId, user => user.LoginId, (login, user) => new { login, user }).FirstOrDefaultAsync();
                if (loginInfo == null) throw new IdpException("unauthorization", "Login information is not correct 1");
                if (((string)loginInfo.login.Status).Equals("D"))
                    throw new IdpException("unauthorization", "Login information is not correct 2");
                if (!((string)loginInfo.login.Status).Equals("A"))
                    throw new IdpException("userloginfailnotactive", "Login failed, user is not active");
                var lstLogin = await digitalContext.IdpLoginInfos.Where(m => m.LoginId == loginId).ToListAsync();
                foreach (var login in lstLogin)
                {
                    login.PolicyId = policy;
                }
            }
            if (rolesBO.Count > 0)
            {
                foreach (string item in rolesBO)
                {
                    IdpUserInRole userinRole = new IdpUserInRole();
                    userinRole.UserId = user.UserId;
                    userinRole.RoleId = int.Parse(item);
                    digitalContext.IdpUserInRoles.Add(userinRole);
                }
            }
            if (rolesRPT != null && rolesRPT.Count > 0)
            {
                foreach (string item in rolesRPT)
                {
                    IdpUserInRole userinRole = new IdpUserInRole();
                    userinRole.UserId = user.UserId;
                    userinRole.RoleId = int.Parse(item);
                    digitalContext.IdpUserInRoles.Add(userinRole);
                }
            }
            if ((roleThirdparty != null && roleThirdparty.Count > 0) || companyId != "IDP")
            {
                foreach (string item in roleThirdparty)
                {
                    IdpUserInRole userinRole = new IdpUserInRole();
                    userinRole.UserId = user.UserId;
                    userinRole.RoleId = int.Parse(item);
                    digitalContext.IdpUserInRoles.Add(userinRole);
                }
            }
            var trancodeNotification = await digitalContext.IdpNotificationTemplates.Where(x => x.TransactionCode == "ADDNEW_USER_TELLER").FirstOrDefaultAsync();
            /// laod config temp email, sms
            string interServiceName = string.Empty;
            string channelId = (string)context.GetVariable(VariableKey.ChannelId);
            string link = string.Empty;
            string imgTop = string.Empty;
            string imgleft = string.Empty;
            string imgRight = string.Empty;
            string imgBGR = string.Empty;
            switch (AppConfig.GetString("Idp:System:SubServiceName"))
            {
                case "SYS":
                    interServiceName = "Sys";
                    link = (await IdpCaching.GetObjectAsync("Idp_Configration", "Img_Url_Static_IB")).VarValue;
                    break;
                case "WEP":
                    interServiceName = "Wep";
                    channelId = "IB";
                    link = (await IdpCaching.GetObjectAsync("Idp_Configration", "Img_Url_Static_IB")).VarValue;
                    break;
                case "MOP":
                    interServiceName = "Mop";
                    channelId = "MB";
                    link = (await IdpCaching.GetObjectAsync("Idp_Configration", "Img_Url_Static_MB")).VarValue;
                    break;
            }
            imgTop = link + (await IdpCaching.GetObjectAsync("Sys_Variables", "IMG_TOP_TEMPLATE")).Value;
            imgleft = link + (await IdpCaching.GetObjectAsync("Sys_Variables", "IMG_LEFT_TEMPLATE")).Value;
            imgRight = link + (await IdpCaching.GetObjectAsync("Sys_Variables", "IMG_RIGHT_TEMPLATE")).Value;
            imgBGR = link + (await IdpCaching.GetObjectAsync("Sys_Variables", "IMG_BACKGROUND")).Value;
            string website = (await IdpCaching.GetObjectAsync("Sys_Variables", "WEBSITE")).Value;
            string hotline = (await IdpCaching.GetObjectAsync("Sys_Variables", "HOTLINE")).Value;
            sendInfo sendIf = new sendInfo();
            var lstSendInfo = new List<sendInfo>();
            string langid = (string)context.GetVariable(VariableKey.Lang);
            var infoSms = new sendInfo();
            var infoEmail = new sendInfo();
            //  JObject jobjEmail = new JObject();
            JObject jobjEmail = new JObject(
                          new JProperty("Fullname", user.FullName != null ? user.FullName : user.LastName),
                          new JProperty("Password", password),
                          new JProperty("PasswordTemp", string.IsNullOrEmpty(user.Birthday.Value.ToString()) ? "" : user.Birthday.Value.ToString("ddMMyyyy")),
                          new JProperty("ImgBGR", imgBGR),
                          new JProperty("ImgTop", imgTop),
                          new JProperty("Imgleft", imgleft),
                          new JProperty("ImgRight", imgRight),
                          new JProperty("Website", website),
                          new JProperty("Hotline", hotline)
                      );
            JObject jobjSms = new JObject(
                          new JProperty("Fullname", user.FullName != null ? user.FullName : user.LastName),
                          new JProperty("Password", password)
                      );
            if (autoGenPass == true)
            {
                switch (typesend.ToLower())
                {
                    case "sms":
                        if (string.IsNullOrEmpty(user.PhoneNo)) throw new IdpException("phonenotnull", "Phone no is not null");
                        infoSms.to = user.PhoneNo;
                        infoSms.tranCode = "ADDNEW_USER_TELLER";
                        infoSms.tempParam = "SENDER";
                        infoSms.lang = langid;
                        infoSms.inputParam = jobjSms;
                        infoSms.typeSend = "sms";
                        lstSendInfo.Add(infoSms);
                        break;
                    case "email":
                        if (string.IsNullOrEmpty(user.Email)) throw new IdpException("emailnotnull", "Email is not null");
                        infoEmail.to = user.Email;
                        infoEmail.tranCode = "ADDNEW_USER_TELLER";
                        infoEmail.tempParam = "SENDER";
                        infoEmail.lang = langid;
                        infoEmail.inputParam = jobjEmail;
                        infoEmail.typeSend = "email";
                        lstSendInfo.Add(infoEmail);
                        break;
                    case "all":
                        if (string.IsNullOrEmpty(user.PhoneNo)) throw new IdpException("phonenotnull", "Phone no is not null");
                        infoSms.to = user.PhoneNo;
                        infoSms.tranCode = "ADDNEW_USER_TELLER";
                        infoSms.tempParam = "SENDER";
                        infoSms.lang = langid;
                        infoSms.inputParam = jobjSms;
                        infoSms.typeSend = "sms";
                        lstSendInfo.Add(infoSms);
                        if (string.IsNullOrEmpty(user.Email)) throw new IdpException("emailnotnull", "Email is not null");
                        infoEmail.to = user.Email;
                        infoEmail.tranCode = "ADDNEW_USER_TELLER";
                        infoEmail.tempParam = "SENDER";
                        infoEmail.lang = langid;
                        infoEmail.inputParam = jobjEmail;
                        infoEmail.typeSend = "email";
                        lstSendInfo.Add(infoEmail);
                        break;
                    default:
                        throw new IdpException("typesendnotnull", "Type send password is not null", "");
                        break;
                }
            }
            await digitalContext.SaveChangesAsync();
            context.SetVariable(VariableKey.UserId, user.UserId);
            return lstSendInfo;
        }
        public async ValueTask<object> DeleteUser(dynamic userIds)
        {
            JArray idArr = new JArray();
            if (userIds is string)
            {
                idArr.Add(userIds);
            }
            else
            {
                idArr = userIds;
            }

            foreach (var item in idArr)
            {
                string id = (string)item;

                var record = await _context.SysUsers.Where(x => x.UserId == id).FirstOrDefaultAsync();
                var userInRole = await _context.SysUserInRoles.Where(x => x.UserId == id).ToListAsync();
                if (record != null)
                {
                    record.Status = "D";
                    record.DateModified = DateTime.Now;
                    record.UserModified = "userId";
                    var UserLogins = await _context.SysUserLogins
                        .Where(m => m.UserId == record.UserId).FirstOrDefaultAsync();
                    var UserInfo = await _context.SysLoginInfos.Where((m => m.LoginId == UserLogins.LoginId))
                                        .ToListAsync();
                    foreach (var user in UserInfo)
                    {
                        user.Status = "D";
                        await _context.SaveChangesAsync();
                    }

                }
                if (userInRole != null)
                {
                    foreach (var role in userInRole)
                    {
                        _context.SysUserInRoles.Remove(role);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        public async ValueTask<object> GetLoginHistory(string userId, string fromDate, string toDate, string serviceId, int pageIndex = 1, int pageSize = 0)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            if (!string.IsNullOrEmpty(fromDate))
            {
                startDate = DateTime.Parse(fromDate, null, System.Globalization.DateTimeStyles.RoundtripKind);
                startDate = DateTime.Parse((String.Format("{0:d}", startDate)), null, System.Globalization.DateTimeStyles.RoundtripKind);
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                endDate = DateTime.Parse(toDate, null, System.Globalization.DateTimeStyles.RoundtripKind);
                endDate = DateTime.Parse((String.Format("{0:d}", endDate)), null, System.Globalization.DateTimeStyles.RoundtripKind).AddDays(1);
            }
            int skip = ((pageIndex - 1) * pageSize);
            var lst = await _context.SysLoginHistories.Where(x =>
            x.UserId == (string.IsNullOrEmpty(userId) ? x.UserId : userId)
            && (x.LoginTime >= startDate)
            && (x.LoginTime < endDate)
            ).OrderByDescending(x => x.LoginTime).ToListAsync();

            var total = lst.Count();
            if (pageSize != 0)
            {
                var data = lst.Skip(skip).Take(pageSize);
                int pages = lst.Count() % pageSize >= 1 ? lst.Count() / pageSize + 1 : lst.Count() / pageSize;
                return new { data = data, pages = pages, total = total, pageIndex = pageIndex };
            }
            else
            {
                return new { data = lst, pages = 0, total = total, pageIndex = 1 };
            }
        }

    }
}