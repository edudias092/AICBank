using System;
using System.Text.Json.Serialization;
using AICBank.Core.Util;

namespace AICBank.Core.DTOs;

public class TransactionDTO
{
    public int ValueInCents { get; set; }
    public double Value { get {
        return Convert.ToDouble(ValueInCents / 100);
    }}

    public DateTime PayDay { get; set; }
    
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime StatusDate { get; set; }
    public string Status { get; set; }

    [JsonPropertyName("Boleto")]
    public BoletoDTO Boleto { get; set; }

    [JsonPropertyName("Pix")]
    public PixDTO Pix { get; set; }

}
public class BoletoDTO
{
    public string Pdf { get; set; }
    public string BankLine { get; set; }
    public string BankAgency { get; set; }
    public string BankAccount { get; set; }
}

public class PixDTO
{
    public string QrCode { get; set; }
    public string Reference { get; set; }
    public string Image { get; set; }
    public string Page { get; set; }
}

