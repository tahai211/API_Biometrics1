using Microsoft.AspNetCore.Mvc;
using MockUp_CardZ.Service.Biomertric;
using MockUp_CardZ.Service.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Controllers
{
    [Route("biometric")]
    [ApiController]
    public class BiometricController : ControllerBase
    {
        private readonly IBiomertricService _biomertricService;

        public BiometricController(IBiomertricService biomertricService)
        {
            _biomertricService = biomertricService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterBiomertric(string userName, string passWord, string dbName)
        {
            //var user = await _userService.Authentication(userName, passWord);
            //if (user == null) return Unauthorized();

            return Ok();
        }
        [HttpPost("positively")]
        public async Task<IActionResult> PositivelyBiomertric(string userName, string passWord, string dbName)
        {
            //var user = await _userService.Authentication(userName, passWord);
            //if (user == null) return Unauthorized();

            return Ok();
        }
    }
}
