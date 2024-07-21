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

    }
}
