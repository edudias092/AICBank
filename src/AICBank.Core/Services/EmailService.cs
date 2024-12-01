using AICBank.Core.DTOs;
using AICBank.Core.Interfaces;
using AICBank.Core.Util;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace AICBank.Core.Services;

public class EmailService(IConfiguration configuration) : IEmailService
{
    private const string TemplateFilePath = "wwwroot/templates/ContaCriada.template.html";
    private const string LogoFilePath = "wwwroot/images/logo.png";
    
    public async Task SendEmailAsync(string subject, BankAccountDTO bankAccountDto)
    {
        var emailConfig = new EmailConfig
        {
            Smtp = configuration["Email:Smtp"] ?? throw new NullReferenceException(nameof(EmailConfig.Smtp)),
            Port = configuration["Email:Port"] ?? throw new NullReferenceException(nameof(EmailConfig.Port)),
            User = configuration["Email:User"] ?? throw new NullReferenceException(nameof(EmailConfig.User)),
            Password = configuration["Email:Password"] ?? throw new NullReferenceException(nameof(EmailConfig.Password))
        };
        
        var message = new MimeMessage ();
        message.From.Add (new MailboxAddress ("No-Reply", emailConfig.User));

        var recipients = new List<EmailRecipient>
        {
            new ()
            {
                Name = bankAccountDto.Name,
                Email = bankAccountDto.EmailContact
            }
        };
        
        foreach (var recipient in recipients)
            message.To.Add (new MailboxAddress (recipient.Name, recipient.Email));
        
        message.Subject = subject;
        message.Body = await BuildBodyAsync(bankAccountDto);

        using var client = new SmtpClient();
        await client.ConnectAsync(emailConfig.Smtp, 
            int.Parse(emailConfig.Port), 
            SecureSocketOptions.StartTls);
        
        // await client.AuthenticateAsync("109cf7c4fa376c", "277e8654cd9ae4");
        await client.AuthenticateAsync(emailConfig.User, emailConfig.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private async Task<MimeEntity> BuildBodyAsync(BankAccountDTO bankAccountDto)
    {
        var templateContents = await File.ReadAllTextAsync(TemplateFilePath);

        Dictionary<string, string> placeholders = new Dictionary<string, string>()
        {
            { "Titular", bankAccountDto.Name},
            { "Telefone", bankAccountDto.Phone},
            { "EnderecoRua", bankAccountDto.Address?.Street},
            { "EnderecoNumero", bankAccountDto.Address?.Number},
            { "EnderecoComplemento",bankAccountDto.Address?.Complement },
            { "EnderecoCidade", bankAccountDto.Address?.City},
            { "EnderecoEstado", bankAccountDto.Address?.State},
            { "EnderecoCep", bankAccountDto.Address?.ZipCode},
        };
        
        foreach (var placeholder in placeholders)
            templateContents = templateContents.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
        
        var builder = new BodyBuilder()
        {
            HtmlBody = templateContents
        };

        var imageLogo = builder.LinkedResources.Add(LogoFilePath);
        imageLogo.ContentId = "ImageContentId";

        builder.HtmlBody = builder.HtmlBody.Replace("{{CaminhoArquivoLogo}}", $"cid:{imageLogo.ContentId}");
        
        return builder.ToMessageBody();
    }
}

public record EmailRecipient
{
    public string Email { get; set; }
    public string Name { get; set; }
}

public record EmailConfig
{
    public string Smtp { get; init; }
    public string Port { get; init; }
    public string User { get; init; }
    public string Password { get; init; }
}