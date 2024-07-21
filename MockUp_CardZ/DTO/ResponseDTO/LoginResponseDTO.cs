using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.DTO.ResponseDTO
{
    public class LoginResponseDTO
    {
        public bool Result { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public bool TempToken { get; set; }
        public string RefreshToken { get; set; }
        public int refresh_token_expires_in { get; set; }
        public int expires_in { get; set; }
        public string TokenType { get; set; }
        public UserInfoDTO UserInfo { get; set; }
    }

    public class UserInfoDTO
    {
        public bool CheckSendMail { get; set; }
        public bool TempToken { get; set; }
        public string PhoneNo { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Image { get; set; }
        public bool NeedOTP { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool HavePin { get; set; }
        public bool Biometric { get; set; }
        public bool FirstLogin { get; set; }
        public bool IsNewDevice { get; set; }
        public int TimeCheckUserAction { get; set; }
        public int TimeRevokeToken { get; set; }
        public int TimeShowCountDown { get; set; }
    }
}
