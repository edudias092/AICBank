using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AICBank.Core.DTOs;
using AICBank.Core.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AICBank.Data.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private UserManager<IdentityUser> _userManager;
        private SignInManager<IdentityUser> _signinManager;
        public AuthRepository(UserManager<IdentityUser> userManager, 
                                SignInManager<IdentityUser> signinManager)
        {
            _userManager = userManager;
            _signinManager = signinManager;
        }

        public async Task<IdentityUserDTO> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if(user == null) return null;

            return new IdentityUserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };
        }

        public async Task<AuthResult> Login(string email, string password)
        {
            var result = await _signinManager.PasswordSignInAsync(email, password, isPersistent: true, lockoutOnFailure: false);

            return new AuthResult{
                Action = "Login",
                Success = result.Succeeded,
                Errors = ["Usuário e/ou senha inválidos"]
            };
        }

        public async Task<AuthResult> Register(AccountUserDTO user)
        {
            var identityUser = new IdentityUser{
                Email = user.Email,
                UserName = user.Email
            };

            var result = await _userManager.CreateAsync(identityUser, user.Password);

            return new AuthResult
            {
                Action = "Register",
                Success = result.Succeeded,
                Errors = result.Errors.Select(x => $"{x.Code}: {x.Description}").ToArray()
            };
        }
    }
}