using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AICBank.Core.DTOs;
using AICBank.Core.Entities;
using AICBank.Core.Interfaces;
using FluentValidation;

namespace AICBank.Core.Services
{
    public class AuthService : IAuthService
    {
        private IAuthRepository _authRepository;
        private IAccountUserRepository _accountUserRepository;
        private IValidator<AccountUserDTO> _accountUserValidator;


        public AuthService(IAuthRepository authRepository, 
                            IAccountUserRepository accountUserRepository,
                            IValidator<AccountUserDTO> accountUserValidator)
        {
            _authRepository = authRepository;
            _accountUserRepository = accountUserRepository;
            _accountUserValidator = accountUserValidator;
        }

        public Task<UserToken> Login(string email, string password)
        {
            throw new NotImplementedException();
        }

        public async Task<UserToken> Register(AccountUserDTO user)
        {
            _accountUserValidator.ValidateAndThrow(user);

            AuthResult result = await _authRepository.Register(user);

            if(result.Success)
            {
                var identityUser = await _authRepository.GetUserByEmail(user.Email);
                if(identityUser == null){
                    //Error
                    throw new ApplicationException("User not found");
                }

                //Map
                var userDb = new AccountUser{
                    Email = user.Email,
                    Name = user.Email,
                    IdentityUserId = identityUser.Id
                };

                //Create AccountUser
                await _accountUserRepository.Add(userDb);

                var token = CreateUserToken(user);

                return token;
            }

            return null;
        }

        private UserToken CreateUserToken(AccountUserDTO user)
        {
            throw new NotImplementedException();
        }
    }
}