using AICBank.Core.DTOs;
using AICBank.Core.Email;
using AICBank.Core.Services;

namespace AICBank.Core.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(IEmailMessageBuilder messageBuilder);
}