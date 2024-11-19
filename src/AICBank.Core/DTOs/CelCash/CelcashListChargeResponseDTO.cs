using System;
using System.Text.Json.Serialization;

namespace AICBank.Core.DTOs.CelCash;

public class CelcashListChargeResponseDTO
{
    [JsonPropertyName("type")]
    public bool TotalQtdFoundInPage { get; set; }

    [JsonPropertyName("Charges")]
    public CelcashChargeDTO[] Charges { get; set; }

    public ErrorDetails Error { get; set; }
}
