using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.DTO.Entity
{
    [Table("Sys_Policy")]
    public class SysPolicy
    {
        [Key, Column(Order = 0)]
        public int PolicyId { get; set; }

        [Key, Column(Order = 1)]
        public string ServiceId { get; set; }

        public string Description { get; set; }
        public DateTime? EfFrom { get; set; }
        public DateTime? EfTo { get; set; }
        public int? PwAge { get; set; }
        public int? PwMinLength { get; set; }
        public int? PwMaxLength { get; set; }
        public bool? PwComplex { get; set; }
        public bool? PwLowerCase { get; set; }
        public bool? PwUpperCase { get; set; }
        public bool? PwSymbols { get; set; }
        public bool? PwNumber { get; set; }
        public TimeSpan AccessTimeFrom { get; set; }
        public TimeSpan AccessTimeTo { get; set; }
        public int? FailAccessNumber { get; set; }
        public int? ResetPwTime { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DateModified { get; set; }
        public int? PwHis { get; set; }
    }
}
