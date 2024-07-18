using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Models.User
{
    public class JWT
    {
        public string PassKey { get; set; }
        public string JwtKey { get; set; }
        public string JwtIssuer { get; set; }
        public string JwtAudience { get; set; }
        public double JwtExpireTokenMinutes { get; set; }
        public double JwtExpireRefreshTokenMinutes { get; set; }
    }
}
