using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<LoginInfo> LoginInfos { get; set; }
        public DbSet<Password> Passwords { get; set; }
        public DbSet<PasswordHis> PasswordHis { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserInRole> UserInRoles { get; set; }
        public DbSet<UserLogin> UserLogins { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }

    public class LoginInfo
    {
        [Key]
        public string LoginId { get; set; }
        public string ServiceId { get; set; }
        public string LoginName { get; set; }
        public string LoginType { get; set; }
        public string AuthenType { get; set; }
        public int? FailNumber { get; set; }
        public bool? NewLogin { get; set; }
        public int? PolicyId { get; set; }
        public DateTime? DateExpired { get; set; }
        public string Status { get; set; }
        public string DeviceId { get; set; }
        public string BiometricToken { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? LastLoginFail { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DateModified { get; set; }
    }

    public class Password
    {
        [Key]
        public string LoginId { get; set; }
        public string Type { get; set; }
        public string PasswordValue { get; set; }
        public string PasswordTemp { get; set; }
        public string UserModified { get; set; }
        public DateTime? DateModified { get; set; }
        public string ServiceId { get; set; }
        public string ActionType { get; set; }
    }

    public class PasswordHis
    {
        [Key]
        public string LoginId { get; set; }
        public string Type { get; set; }
        public string PasswordValue { get; set; }
        public string UserModified { get; set; }
        public DateTime? DateModified { get; set; }
        public int ChangeTimeNumber { get; set; }
        public string ServiceId { get; set; }
        public string ActionType { get; set; }
        public string Option { get; set; }
    }

    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public string ServiceId { get; set; }
        public string CtmType { get; set; }
        public string UserType { get; set; }
        public string RoleType { get; set; }
        public bool? Active { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
    }

    public class User
    {
        [Key]
        public string UserId { get; set; }
        public string UserType { get; set; }
        public string SourceId { get; set; }
        public string CompanyId { get; set; }
        public string Status { get; set; }
        public string BranchCode { get; set; }
        public int? PolicyId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public DateTime? Birthday { get; set; }
        public string Avatar { get; set; }
        public string AvatarType { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DateModified { get; set; }
    }

    public class UserInRole
    {
        [Key]
        public string UserId { get; set; }
        public int RoleId { get; set; }
    }

    public class UserLogin
    {
        [Key]
        public string UserId { get; set; }
        public string LoginId { get; set; }
        public string Status { get; set; }
    }
}
