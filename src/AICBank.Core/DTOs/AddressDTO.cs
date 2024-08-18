using System;

namespace AICBank.Core.DTOs;

public class AddressDTO
{
    public int Id { get; set; }
    public string ZipCode { get; set; }
    public string Street { get; set; }
    public string Number { get; set; }
    public string Complement { get; set; }
    public string Neighborhood { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public int BankAccountId { get; set; }
    public BankAccountDTO BankAccount { get; set; }
}
