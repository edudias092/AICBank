using AICBank.Core.DTOs;
using AICBank.Core.Entities;
using MimeKit;

namespace AICBank.Core.Email;

public class UserAccountCreatedMessageBuilder : IEmailMessageBuilder
{
    private const string TemplateFilePath = "wwwroot/templates/ContaCriada.template.html";
    private const string LogoFilePath = "wwwroot/images/logo.png";
    private readonly AccountUserDTO _accountUserDto;
    private const string Subject = "Bem-vindo ao AIC BANK!";

    public UserAccountCreatedMessageBuilder(AccountUserDTO accountUserDto)
    {
        _accountUserDto = accountUserDto;
    }
    
    public async Task<MimeMessage> BuildEmailMessage()
    {
        var message = new MimeMessage ();


        var recipients = new List<EmailRecipient>
        {
            new (_accountUserDto.Email, _accountUserDto.Email)
        };
        
        foreach (var recipient in recipients)
            message.To.Add (new MailboxAddress (recipient.Name, recipient.Email));
        
        message.Subject = Subject;
        message.Body = await BuildBodyAsync(_accountUserDto);

        return message;
    }

    private async Task<MimeEntity> BuildBodyAsync(AccountUserDTO bankAccountDto)
    {
        var templateContents = await File.ReadAllTextAsync(TemplateFilePath);
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