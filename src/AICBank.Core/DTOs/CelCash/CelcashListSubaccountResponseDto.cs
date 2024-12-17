using System.Text.Json.Serialization;

namespace AICBank.Core.DTOs.CelCash;

public class CelcashListSubaccountResponseDto
{
    [JsonPropertyName("type")]
    public bool Type { get; set; }

    [JsonPropertyName("SubAccounts")]
    public CelcashCompanyDTO[] Subaccounts { get; set; }
}