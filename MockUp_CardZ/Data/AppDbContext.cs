using Microsoft.EntityFrameworkCore;
using MockUp_CardZ.DTO.Entity;
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

}
