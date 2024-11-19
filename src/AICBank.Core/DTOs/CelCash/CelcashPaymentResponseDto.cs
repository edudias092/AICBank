using System.Text.Json.Serialization;
using AICBank.Core.Util;

namespace AICBank.Core.DTOs.CelCash;

public class CelcashPaymentResponseDto
{
    public bool Type { get; set; }
    
    [JsonPropertyName("Payment")]
    public PaymentResponseDto Payment { get; set; }
}

public class PaymentResponseDto
{
    public int GalaxPayId { get; set; }
    public string Key { get; set; }
    public int Value { get; set; }
    public string Desc { get; set; }
    public string EndToEndId { get; set; }
    public string Status { get; set; }
    
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime CreatedAt { get; set; }
}