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
        public DbSet<SysLoginInfo> SysLoginInfos { get; set; }
        public DbSet<SysPassword> SysPasswords { get; set; }
        public DbSet<SysPasswordHis> SysPasswordHis { get; set; }
        public DbSet<SysRole> SysRoles { get; set; }
        public DbSet<SysUser> SysUsers { get; set; }
        public DbSet<SysUserInRole> SysUserInRoles { get; set; }
        public DbSet<SysUserLogin> SysUserLogins { get; set; }
        public DbSet<SysPolicy> SysPolicies { get; set; }
        public DbSet<SysService> SysService { get; set; }
        public DbSet<SysLoginHistory> SysLoginHistories { get; set; }
        public DbSet<SysUserAccessToken> SysUserAccessTokens { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SysPolicy>()
                .HasKey(p => new { p.PolicyId, p.ServiceId });

            modelBuilder.Entity<SysService>()
                .HasKey(s => s.ServiceId);

            modelBuilder.Entity<SysLoginInfo>()
                .HasKey(l => new { l.LoginId, l.ServiceId });

            modelBuilder.Entity<SysPassword>()
                .HasKey(p => new { p.LoginId, p.Type, p.ServiceId });

            modelBuilder.Entity<SysPasswordHis>()
                .HasKey(ph => new { ph.LoginId, ph.ChangeTimeNumber, ph.ServiceId });

            modelBuilder.Entity<SysRole>()
                .HasKey(r => r.RoleId);

            modelBuilder.Entity<SysUser>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<SysUserInRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<SysUserLogin>()
                .HasKey(ul => ul.UserId);

            modelBuilder.Entity<SysLoginHistory>()
                .HasKey(lh => lh.Id);

            modelBuilder.Entity<SysUserAccessToken>()
                .HasKey(lh => lh.Id);

            //// Thiết lập các thuộc tính khác nếu cần
            //modelBuilder.Entity<Password>()
            //    .Property(p => p.Type)
            //    .HasDefaultValue("PASSWORD");

            //modelBuilder.Entity<Role>()
            //    .Property(r => r.DateCreated)
            //    .HasDefaultValueSql("GETDATE()");

            //modelBuilder.Entity<User>()
            //    .Property(u => u.SourceId)
            //    .HasDefaultValue("INTERNAL");
        }
    }

}
