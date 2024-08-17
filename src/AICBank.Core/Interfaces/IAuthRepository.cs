
using AICBank.Core.DTOs;

namespace AICBank.Core.Interfaces
{
    public interface IAuthRepository
    {
        Task<AuthResult> Login(string email, string password);
        Task<AuthResult> Register(AccountUserDTO user);
        Task<IdentityUserDTO> GetUserByEmail(string email);
        Task<List<IdentityClaimDTO>> GetUserClaims(string email);
    }
}