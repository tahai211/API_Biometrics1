using Microsoft.AspNetCore.Mvc;
using MockUp_CardZ.Data;
using MockUp_CardZ.Service.User;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<IActionResult> GetListUserManagement(string userName, string passWord, string dbName)
        {
            //var user = await _userService.Authentication(userName, passWord);
            //if (user == null) return Unauthorized();

            return Ok();
        }
        [HttpPost("view")]
        public async Task<IActionResult> GetDetailUserManagement(string userName, string passWord, string dbName)
        {
            //var user = await _userService.Authentication(userName, passWord);
            //if (user == null) return Unauthorized();

            return Ok();
        }
        [HttpPost("edit")]
        public async Task<IActionResult> EditUserManagement(string userName, string passWord, string dbName)
        {
            //var user = await _userService.Authentication(userName, passWord);
            //if (user == null) return Unauthorized();

            return Ok();
        }
        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteUserManagement(string userName, string passWord, string dbName)
        {
            //var user = await _userService.Authentication(userName, passWord);
            //if (user == null) return Unauthorized();

            return Ok();
        }
    }
}
