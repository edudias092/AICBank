namespace AICBank.Core.DTOs.CelCash;

using System.Text.Json;
using System.Text.Json.Serialization;

public class CelcashCreatedSubaccountResponseDTO
{
    [JsonPropertyName("type")]
    public bool Type { get; set; }

    [JsonPropertyName("Company")]
    public CelcashCompanyDTO CelcashCompany { get; set; }

    public ErrorDetails Error { get; set; }
}

public class CelcashCompanyDTO : BankAccountDTO
{
    [JsonPropertyName("ApiAuth")]
    public ApiAuthData ApiAuth { get; set; }
    [JsonPropertyName("Verification")]
    public VerificationData Verification { get; set; }
}

public class VerificationData
{
    public string Status { get; set; }
    public string[] Reasons { get; set; }
}

public class ApiAuthData
{
    public int GalaxId { get; set; }
    public string GalaxHash { get; set; }
    public string PublicToken { get; set; }
    public string confirmHashWebhook { get; set; }
}

public class ErrorDetails
{
    public string Message { get; set; }
    public Dictionary<string, string[]> Details { get; set; }
}