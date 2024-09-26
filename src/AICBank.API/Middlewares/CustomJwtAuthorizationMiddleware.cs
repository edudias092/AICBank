using System;
using System.IdentityModel.Tokens.Jwt;

namespace AICBank.API.Middlewares;

public class CustomJwtAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    
    private const string apiPrefix = "api";
    private List<AppRoute> RestrictedRoutes =
        new List<AppRoute>
        {
            new ("GetById", "BankAccount"),
            new ("Create", "BankAccount"),
            new ("Integrate", "BankAccount"),
            new ("Update", "BankAccount")
        };
    
    public record AppRoute(string Action, string Controller);

public CustomJwtAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(token))
        {
            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);
                
                // Agora você pode acessar as claims do JWT:
                var claims = jwtToken.Claims;

                // Exemplo: adicionar uma claim ao contexto do usuário
                context.User.AddIdentity(new System.Security.Claims.ClaimsIdentity(claims));
            }
        }

        if (!Authenticate(context))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            await context.Response.CompleteAsync();

            return;
        }
        
        await _next(context);
    }

    private bool Authenticate(HttpContext context)
    {
        var routeVals = context.Request.RouteValues;

        if (routeVals == null)
        {
            throw new Exception("Rota inválida.");
        }
        
        if (routeVals["controller"]?.ToString() == "AuthController")
        {
            return true;
        }

        if (RestrictedRoutes.Any(r => routeVals["action"].ToString() == r.Action 
                                      && routeVals["controller"].ToString() == r.Controller) 
            && !context.User.Identity.IsAuthenticated)
        {
            return false;
        }
        
        return true;
    }
}
