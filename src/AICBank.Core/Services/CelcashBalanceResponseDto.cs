using System.Text.Json.Serialization;
using AICBank.Core.Util;

namespace AICBank.Core.Services;

public class CelcashBalanceResponseDto
{
    public bool Type { get; set; }
    
    [JsonPropertyName("Balance")]
    public SaldoDTO Balance { get; set; }
}

public class SaldoDTO
{
    public int Enabled { get; set; }
    public int Requested { get; set; }
    public int BlockedBoleto { get; set; }
    public int BlockedCard { get; set; }
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime UpdatedAt { get; set; }
}