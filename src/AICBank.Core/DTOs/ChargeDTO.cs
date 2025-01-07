using System;
using System.Text.Json.Serialization;

namespace AICBank.Core.DTOs;

public class ChargeDTO
{
    public ChargeDTO()
    {
        MyId = Guid.NewGuid();
    }

    public Guid? MyId { get; set; }

    public int GalaxPayId { get; set; }

    [JsonPropertyName("valueInDouble")]
    public double Value { get; set; }

    [JsonPropertyName("value")]
    public int ValueInCents => Convert.ToInt32(Value * 100);

    public DateTime Paydate { get; set; }
    public string Payday => $"{Paydate:yyyy-MM-dd}";

    public string MainPaymentMethodId { get; set; }

    public string PaymentLink { get; set; }

    public string AdditionalInfo { get; set; }

    public string Status { get; set; }

    [JsonPropertyName("Customer")]
    public CustomerDTO Customer { get; set;}

    [JsonPropertyName("Transactions")]
    public List<TransactionDTO> Transactions { get; set; }

    [JsonPropertyName("PaymentMethodBoleto")]
    public PaymentMethodBoleto PaymentMethodBoleto { get; set; }
    
    [JsonPropertyName("Split")] 
    public Split Split { get; set; }
}

public class PaymentMethods {
    public const string CreditCard = "creditcard";
    public const string Boleto = "boleto";
    public const string Pix = "pix";
}

public class PaymentMethodBoleto
{
    public int? Fine { get; set; }
    public int? Interest { get; set; }
    public string Instructions { get; set; }
    public int? DeadlineDays { get; set; }
}

public class Split
{
    public SplitDetails All { get; set; }
}

public class SplitDetails
{
    private string _type;
    public string Type
    {
        get => _type;
        set
        {
            if (value != SplitType.Fixed && value != SplitType.Percent)
            {
                throw new ArgumentException("Split type must be either Fixed, or Percent.");
            }
            
            _type = value;
        }
    }

    [JsonPropertyName("Companies")]
    public SplitCompany[] Companies { get; set; }
}

public class SplitType
{
    public const string Percent = "percent";
    public const string Fixed = "fixed";
}

public class SplitCompany
{
    public int GalaxId { get; set; }
    public int Value { get; set; }
}