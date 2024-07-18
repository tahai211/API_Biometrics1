using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MockUp_CardZ.DTO.Entity;
using MockUp_CardZ.Models.User;
using MockUp_CardZ.Service.User;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
        ///        "password":"123456",
        ///        "domain":"original.possoft.asia"
        ///     }
        ///
        /// </remarks>
        /// <param name="model"></param>
        /// <returns>Tài khoản người dùng</returns>
        /// <response code="200">Đăng nhập thành công</response>
        /// <response code="404">Không tìm thấy tài khoản</response>
        /// <response code="400">Đăng nhập thất bại</response> 
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                model.Password = Security.EncryptKey(model.Password);
                //var customerResult = await _customerService.GetCustomerByDomain(model.Domain);
                var authenticationModel = new AuthenticationModel();
                HttpResponseExtensions httpResponseExtensions = null;
                var user = await _userService.Authentication(model.Username, model.Password);
                if (user.IsActivated == true)
                {
                    //RefreshTokenEntity refreshToken = GenerateRefreshToken();
                    //refreshToken.UserId = user.UserId ?? 0;
                    //var json = JsonConvert.SerializeObject(refreshToken);
                    //XmlNode xmlNode = JsonConvert.DeserializeXmlNode(json, "XMLData");
                    //await _userService.AddRefreshToken(xmlNode.InnerXml, user.UserId, model.Domain);
                    user.DbName = customerResult.DbName;
                    JwtSecurityToken jwtSecurityToken = CreateJwtToken(user);
                    authenticationModel.Id = user.Id;
                    authenticationModel.UserName = user.UserName;
                    authenticationModel.Name = user.Name;
                    //authenticationModel.Avatar = user.ImageLink;
                    authenticationModel.RoleId = user.RoleId;
                    //authenticationModel.PotentialId = user.PotentialId;
                    authenticationModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                    //authenticationModel.RefreshToken = refreshToken.RefreshToken;
                    // var token = new JwtSecurityTokenHandler().ReadJwtToken(authenticationModel.Token);
                    // var jti = token.Claims.FirstOrDefault(claim => claim.Type == "jti")?.Value;
                    //if (!String.IsNullOrEmpty(model.DeviceToken))
                    //{
                    //    await _notificationConnectionService.InsertDevice(model.DeviceToken, model.DeviceType, user.Id.Value, model.Domain);
                    //}
                    httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", authenticationModel);
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

        private JwtSecurityToken CreateJwtToken(User user)
        {
            //var isRedisServer = bool.Parse(Configuration["AppSettings:IsRedisServer"]);
            var roleClaims = new List<Claim>();
            roleClaims.Add(new Claim("Roles", user.RoleId.ToString()));
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, String.IsNullOrEmpty(user.Email) ? "": user.Email),
                new Claim(JwtRegisteredClaimNames.FamilyName, String.IsNullOrEmpty(user.DbName) ? "": user.DbName),
                new Claim("Name", user.Name ?? user.Name),
                new Claim("DbName", String.IsNullOrEmpty(user.DbName) ? "": user.DbName),
                new Claim("Uid", user.Id.ToString())
            }
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.JwtKey ?? "JwtKey"));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.JwtIssuer,
                audience: _jwt.JwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.JwtExpireTokenMinutes),
                signingCredentials: signingCredentials);
            //if (isRedisServer)
            //{
            //var getCacheLogin = await _distributedCache.GetStringAsync($"{Constant.ProjectName}_{user.DBName}_{user.Id}".ToLower());
            //if (String.IsNullOrEmpty(getCacheLogin))
            //{
            //    await _distributedCache.SetStringAsync($"{Constant.ProjectName}_{user.DBName}_{user.Id}".ToLower(), user.Id.ToString());
            //}
            //}
            return jwtSecurityToken;
        }

    }
}
