using System;
using System.Collections.ObjectModel;
using AICBank.Core.DTOs;
using AICBank.Core.Entities;
using AICBank.Core.Interfaces;
using AICBank.Core.Services;
using AICBank.Core.Validators;
using FakeItEasy;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;

namespace AICBank.Core.Tests.Services;

public class AuthServiceTests
{
    private IAuthRepository _authRepository;
    private IAccountUserRepository _accountUserRepository;
    private IValidator<AccountUserDTO> _accountUserValidator;
    private IConfiguration _configuration;

    private AuthService _sut;
    public AuthServiceTests()
    {
        _authRepository = A.Fake<IAuthRepository>();
        _accountUserRepository = A.Fake<IAccountUserRepository>();
        _accountUserValidator = new AccountUserDTOValidator();
        _configuration = A.Fake<IConfiguration>();

        _sut = new AuthService(_authRepository, _accountUserRepository, _accountUserValidator, _configuration);
    }

    [Fact]
    public async Task Register_ShouldThrowException_WhenDataIsInvalid()
    {
        //Arrange
        var userDTO = new AccountUserDTO
        {
            Email = "bademail",
            Password = "",
            ConfirmPassword = "another"
        };

        //Act
        //Assert
        UserToken result;
        var ex = await Assert.ThrowsAsync<ValidationException>( async () => {
            result = await _sut.Register(userDTO);
        });
        
        Assert.NotNull(ex.Errors.FirstOrDefault(x => x.ErrorMessage == "Senha não pode ser vazia."));
        Assert.NotNull(ex.Errors.FirstOrDefault(x => x.ErrorMessage == "Senhas não conferem."));
        Assert.NotNull(ex.Errors.FirstOrDefault(x => x.ErrorMessage == "Email inválido."));
    }

    [Fact]
    public async Task Register_ShouldNotThrow_WhenDataIsValid()
    {
        //Arrange
        var userDTO = new AccountUserDTO
        {
            Email = "test@test.com",
            Password = "Pwd123",
            ConfirmPassword = "Pwd123"
        };

        A.CallTo(() => _authRepository.Register(A<AccountUserDTO>.Ignored))
            .Returns(new AuthResult(){
                Success = true,
                Errors = []
            });

        A.CallTo(() => _authRepository.GetUserByEmail(A<string>.Ignored))
            .Returns(new IdentityUserDTO{
                Email = "test@test.com",
                Id = Guid.NewGuid().ToString()
            });

        //Act
        UserToken result = await _sut.Register(userDTO);

        //Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("anInvalidPhone")]
    [InlineData("31-123234859")]
    [InlineData("(31)123234859")]
    [InlineData("319958181978")]
    public async Task Register_ShouldThrow_WhenPhoneIsInvalid(string invalidPhone)
    {
        //Arrange
        var userDTO = new AccountUserDTO
        {
            Email = "test@test.com",
            Password = "mypwd",
            ConfirmPassword = "mypwd",
            Phone = invalidPhone
        };

        //Act
        //Assert
        UserToken result;
        var ex = await Assert.ThrowsAsync<ValidationException>( async () => {
            result = await _sut.Register(userDTO);
        });

        Assert.NotNull(ex.Errors.FirstOrDefault(x => x.ErrorMessage == "Telefone inválido."));
    }

    [Theory]
    [InlineData("31999887766")]
    [InlineData("41999776655")]
    [InlineData("99999776655")]
    public async Task Register_ShouldNotThrow_WhenPhoneIsValid(string validPhone)
    {
        //Arrange
        var userDTO = new AccountUserDTO
        {
            Email = "test@test.com",
            Password = "mypwd",
            ConfirmPassword = "mypwd",
            Phone = validPhone
        };

        A.CallTo(() => _authRepository.Register(A<AccountUserDTO>.Ignored))
            .Returns(new AuthResult(){
                Success = true,
                Errors = []
            });

        A.CallTo(() => _authRepository.GetUserByEmail(A<string>.Ignored))
            .Returns(new IdentityUserDTO{
                Email = "test@test.com",
                Id = Guid.NewGuid().ToString()
            });

        //Act
        UserToken result = await _sut.Register(userDTO);

        //Assert
        Assert.Null(result);
        A.CallTo(() => _accountUserRepository.Add(A<AccountUser>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
