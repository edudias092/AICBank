namespace AICBank.Core.DTOs.CelCash;
using System.Text.Json.Serialization;

public class CelcashSubaccountResponseDTO
{
    [JsonPropertyName("type")]
    public bool Type { get; set; }

    [JsonPropertyName("Company")]
    public CompanyDTO Company { get; set; }

    public ErrorDetails Error { get; set; }
}

public class CompanyDTO : BankAccountDTO
{
    public ApiAuthData ApiAuth { get; set; }
    public VerificationData Verification { get; set; }
}

public class VerificationData
{
    public string Status { get; set; }
    public string[] Reasons { get; set; }
}

public class ApiAuthData
{
    public string GalaxId { get; set; }
    public string GalasHash { get; set; }
    public string PublicToken { get; set; }
    public string confirmHashWebhook { get; set; }
}

public class ErrorDetails
{
    public string Message { get; set; }
    public Dictionary<string, object> Details { get; set; }
}