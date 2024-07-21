using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.DTO.Entity
{
    [Table("Sys_UserLogin")]
    public class SysUserLogin
    {
        [Key]
        public string UserId { get; set; }
        public string LoginId { get; set; }
        public string Status { get; set; }
    }
}
