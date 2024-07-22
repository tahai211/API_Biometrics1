using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.DTO.Entity
{
    [Table("Sys_UserAccessToken")]
    public class SysUserAccessToken
    {
        public decimal Id { get; set; } // ID duy nhất của token
        public string? Data { get; set; } // Dữ liệu bổ sung liên quan đến token
        public string? UserId { get; set; } // ID của người dùng
        public string? Token { get; set; } // Chuỗi token truy cập
        public string? Service { get; set; } // Tên hoặc mã của dịch vụ liên quan đến token
        public DateTime? ExpiredDate { get; set; } // Ngày hết hạn của token
        public string? Status { get; set; } // Trạng thái của token
    }
}
