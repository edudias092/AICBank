using System;
using System.Text.Json.Serialization;
using AICBank.Core.Util;

namespace AICBank.Core.DTOs.CelCash;

public class BalanceDTO
{
    public int GalaxPayId { get; set; }
    public int Value { get; set; }
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime CreatedAt { get; set; }
    public string FriendlyDescription { get; set; }
    public string GroupPaymentType { get; set; }
    public string PaymentType { get; set; }
    public long TransactionGalaxPayId { get; set; }
}

public class TotalsDTO 
{
    public int Initial { get; set; }
    public int Final { get; set; }
}