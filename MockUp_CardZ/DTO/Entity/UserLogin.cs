using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.DTO.Entity
{
    public class UserLogin
    {
        [Key]
        public string UserId { get; set; }
        public string LoginId { get; set; }
        public string Status { get; set; }
    }
}
