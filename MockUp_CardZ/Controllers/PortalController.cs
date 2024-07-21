using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MockUp_CardZ.Infra.Attributes;
using MockUp_CardZ.Infra.Common.HttpCustom;
using MockUp_CardZ.Models.User;
using MockUp_CardZ.Service.Portal;
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
    [Route("portal")]
    public class PortalController : ControllerBase
    {
        private readonly IPortalService _portalService;
        public PortalController(IPortalService portalService)
        {
            this._portalService = portalService;
        }

        [HttpPost("list")]
        [Check(checkRole: false, checkToken: false)]
        public async Task<IActionResult> GetListPortalManagement(string? portalName, string? portaiId, string? status, int pageSize = 0, int pageIndex = 1)
        {
            try
            {
                HttpResponseExtensions httpResponseExtensions = null;
                var lsiPortal = await _portalService.GetListPortalManagement(portalName, portaiId, status, 0, 1);
                httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", lsiPortal);
                return Ok(httpResponseExtensions);
            }
            catch (Exception ex) {
                var responeResult = new HttpBase(HttpStatusCode.BadRequest, ex.Message);
                return BadRequest(responeResult);
            }
        }
        [HttpPost("view")]
        [Check(checkRole: false, checkToken: false)]
        public async Task<IActionResult> GetDetailPortalManagement(string serviceId)
        {
            try
            {
                HttpResponseExtensions httpResponseExtensions = null;
                var portalInfo = await _portalService.GetDetailPortalManagement(serviceId);
                httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", portalInfo);
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
        public async Task<IActionResult> AddPortalManagement(string? serviceId, string? serviceName, string? status, string? customerChannel, int checkUserAction, int timeRevokeToken, int timeShowCountDown)
        {
            try
            {
                HttpResponseExtensions httpResponseExtensions = null;
                var portalAdd = await _portalService.UpdatePortalManagement(serviceId, serviceName, status, customerChannel, checkUserAction, timeRevokeToken, timeShowCountDown, "ADD");
                httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", portalAdd);
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
        public async Task<IActionResult> EditPortalManagement(string? serviceId, string? serviceName, string? status, string? customerChannel, int checkUserAction, int timeRevokeToken, int timeShowCountDown)
        {
            try
            {
                HttpResponseExtensions httpResponseExtensions = null;
                var portalEdit = await _portalService.UpdatePortalManagement(serviceId, serviceName, status, customerChannel, checkUserAction, timeRevokeToken, timeShowCountDown, "EDIT");
                httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", portalEdit);
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
        public async Task<IActionResult> DeletePortalManagement(dynamic serviceId)
        {
            try
            {
                HttpResponseExtensions httpResponseExtensions = null;
                var portalDelete = await _portalService.DeletePortalManagement(serviceId);
                httpResponseExtensions = new HttpResponseExtensions(HttpStatusCode.OK, "Thành công", portalDelete);
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
