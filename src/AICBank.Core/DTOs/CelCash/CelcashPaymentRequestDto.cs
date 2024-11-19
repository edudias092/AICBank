using System.Text.Json.Serialization;

namespace AICBank.Core.DTOs.CelCash;

public class CelcashPaymentRequestDto
{
    public string Key { get; set; }
    public string Type { get; set; }
    [JsonPropertyName("valueInDouble")]
    public double Value { get; set; }

    [JsonPropertyName("value")]
    public int ValueInCents {get {
        return Convert.ToInt32(Value * 100);
    }}
    public string Desc { get; set; }
}

public static class CelcashPaymentType
{
    public static string Cpf = "cpf";
    public static string Cnpj = "cnpj";
    public static string Email = "email";
    public static string MobilePhone = "mobilePhone";
    public static string Random = "random";
}