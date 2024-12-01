using AICBank.Core.DTOs;
using AICBank.Core.Services;

namespace AICBank.Core.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string subject, BankAccountDTO bankAccountDto);
}