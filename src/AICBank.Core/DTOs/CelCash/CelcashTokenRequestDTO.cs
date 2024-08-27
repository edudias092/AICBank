using System;
using System.Text.Json.Serialization;

namespace AICBank.Core.DTOs.CelCash;

public class CelcashTokenRequestDTO
{
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }
}
