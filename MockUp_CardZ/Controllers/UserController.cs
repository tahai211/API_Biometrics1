using Microsoft.AspNetCore.Mvc;
using MockUp_CardZ.Data;
using MockUp_CardZ.Infra.Attributes;
using MockUp_CardZ.Infra.Common.HttpCustom;
using MockUp_CardZ.Service.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Controllers
{
    [ApiController]
    [Route("user")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("list")]
        [Check(checkRole: false, checkToken: false)]
        public async Task<IActionResult> GetListUserManagement(string? serviceId, string? status, string? userName, string? name, string? branch, string? email, string? phoneNo, string? companyId, int pageIndex = 1, int pageSize = 0)
        {
            try
            {
                HttpResponseExtensions httpResponseExtensions = null;
                var listUser = await _userService.GetListUser(serviceId,status,userName,  name, branch,email,phoneNo, companyId,pageIndex, pageSize);
                httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", listUser);
                return Ok(httpResponseExtensions);
            }
            catch (Exception ex)
            {
                var responeResult = new HttpBase(HttpStatusCode.BadRequest, ex.Message);
                return BadRequest(responeResult);
            }
        }
        [HttpPost("view")]
        [Check(checkRole: false, checkToken: false)]
        public async Task<IActionResult> GetDetailUserManagement(string userId)
        {
            try
            {
                HttpResponseExtensions httpResponseExtensions = null;
                var userDetail = await _userService.GetDetailUser( userId);
                httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", userDetail);
                return Ok(httpResponseExtensions);
            }
            catch (Exception ex)
            {
                var responeResult = new HttpBase(HttpStatusCode.BadRequest, ex.Message);
                return BadRequest(responeResult);
            }
        }
        [HttpPost("edit")]
        [Check(checkRole: false, checkToken: false)]
        public async Task<IActionResult> EditUserManagement(string userName, string password, string firstName, string middleName, string lastName,
        string gender, string email, string phoneNo, string birthday, string address, string branch, int policy, dynamic rolesBO, dynamic rolesRPT, dynamic roleThirdparty, string companyId, string id,
         string userType, bool autoGenPass, string typesend, string userNameCbs, string employeeId, string serviceId, string sourceId = "", string tokencbs = "")
        {
            try
            {
                HttpResponseExtensions httpResponseExtensions = null;
                var editUser = await _userService.UpdateUser(userName, password, firstName, middleName, lastName,
         gender, email, phoneNo, birthday, address, branch, policy, rolesBO, rolesRPT, roleThirdparty, companyId, id,
          userType, autoGenPass, typesend, "EDIT", userNameCbs, employeeId, serviceId, sourceId, tokencbs);
                httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", editUser);
                return Ok(httpResponseExtensions);
            }
            catch (Exception ex)
            {
                var responeResult = new HttpBase(HttpStatusCode.BadRequest, ex.Message);
                return BadRequest(responeResult);
            }
        }
        [HttpPost("edit")]
        [Check(checkRole: false, checkToken: false)]
        public async Task<IActionResult> AddUserManagement(string userName, string password, string firstName, string middleName, string lastName,
        string gender, string email, string phoneNo, string birthday, string address, string branch, int policy, dynamic rolesBO, dynamic rolesRPT, dynamic roleThirdparty, string companyId, string id,
         string userType, bool autoGenPass, string typesend, string userNameCbs, string employeeId, string serviceId, string sourceId = "", string tokencbs = "")
        {
            try
            {
                HttpResponseExtensions httpResponseExtensions = null;
                var addUser = await _userService.UpdateUser(userName, password, firstName, middleName, lastName,
         gender, email, phoneNo, birthday, address, branch, policy, rolesBO, rolesRPT, roleThirdparty, companyId, id,
          userType, autoGenPass, typesend, "ADD", userNameCbs, employeeId, serviceId, sourceId, tokencbs);
                httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", addUser);
                return Ok(httpResponseExtensions);
            }
            catch (Exception ex)
            {
                var responeResult = new HttpBase(HttpStatusCode.BadRequest, ex.Message);
                return BadRequest(responeResult);
            }
        }
        [HttpPost("Delete")]
        [Check(checkRole: false, checkToken: false)]
        public async Task<IActionResult> DeleteUserManagement(dynamic userIds)
        {
            try
            {
                HttpResponseExtensions httpResponseExtensions = null;
                var deleteUser = await _userService.DeleteUser(userIds);
                httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", deleteUser);
                return Ok(httpResponseExtensions);
            }
            catch (Exception ex)
            {
                var responeResult = new HttpBase(HttpStatusCode.BadRequest, ex.Message);
                return BadRequest(responeResult);
            }
        }
    }
}
