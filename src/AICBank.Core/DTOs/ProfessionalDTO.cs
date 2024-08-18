using System;

namespace AICBank.Core.DTOs;

public class ProfessionalDTO
{
    public int Id { get; set; }
    public string InternalName { get; set; } //lawyer,doctor,accountant,realtor,broker,physicalEducator,physiotherapist,others
    public string Inscription { get; set; }
    public BankAccountDTO BankAccount { get; set; }
}
