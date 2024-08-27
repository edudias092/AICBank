using System;
using System.Text.Json.Serialization;

namespace AICBank.Core.DTOs.CelCash;

public class CelcashTokenResponseDTO
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
