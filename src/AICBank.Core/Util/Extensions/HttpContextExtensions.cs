using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace AICBank.Core.Util.Extensions;

public static class HttpContextExtensions
{
    public static string GetAccountUserId(this HttpContext httpContext)
    {
        var claims = httpContext.User?.Claims;

        if(claims != null && claims.Any())
        {
            return claims.FirstOrDefault(x => x.Type == "AccountUserId")?.Value;
        }

        return null;
    }

    public static bool UserIsAuthenticated(this HttpContext httpContext)
    {
        return httpContext.User != null && httpContext.User.Identity.IsAuthenticated;
    }

    public static string GetUserEmail(this HttpContext httpContext)
    {
        var claims = httpContext.User?.Claims;

        if(claims != null && claims.Any())
        {
            return claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        }

        return null;
    }

}
