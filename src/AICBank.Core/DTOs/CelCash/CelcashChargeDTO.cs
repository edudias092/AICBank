using System;
using System.Text.Json.Serialization;
using AICBank.Core.Util;

namespace AICBank.Core.DTOs.CelCash;

public class CelcashChargeDTO
{

    public string MyId { get; set; }

    public int GalaxPayId { get; set; }

    public int Value { get; set; }

    public string MainPaymentMethodId { get; set; }

    public string PaymentLink { get; set; }

    public string AdditionalInfo { get; set; }

    public string Status { get; set; }

    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime createdAt { get; set; }

    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime updatedAt { get; set; }

    [JsonPropertyName("Customer")]
    public CustomerDTO Customer { get; set;}

    [JsonPropertyName("Transactions")]
    public TransactionDTO[] Transactions { get; set; }
}
