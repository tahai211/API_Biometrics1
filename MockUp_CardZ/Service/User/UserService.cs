using Microsoft.EntityFrameworkCore;
using MockUp_CardZ.Data;
using MockUp_CardZ.DTO.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<DTO.Entity.User> Authentication(string userName, string passWord)
        {
            // Logic đăng nhập
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == userName && u.Password == passWord);

            return new DTO.Entity.User();
        }

        //public async Task<UserEntity> SaveUser(string stringXml, BasicParamType param)
        //{
        //    // Logic cập nhật user
        //    // Deserialize stringXml thành UserEntity
        //    var user = new UserEntity();
        //    // Thêm hoặc cập nhật user
        //    _context.Users.Update(user);
        //    await _context.SaveChangesAsync();

        //    return user;
        //}

        //public async Task<UserEntity> GetUserById(Guid userId, BasicParamType param)
        //{
        //    return await _context.Users.FindAsync(userId);
        //}

        //public async Task<int> Delete(Guid userId, BasicParamType param)
        //{
        //    var user = await _context.Users.FindAsync(userId);
        //    if (user == null) return 0;

        //    _context.Users.Remove(user);
        //    return await _context.SaveChangesAsync();
        //}

        //public async Task<List<UserEntity>> GetListUser(BasicParamType param)
        //{
        //    return await _context.Users.ToListAsync();
        //}
    }
}
