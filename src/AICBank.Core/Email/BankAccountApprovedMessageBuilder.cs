using AICBank.Core.DTOs;
using AICBank.Core.Entities;
using MimeKit;

namespace AICBank.Core.Email;

public class BankAccountApprovedMessageBuilder : IEmailMessageBuilder
{
    private const string TemplateFilePath = "wwwroot/templates/ContaAprovada.template.html";
    private const string LogoFilePath = "wwwroot/images/logo.png";
    private readonly BankAccountDTO _bankAccountDto;
    private const string Subject = "Conta Aprovada - AIC BANK";
    private List<EmailRecipient> _adminRecipients;

    public BankAccountApprovedMessageBuilder(BankAccountDTO bankAccountDto)
    {
        _bankAccountDto = bankAccountDto;

        _adminRecipients = new List<EmailRecipient>
        {
            new ("eduardo.dias092@outlook.com", "Eduardo Dias - TI"),
            new ("aicbrasill@gmail.com", "AIC Brasil")
        };
    }
    
    public async Task<MimeMessage> BuildEmailMessage()
    {
        var message = new MimeMessage ();
        var recipients = new List<EmailRecipient>
        {
            new (_bankAccountDto.EmailContact, _bankAccountDto.Name)
        };
        
        foreach (var recipient in recipients)
            message.To.Add (new MailboxAddress (recipient.Name, recipient.Email));

        foreach (var adminRecipient in _adminRecipients)
        {
            message.Bcc.Add (new MailboxAddress (adminRecipient.Name, adminRecipient.Email));
        }
        
        message.Subject = Subject;
        message.Body = await BuildBodyAsync(_bankAccountDto);

        return message;
    }

    private async Task<MimeEntity> BuildBodyAsync(BankAccountDTO bankAccountDto)
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