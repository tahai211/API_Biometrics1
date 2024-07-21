using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.DTO.Entity
{
    [Table("Sys_Role")]
    public class SysRole
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public string ServiceId { get; set; }
        public string UserType { get; set; }
        public string RoleType { get; set; }
        public bool? Active { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
