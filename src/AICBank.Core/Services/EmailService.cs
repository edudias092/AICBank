using AICBank.Core.DTOs;
using AICBank.Core.Email;
using AICBank.Core.Interfaces;
using AICBank.Core.Util;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.Configuration;
using MimeKit;

namespace AICBank.Core.Services;

public class EmailService : IEmailService
{
    private readonly EmailConfig _emailConfig;
    public EmailService(IConfiguration configuration)
    {
        _emailConfig = new EmailConfig
        {
            Smtp = configuration["Email:Smtp"] ?? throw new InvalidConfigurationException("Email:Smtp"),
            Port = configuration["Email:Port"] ?? throw new InvalidConfigurationException("Email:Port"),
            User = configuration["Email:User"] ?? throw new InvalidConfigurationException("Email:User"),
            Password = configuration["Email:Password"] ?? throw new InvalidConfigurationException("Email:Password"),
        };
    }
    
    public async Task SendEmailAsync(IEmailMessageBuilder emailMessageBuilder)
    {
        var message = await emailMessageBuilder.BuildEmailMessage();
        message.From.Add( new MailboxAddress ("No-Reply", _emailConfig.User));
        
        using var client = new SmtpClient();
        await client.ConnectAsync(_emailConfig.Smtp, 
            int.Parse(_emailConfig.Port), 
            SecureSocketOptions.StartTls);
        
        await client.AuthenticateAsync(_emailConfig.User, _emailConfig.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}