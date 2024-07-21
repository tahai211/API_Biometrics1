using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.DTO.Entity
{
    public class SysUserInRole
    {
        [Key]
        public string UserId { get; set; }
        public int RoleId { get; set; }
    }
}
