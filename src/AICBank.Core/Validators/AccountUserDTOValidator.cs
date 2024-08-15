using System;
using AICBank.Core.DTOs;
using FluentValidation;

namespace AICBank.Core.Validators;

public class AccountUserDTOValidator : AbstractValidator<AccountUserDTO>
{
    public AccountUserDTOValidator()
    {
        RuleFor(x => x.Password).NotEmpty().WithMessage("Senha não pode ser vazia.");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Senhas não conferem.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email inválido.");
    }
}
