using System;
using System.Collections.ObjectModel;
using AICBank.Core.DTOs;
using AICBank.Core.Interfaces;
using AICBank.Core.Services;
using AICBank.Core.Validators;
using FakeItEasy;
using FluentValidation;
using FluentValidation.Results;

namespace AICBank.Core.Tests.Services;

public class AuthServiceTests
{
    private IAuthRepository _authRepository;
    private IAccountUserRepository _accountUserRepository;
    private IValidator<AccountUserDTO> _accountUserValidator;

    private AuthService _sut;
    public AuthServiceTests()
    {
        _authRepository = A.Fake<IAuthRepository>();
        _accountUserRepository = A.Fake<IAccountUserRepository>();
        _accountUserValidator = new AccountUserDTOValidator();

        _sut = new AuthService(_authRepository, _accountUserRepository, _accountUserValidator);
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
                Success = false
            });

        //Act
        UserToken result = await _sut.Register(userDTO);

        //Assert
        Assert.Null(result);
    }
}
