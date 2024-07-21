using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MockUp_CardZ.Infra.Attributes;

namespace MockUp_CardZ.Infra.Middleware
{
    public class RoleCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public RoleCheckMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var checkRole = _configuration.GetValue<bool>("CheckSettings:CheckRole");
            var checkToken = _configuration.GetValue<bool>("CheckSettings:CheckToken");

            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var attributes = endpoint.Metadata.OfType<CheckAttribute>().FirstOrDefault();

                if (attributes != null)
                {
                    checkRole = attributes.CheckRole;
                    checkToken = attributes.CheckToken;
                }
            }

            if (checkToken)
            {
                var user = context.User;
                if (user.Identity != null && user.Identity.IsAuthenticated)
                {
                    if (checkRole && !user.IsInRole("Admin"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("Forbidden: User does not have the required role.");
                        return;
                    }
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: User is not authenticated.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
