using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.DTO.Entity
{
    [Table("Sys_Password")]
    public class SysPassword
    {
        [Key]
        public string LoginId { get; set; }
        public string Type { get; set; }
        public string Password { get; set; }
        public string PasswordTemp { get; set; }
        public string UserModified { get; set; }
        public DateTime? DateModified { get; set; }
        public string ServiceId { get; set; }
        public string ActionType { get; set; }
    }
}
