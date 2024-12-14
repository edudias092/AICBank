using AICBank.Core.DTOs;
using AICBank.Core.Entities;
using MimeKit;

namespace AICBank.Core.Email;

public class BankAccountSavedMessageBuilder : IEmailMessageBuilder
{
    private const string TemplateFilePath = "wwwroot/templates/ContaCriada.template.html";
    private const string LogoFilePath = "wwwroot/images/logo.png";
    private readonly BankAccountDTO _bankAccountDto;
    private const string Subject = "Bem-vindo ao AIC BANK!";
    private List<EmailRecipient> _adminRecipients;

    public BankAccountSavedMessageBuilder(BankAccountDTO bankAccountDto)
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