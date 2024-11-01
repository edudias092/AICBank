using System;
using System.Text.Json.Serialization;

namespace AICBank.Core.DTOs.CelCash;

public class CelcashChargeResponseDTO
{
    [JsonPropertyName("type")]
    public bool Type { get; set; }

    [JsonPropertyName("Charge")]
    public ChargeDTO Charge { get; set; }

    public ErrorDetails Error { get; set; }
}
