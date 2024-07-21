using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MockUp_CardZ.DTO.Entity;
using MockUp_CardZ.Infra.Common;
using MockUp_CardZ.Infra.Common.HttpCustom;
using MockUp_CardZ.Models.User;
using MockUp_CardZ.Service.User;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using MockUp_CardZ.DTO.ResponseDTO;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using MockUp_CardZ.Infra.Constans;
using System.Xml;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using MockUp_CardZ.Infra.Attributes;


namespace MockUp_CardZ.Controllers
{
    [Route("api/authen")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly IUserService _userService;
        //private readonly ICustomerService _customerService;
        private readonly JWT _jwt;

        public AuthenticationController(IUserService userService,  IOptions<JWT> jwt)
        {
            this._userService = userService;
            //this._customerService = customerService;
            this._jwt = jwt.Value;
        }
        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "username": "admin",
        ///        "password":"123456"
        ///     }
        ///
        /// </remarks>
        /// <param name="model"></param>
        /// <returns>Tài khoản người dùng</returns>
        /// <response code="200">Đăng nhập thành công</response>
        /// <response code="404">Không tìm thấy tài khoản</response>
        /// <response code="400">Đăng nhập thất bại</response> 
        [HttpPost("login")]
        [Check(checkRole: false, checkToken: false)]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                model.Password = Security.EncryptKey(model.Password);
                HttpResponseExtensions httpResponseExtensions = null;
                var user = await _userService.Authentication(model.Username, model.Password, model.ServiceId);
                if (user != null && user.Result)
                {
                    RefreshTokenEntity refreshToken = GenerateRefreshToken();
                    refreshToken.UserId = user.UserId;
                    //var json = JsonConvert.SerializeObject(refreshToken);
                    //XmlNode xmlNode = JsonConvert.DeserializeXmlNode(json, "XMLData");
                    await _userService.AddRefreshToken(refreshToken, user.UserId , model.ServiceId);
                    JwtSecurityToken jwtSecurityToken = CreateJwtToken(user);
                    user.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                    user.RefreshToken = refreshToken.RefreshToken;
                    user.TokenType = "Bearer";
                    user.expires_in = StaticVariable.ExpiresToken * 3600;
                    user.refresh_token_expires_in = StaticVariable.ExpiresToken * 24 * 3600;
                    httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", user);
                    return Ok(httpResponseExtensions);
                }
                else
                {
                    httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.NotFound, "Thất bại", "Tài khoản đã bị khóa");
                    return NotFound(httpResponseExtensions);
                }
            }
            catch (Exception ex)
            {
                var responeResult = new HttpBase(HttpStatusCode.BadRequest, ex.Message);
                return BadRequest(responeResult);
            }
        }

        [HttpPost("refreshtoken")]
        [Check(checkRole: false, checkToken: false)]
        public async Task<ActionResult<LoginResponseDTO>> RefreshToken([FromBody] RefreshRequestModel refreshRequest)
        {
            var userInfo = await GetUserFromAccessToken(refreshRequest.AccessToken);

            if (userInfo != null && userInfo.Status == "A" && await ValidateRefreshToken(userInfo, refreshRequest.RefreshToken, refreshRequest.ServiceId))
            {
                var refreshToken1 = new RefreshTokenEntity() { ExpiryDate = DateTime.Now, RefreshToken = refreshRequest.RefreshToken };
                //var json1 = JsonConvert.SerializeObject(refreshToken1);
                //XmlNode xmlNode1 = JsonConvert.DeserializeXmlNode(json1, "XMLData");
                await _userService.UpdateRefreshToken(refreshToken1, userInfo.UserId, refreshRequest.ServiceId);

                RefreshTokenEntity refreshToken = GenerateRefreshToken();
                refreshToken.UserId = userInfo.UserId ?? "";
                //var json = JsonConvert.SerializeObject(refreshToken);
                //XmlNode xmlNode = JsonConvert.DeserializeXmlNode(json, "XMLData");
                await _userService.AddRefreshToken(refreshToken, userInfo.UserId, refreshRequest.ServiceId);

                LoginResponseDTO user = new LoginResponseDTO();
                JwtSecurityToken jwtSecurityToken = CreateJwtToken(user);
                user.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                user.RefreshToken = refreshToken.RefreshToken;
                user.TokenType = "Bearer";
                user.expires_in = StaticVariable.ExpiresToken * 3600;
                user.refresh_token_expires_in = StaticVariable.ExpiresToken * 24 * 3600;
                var httpResponse = new HttpResponseExtensions(HttpStatusCode.OK, "Success", user);
                return Ok(httpResponse);
            }

            HttpResponseExtensions httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.NotFound, "Failed", new {});
            return NotFound(httpResponseExtensions);
        }
        [HttpPost("revoketoken")]
        [Check(checkRole: false, checkToken: true)]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest model)
        {
            HttpBase responeResult = null;
            var token = model.Token;
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    responeResult = new HttpBase(HttpStatusCode.BadRequest, "Message.Token.NotFound");
                    return NotFound(responeResult);
                }
                else
                {
                    var user = await GetUserFromAccessToken(model.AccessToken);

                    if (user != null)
                    {
                        var refreshToken = await _userService.GetRefreshToken(model.Token, user.UserId, model.ServiceId);
                        if (refreshToken == null)
                        {
                            responeResult = new HttpBase(HttpStatusCode.BadRequest, "Message.Token.NotFound");
                            return NotFound(responeResult);
                        }
                        if (refreshToken.ExpiryDate < DateTime.UtcNow)
                        {
                            responeResult = new HttpBase(HttpStatusCode.NotFound, "Message.Token.NotFound");
                            return BadRequest(responeResult);
                        }
                        else
                        {
                            refreshToken = new RefreshTokenEntity() { ExpiryDate = DateTime.Now, RefreshToken = refreshToken.RefreshToken };
                            //var json = JsonConvert.SerializeObject(refreshToken);
                            //XmlNode xmlNode = JsonConvert.DeserializeXmlNode(json, "XMLData");
                            await _userService.UpdateRefreshToken(refreshToken, user.UserId, model.ServiceId);
                            responeResult = new HttpBase(HttpStatusCode.OK, "Message.Successful");
                            return Ok(responeResult);
                        }
                    }
                    else
                    {
                        responeResult = new HttpBase(HttpStatusCode.NotFound, "Message.Account.NotFound");
                        return NotFound(responeResult);
                    }
                }
            }
            catch (Exception ex)
            {
                responeResult = new HttpBase(HttpStatusCode.BadRequest, ex.Message);
                return BadRequest(responeResult);
            }
        }
    

    private JwtSecurityToken CreateJwtToken(LoginResponseDTO user)
        {
            var roleClaims = new List<Claim>();
            var claims = new[]
            {
                 new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.UserInfo.FullName),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, user.UserInfo.Email),
                new Claim("UserId", user.UserId)
            };

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.JwtKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.JwtIssuer,
                audience: _jwt.JwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(StaticVariable.ExpiresToken),
                signingCredentials: signingCredentials);
            return jwtSecurityToken;
        }
        private RefreshTokenEntity GenerateRefreshToken()
        {
            RefreshTokenEntity refreshToken = new RefreshTokenEntity();

            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                refreshToken.RefreshToken = Convert.ToBase64String(randomNumber);
            }
            refreshToken.ExpiryDate = DateTime.UtcNow.AddDays(StaticVariable.ExpiresTokenRefresh);
            return refreshToken;
        }
        private async Task<bool> ValidateRefreshToken(SysUser user, string refreshToken, string serviceId)
        {
            var UserId = user.UserId ?? "";
            RefreshTokenEntity refreshTokenUser = await _userService.GetRefreshToken(refreshToken, UserId, serviceId);
            if (refreshTokenUser != null
                && refreshTokenUser.ExpiryDate > DateTime.UtcNow)
            {
                return true;
            }

            return false;
        }
        private string GetName(string token)
        {
            string secret = _jwt.JwtKey;
            var key = Encoding.ASCII.GetBytes(secret);
            var handler = new JwtSecurityTokenHandler();
            var validations = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
            var claims = handler.ValidateToken(token, validations, out var tokenSecure);
            var email = claims.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value;
            return email;
        }

        private string DecodeJwtTokenToLoginName(string token)
        {
            JwtSecurityToken jwtToken = new JwtSecurityTokenHandler()
                .ReadJwtToken(token);
            string userName = jwtToken.Claims.First(claim => claim.Type == "email").Value;

            return userName;

        }
        private void SetRefreshTokenInCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(10),
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
        private async Task<SysUser> GetUserFromAccessToken(string accessToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwt.JwtKey);
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(accessToken);

                if (jwtSecurityToken != null && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    var userId = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
                    var jti = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == "Jti")?.Value;
                    var email = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
              
                    var user = await _userService.GetUserById(userId);
                    return user;
                }
            }
            catch (Exception x)
            {
                return new SysUser();
            }

            return new SysUser();
        }

    }
}
