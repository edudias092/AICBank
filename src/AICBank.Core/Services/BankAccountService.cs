using System;
using System.Runtime.CompilerServices;
using AICBank.Core.DTOs;
using AICBank.Core.Entities;
using AICBank.Core.Interfaces;

namespace AICBank.Core.Services;

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    public BankAccountService(IBankAccountRepository bankAccountRepository)
    {
        _bankAccountRepository = bankAccountRepository;
    }
    public async Task<ResponseDTO<BankAccountDTO>> CreateBankAccount(BankAccountDTO bankAccountDTO)
    {
        var bankAccount = new BankAccount
        {
            Name = bankAccountDTO.Name,
            Document = bankAccountDTO.Document,
            Phone = bankAccountDTO.Phone,
            EmailContact = bankAccountDTO.EmailContact,
            Logo = bankAccountDTO.Logo,
            SoftDescriptor = bankAccountDTO.SoftDescriptor,
            Address = new Address {
                ZipCode = bankAccountDTO.Address.ZipCode,
                Street = bankAccountDTO.Address.Street,
                Number = bankAccountDTO.Address.Number,
                Complement = bankAccountDTO.Address.Complement,
                Neighborhood = bankAccountDTO.Address.Neighborhood,
                City = bankAccountDTO.Address.City,
                State = bankAccountDTO.Address.State
            },
            GalaxPayId = bankAccountDTO.GalaxPayId,
            GalaxId = bankAccountDTO.GalaxId,
            GalaxHash = bankAccountDTO.GalaxHash,
            NameDisplay = bankAccountDTO.NameDisplay,
            ResponsibleDocument = bankAccountDTO.ResponsibleDocument,
            TypeCompany = bankAccountDTO.TypeCompany,
            Cnae = bankAccountDTO.Cnae,
            Status = bankAccountDTO.Status,
            Type = bankAccountDTO.Type,
            Professional = new Professional 
            { 
                InternalName = bankAccountDTO.Professional?.InternalName,
                Inscription = bankAccountDTO.Professional?.Inscription
            },
            AccountUserId = bankAccountDTO.AccountUser.Id,
        };

        await _bankAccountRepository.Add(bankAccount);

        return new ResponseDTO<BankAccountDTO>{
            Success = true,
            Data = bankAccountDTO
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> GetBankAccountById(int id)
    {
        var bankAccount = await _bankAccountRepository.GetById(id);

        var bankAccountDTO = new BankAccountDTO
        {
            Id = bankAccount.Id,
            Cnae = bankAccount.Cnae,
            Document = bankAccount.Document,
            EmailContact = bankAccount.EmailContact,
            GalaxHash = bankAccount.GalaxHash,
            GalaxId = bankAccount.GalaxId,
            GalaxPayId = bankAccount.GalaxPayId,
            Logo = bankAccount.Logo,
            Name = bankAccount.Name,
            NameDisplay = bankAccount.NameDisplay,
            Phone = bankAccount.Phone,
            ResponsibleDocument = bankAccount.ResponsibleDocument,
            Status = bankAccount.Status,
            Type = bankAccount.Type,
            SoftDescriptor = bankAccount.SoftDescriptor,
            TypeCompany = bankAccount.TypeCompany,
            Address = new AddressDTO 
            {
                Id = bankAccount.Address.Id,
                City = bankAccount.Address.City,
                Complement = bankAccount.Address.Complement,
                Neighborhood = bankAccount.Address.Neighborhood,
                Number = bankAccount.Address.Number,
                State = bankAccount.Address.State,
                Street = bankAccount.Address.Street,
                ZipCode = bankAccount.Address.ZipCode
            },
            Professional = new ProfessionalDTO 
            {
                Id = bankAccount.Professional.Id,
                Inscription = bankAccount.Professional.Inscription,
                InternalName = bankAccount.Professional.InternalName
            }
        };

        return new ResponseDTO<BankAccountDTO>{
            Success = true,
            Errors = [],
            Data = bankAccountDTO
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> UpdateBankAccount(BankAccountDTO bankAccountDTO)
    {
        var bankAccount = new BankAccount
        {
            Name = bankAccountDTO.Name,
            Document = bankAccountDTO.Document,
            Phone = bankAccountDTO.Phone,
            EmailContact = bankAccountDTO.EmailContact,
            Logo = bankAccountDTO.Logo,
            SoftDescriptor = bankAccountDTO.SoftDescriptor,
            Address = new Address {
                ZipCode = bankAccountDTO.Address.ZipCode,
                Street = bankAccountDTO.Address.Street,
                Number = bankAccountDTO.Address.Number,
                Complement = bankAccountDTO.Address.Complement,
                Neighborhood = bankAccountDTO.Address.Neighborhood,
                City = bankAccountDTO.Address.City,
                State = bankAccountDTO.Address.State
            },
            GalaxPayId = bankAccountDTO.GalaxPayId,
            GalaxId = bankAccountDTO.GalaxId,
            GalaxHash = bankAccountDTO.GalaxHash,
            NameDisplay = bankAccountDTO.NameDisplay,
            ResponsibleDocument = bankAccountDTO.ResponsibleDocument,
            TypeCompany = bankAccountDTO.TypeCompany,
            Cnae = bankAccountDTO.Cnae,
            Status = bankAccountDTO.Status,
            Type = bankAccountDTO.Type,
            Professional = new Professional 
            { 
                InternalName = bankAccountDTO.Professional?.InternalName,
                Inscription = bankAccountDTO.Professional?.Inscription
            },
            AccountUserId = bankAccountDTO.AccountUser.Id,
        };

        await _bankAccountRepository.Update(bankAccount);

        return new ResponseDTO<BankAccountDTO>{
            Success = true,
            Data = bankAccountDTO
        };
    }
}
