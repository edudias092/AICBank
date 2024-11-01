using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AICBank.Core.DTOs;
using AICBank.Core.Entities;
using AICBank.Core.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ValidationFailure = FluentValidation.Results.ValidationFailure;

namespace AICBank.Core.Services
{
    public class AuthService : IAuthService
    {
        private IAuthRepository _authRepository;
        private IAccountUserRepository _accountUserRepository;
        private IValidator<AccountUserDTO> _accountUserValidator;
        private IConfiguration _configuration;

        public AuthService(IAuthRepository authRepository, 
                            IAccountUserRepository accountUserRepository,
                            IValidator<AccountUserDTO> accountUserValidator,
                            IConfiguration configuration)
        {
            _authRepository = authRepository;
            _accountUserRepository = accountUserRepository;
            _accountUserValidator = accountUserValidator;
            _configuration = configuration;
        }

        public async Task<UserToken> Login(string email, string password)
        {
            var result = await _authRepository.Login(email, password);

            if(!result.Success){
                throw new ApplicationException("Usuário e/ou Senha inválido.");
            }
            
            var users = await _accountUserRepository.Get(x => x.Email == email);
            
            if(!users.Any()){
                throw new ApplicationException("Usuário não encontrado.");
            }

            var user = users.FirstOrDefault();
            return await CreateUserToken(new AccountUserDTO
            {
                Id = user.Id,
                Email = user.Email,
                Phone = user.Phone
            });
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
                user.Id = userDb.Id;
                
                var token = await CreateUserToken(user);

                return token;
            }
            else
            {
                throw new ValidationException("Erro ao inserir usuário.", 
                    result.Errors.Select(e => new ValidationFailure {
                         ErrorMessage = e
                    }));
            }
        }

        private async Task<UserToken> CreateUserToken(AccountUserDTO user)
        {
            var secretKey = _configuration["JWT:SecretKey"] 
                    ?? throw new ArgumentException("Key Not Found");

            var claimsDTO = await _authRepository.GetUserClaims(user.Email);

            var claims = new List<Claim>
            {
                new Claim("Email", user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("AccountUserId", user.Id.ToString())
            };
            claims.AddRange(claimsDTO.Select(c => new Claim(c.Type, c.Value)));

            var key = Encoding.UTF8.GetBytes(secretKey);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                                                            SecurityAlgorithms.HmacSha256Signature);
            
            var tokenValidityInMinutes = int.Parse(_configuration["JWT:TokenValidityInMinutes"]);
            var expiresIn = DateTime.UtcNow.AddMinutes(tokenValidityInMinutes);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresIn,
                Audience = _configuration["JWT:Audience"],
                Issuer = _configuration["JWT:Issuer"],
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateEncodedJwt(tokenDescriptor);

            return new UserToken
            {
                Token = token,
                ExpiresIn = expiresIn,
                Claims = claims.Select(c => new UserClaims { Type = c.Type, Value = c.Value }).ToArray()
            };
        }
    }
}