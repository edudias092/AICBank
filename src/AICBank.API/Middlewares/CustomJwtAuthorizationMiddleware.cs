using System;
using System.IdentityModel.Tokens.Jwt;

namespace AICBank.API.Middlewares;

public class CustomJwtAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

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

        await _next(context);
    }
}
