using MockUp_CardZ.DTO.Entity;
using MockUp_CardZ.DTO.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Service.User
{
    public interface IUserService
    {
        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        ValueTask<LoginResponseDTO> Authentication(string userName, string passWord, string serviceId);
        ValueTask<object> AddRefreshToken(RefreshTokenEntity refreshToken, string userId, string serviceId);
        ValueTask<object> UpdateRefreshToken(RefreshTokenEntity refreshToken, string userId, string serviceId);
        ValueTask<RefreshTokenEntity> GetRefreshToken(string token, string userId, string serviceId);
        ValueTask<SysUser> GetUserById( string userId);
        ValueTask<object> GetListUser(string? serviceId, string? status, string? userName, string? name, string? branch, string? email, string? phoneNo, string? companyId, int pageIndex = 1, int pageSize = 0);
        ValueTask<object> GetDetailUser(string userId);
        ValueTask<object> UpdateUser(string userName, string password, string firstName, string middleName, string lastName,
        string gender, string email, string phoneNo, string birthday, string address, string branch, int policy, dynamic rolesBO, dynamic rolesRPT, dynamic roleThirdparty, string companyId, string id,
         string userType, bool autoGenPass, string typesend, string actionType, string userNameCbs, string employeeId, string serviceId, string sourceId = "", string tokencbs = "");
        ValueTask<object> DeleteUser(dynamic userIds);
        ValueTask<object> GetLoginHistory(string userId, string fromDate, string toDate, string serviceId, int pageIndex = 1, int pageSize = 0);

    }
}
