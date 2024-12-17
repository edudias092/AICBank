namespace AICBank.Core.DTOs;

public record EmailConfig
{
    public string Smtp { get; init; }
    public string Port { get; init; }
    public string User { get; init; }
    public string Password { get; init; }
}