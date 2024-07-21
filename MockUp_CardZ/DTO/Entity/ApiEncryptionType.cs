using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.DTO.Entity
{
    public class ApiEncryptionType
    {
        [Key]
        public string EncryptId { get; set; }
        public string EncryptName { get; set; }
        public string ParamData { get; set; }
    }
}
