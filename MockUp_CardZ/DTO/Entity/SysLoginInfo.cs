using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.DTO.Entity
{
    [Table("Sys_LoginInfo")]
    public class SysLoginInfo
    {
        [Key]
        public string? LoginId { get; set; }
        [Key]
        public string? ServiceId { get; set; }
        public string? LoginName { get; set; }
        public string? LoginType { get; set; }
        public string? AuthenType { get; set; }
        public int? FailNumber { get; set; }
        public bool? NewLogin { get; set; }
        public int? PolicyId { get; set; }
        public DateTime? DateExpired { get; set; }
        public string? Status { get; set; }
        public string? DeviceId { get; set; }
        public string? BiometricToken { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? LastLoginFail { get; set; }
        public string? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public string? UserModified { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
