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
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(string userName, string passWord, string dbName)
        {
            var user = await _userService.Authentication(userName, passWord);
            if (user == null) return Unauthorized();

            return Ok(user);
        }
    }
}
