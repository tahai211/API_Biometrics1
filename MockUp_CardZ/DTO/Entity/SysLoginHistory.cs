using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.DTO.Entity
{
    [Table("Sys_LoginHistory")]
    public class SysLoginHistory
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? UserId { get; set; }

        [Required]
        public DateTime? LoginTime { get; set; }

        [Required]
        [StringLength(50)]
        public string? ServiceId { get; set; }

        [StringLength(450)]
        public string? DeviceInfo { get; set; }

        [StringLength(1000)]
        public string? IpAddress { get; set; }

        [StringLength(1000)]
        public string? Location { get; set; }
    }
}
