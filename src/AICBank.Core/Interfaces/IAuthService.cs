using AICBank.Core.DTOs;

namespace AICBank.Core.Services
{
    public interface IAuthService
    {
        Task<UserToken> Login(string email, string password);
        Task<UserToken> Register(AccountUserDTO user);
    }
}