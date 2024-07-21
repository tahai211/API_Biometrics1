using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.DTO.Entity
{
    public class SysPasswordHis
    {
        [Key]
        public string LoginId { get; set; }
        public string Type { get; set; }
        public string PasswordValue { get; set; }
        public string UserModified { get; set; }
        public DateTime? DateModified { get; set; }
        public int ChangeTimeNumber { get; set; }
        public string ServiceId { get; set; }
        public string ActionType { get; set; }
        public string Option { get; set; }
    }
}
