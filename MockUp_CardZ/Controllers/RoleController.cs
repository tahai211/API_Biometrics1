using Microsoft.AspNetCore.Mvc;
using MockUp_CardZ.Infra.Attributes;
using MockUp_CardZ.Infra.Common.HttpCustom;
using MockUp_CardZ.Service.Portal;
using MockUp_CardZ.Service.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Controllers
{

    [ApiController]
    [Route("role")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RoleController(IRoleService roleService)
        {
            this._roleService = roleService;
        }

        [HttpPost("list")]
        [Check(checkRole: false, checkToken: false)]
        public async Task<IActionResult> GetListRoleManagement(string? roleName, string? usertype, string? serviceId, int pageIndex = 0, int pageSize = 15)
        {
            try
            {
                HttpResponseExtensions httpResponseExtensions = null;
                var lsiRole = await _roleService.GetListRoleManagement(roleName, usertype, serviceId, pageIndex, pageSize);
                httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", lsiRole);
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
        public async Task<IActionResult> GetDetailRoleManagement(int roleId)
        {
            try
            {
                HttpResponseExtensions httpResponseExtensions = null;
                var detailRole = await _roleService.GetDetailRoleManagement(roleId);
                httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", detailRole);
                return Ok(httpResponseExtensions);
            }
            catch (Exception ex)
            {
                var responeResult = new HttpBase(HttpStatusCode.BadRequest, ex.Message);
                return BadRequest(responeResult);
            }
        }
        [HttpPost("add")]
        [Check(checkRole: false, checkToken: false)]
        public async Task<IActionResult> AddRoleManagement(int? roleId, string? roleName, string? serviceId, string? desc, string? usertype, string? userId)
        {
            try
            {
                HttpResponseExtensions httpResponseExtensions = null;
                var addRole = await _roleService.UpdateRoleManagement(roleId, roleName, serviceId, desc, usertype, userId, "ADD");
                httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", addRole);
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
        public async Task<IActionResult> EditRoleManagement(int? roleId, string? roleName, string? serviceId, string? desc, string? usertype, string? userId)
        {
            try
            {
                HttpResponseExtensions httpResponseExtensions = null;
                var editRole = await _roleService.UpdateRoleManagement(roleId, roleName, serviceId, desc, usertype, userId, "EDIT");
                httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", editRole);
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
        public async Task<IActionResult> DeleteRoleManagement(dynamic role)
        {
            try
            {
                HttpResponseExtensions httpResponseExtensions = null;
                var deleteRole = await _roleService.DeleteRoleManagement(role);
                httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", deleteRole);
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
