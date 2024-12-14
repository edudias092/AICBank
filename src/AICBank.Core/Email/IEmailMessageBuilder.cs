using MimeKit;

namespace AICBank.Core.Email;

public interface IEmailMessageBuilder
{
    Task<MimeMessage> BuildEmailMessage();
}