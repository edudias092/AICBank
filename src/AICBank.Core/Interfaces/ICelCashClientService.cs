using System;
using AICBank.Core.DTOs;
using AICBank.Core.DTOs.CelCash;

namespace AICBank.Core.Interfaces;

public interface ICelCashClientService
{
    Task<CelcashSubaccountResponseDTO> CreateSubBankAccount(BankAccountDTO bankAccountDTO);
    Task SendMandatoryDocuments(BankAccountDTO bankAccountDTO);
}
