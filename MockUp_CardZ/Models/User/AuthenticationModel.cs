using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Models.User
{
    public class AuthenticationModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public Guid? RoleId { get; set; }
        //public long? PotentialId { get; set; }
        //public string Avatar { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
