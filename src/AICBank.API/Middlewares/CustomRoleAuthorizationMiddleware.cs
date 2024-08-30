using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AICBank.Core.Interfaces;
using AICBank.Core.Util.Extensions;

namespace AICBank.API.Middlewares
{
    public class CustomRoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAccountUserRepository _accountUserRepository;
        public CustomRoleAuthorizationMiddleware(RequestDelegate next, IAccountUserRepository accountUserRepository)
        {
            _next = next;
            _accountUserRepository = accountUserRepository;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if(!httpContext.UserIsAuthenticated())
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await httpContext.Response.WriteAsync("Não autorizado.");

                return;
            }

            // var roleAdmin = httpContext.User.Claims.FirstOrDefault(c => c.Type == "UserType");
            // if(roleAdmin != null && roleAdmin.Value == "Admin")

            var menusAndRoles = new Dictionary<string,string>{
                {"bankAccount", "Admin,Managers"}
            };

            if(!httpContext.User.IsInRole("Admin")){
                var routeValues = httpContext.Request.RouteValues;
                foreach(var val in routeValues){
                    System.Console.WriteLine(val.Key, val.Value);
                }
            }

            await _next(httpContext);
        }
    }
}