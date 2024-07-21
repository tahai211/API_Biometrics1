using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.DTO.Entity
{
    public class SysUser
    {
        [Key]
        public string UserId { get; set; }
        public string UserType { get; set; }
        public string SourceId { get; set; }
        public string CompanyId { get; set; }
        public string Status { get; set; }
        public string BranchCode { get; set; }
        public int? PolicyId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public DateTime? Birthday { get; set; }
        public string Avatar { get; set; }
        public string AvatarType { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
