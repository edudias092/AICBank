using System;
using System.Text.Json.Serialization;

namespace AICBank.Core.DTOs;

public class ProfessionalDTO
{
    public int Id { get; set; }
    public string InternalName { get; set; } //lawyer,doctor,accountant,realtor,broker,physicalEducator,physiotherapist,others
    public string Inscription { get; set; }
    [JsonIgnore]
    public BankAccountDTO BankAccount { get; set; }
}
