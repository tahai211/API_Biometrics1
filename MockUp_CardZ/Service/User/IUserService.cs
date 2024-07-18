using MockUp_CardZ.DTO.Entity;
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
        Task<DTO.Entity.User> Authentication(string userName, string passWord);
        /// <summary>
        /// cập nhật user
        /// </summary>
        /// <param name = "stringXml" ></ param >
        /// < param name="userId"></param>
        /// <param name = "dbName" ></ param >
        /// < returns ></ returns >
        //Task<UserEntity> SaveUser(string stringXml, BasicParamType param);
        /// <summary>
        /// Lấy user theo Id
        /// </summary>
        /// <param name = "userId" ></ param >
        /// < param name="dbName"></param>
        /// <returns></returns>
        //Task<UserEntity> GetUserById(Guid userId, BasicParamType param);
        /// <summary>
        /// Xóa người dùng
        /// </summary>
        /// <param name = "userId" ></ param >
        /// < param name="dbName"></param>
        /// <returns></returns>
        //Task<int> Delete(Guid userId, BasicParamType param);
        /// <summary>
        /// Lấy danh sách user
        /// </summary>
        /// <param name = "param" ></ param >
        /// < returns ></ returns >
        //Task < List < UserEntity >> GetListUser(BasicParamType param);
    }
}
