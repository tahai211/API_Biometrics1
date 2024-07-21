using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.DTO.Entity
{
    [Table("Sys_Service")]
    public class SysService
    {
        [Key]
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public bool? Status { get; set; }
        public bool CustomerChannel { get; set; }
        public string CompanyId { get; set; }
        public int CheckUserAction { get; set; }
        public int TimeRevokeToken { get; set; }
        public int TimeShowCountDown { get; set; }
    }
}
