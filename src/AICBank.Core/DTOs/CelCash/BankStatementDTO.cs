using System;
using System.Text.Json.Serialization;

namespace AICBank.Core.DTOs.CelCash;

public class BankStatementDTO
{
    public bool Type { get; set; }

    [JsonPropertyName("Balances")]
    public List<BalanceDTO> Balances { get; set; }

    [JsonPropertyName("Totals")]
    public TotalsDTO Totals { get; set; }
    
    public ErrorDetails Error { get; set; }
}
