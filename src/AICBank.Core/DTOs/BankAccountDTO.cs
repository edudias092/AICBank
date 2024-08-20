using System;
using AICBank.Core.Entities;

namespace AICBank.Core.DTOs;

public class BankAccountDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Document { get; set; }
    public string Phone { get; set; }
    public string EmailContact { get; set; }
    public string Logo { get; set; }
    public string SoftDescriptor { get; set; }
    public AddressDTO Address { get; set; }
    public int GalaxPayId { get; set; }
    public string GalaxId { get; set; }
    public string GalaxHash { get; set; }
    public string NameDisplay { get; set; }
    public string ResponsibleDocument { get; set; }
    public string TypeCompany { get; set; }
    public string Cnae { get; set; }
    public StatusBankAccount Status { get; set; }
    public TypeBankAccount Type { get; set; }
    public ProfessionalDTO Professional { get; set; }
    public int AccountUserId { get; set; }
}
