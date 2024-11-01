using System;
using System.Text.Json.Serialization;

namespace AICBank.Core.DTOs;

public class ChargeDTO
{
    public ChargeDTO()
    {
        MyId = Guid.NewGuid();
    }

    public Guid MyId { get; set; }

    public int GalaxPayId { get; set; }

    [JsonPropertyName("valueInDouble")]
    public double Value { get; set; }

    [JsonPropertyName("value")]
    public int ValueInCents {get {
        return Convert.ToInt32(Value * 100);
    }}

    public DateTime Paydate { get; set; }
    public string Payday { get => $"{Paydate:yyyy-MM-dd}" ; }

    public string MainPaymentMethodId { get; set; }

    public string PaymentLink { get; set; }

    public string AdditionalInfo { get; set; }

    public string Status { get; set; }

    [JsonPropertyName("Customer")]
    public CustomerDTO Customer { get; set;}

    [JsonPropertyName("Transactions")]
    public List<TransactionDTO> Transactions { get; set; }

}

public class PaymentMethods {
    public const string CreditCard = "creditcard";
    public const string Boleto = "boleto";
    public const string Pix = "pix";

}