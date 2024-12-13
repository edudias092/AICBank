using System.Text.Json.Serialization;

namespace AICBank.Core.DTOs.CelCash;

public class CelcashCompanyDocumentsWebhookRequestDto
{
    [JsonPropertyName("Company")]
    public CompanyDocumentsWebhookRequestDto Company { get; set; }
}

public class CompanyDocumentsWebhookRequestDto
{
    [JsonPropertyName("galaxPayId")]
    public int GalaxId { get; set; }

    [JsonPropertyName("Verification")] 
    public VerificationDataDto Verification { get; set; }
}

public class VerificationDataDto
{
    public string Status { get; set; }
    [JsonPropertyName("Reasons")]
    public string[] Reasons { get; set; }
}