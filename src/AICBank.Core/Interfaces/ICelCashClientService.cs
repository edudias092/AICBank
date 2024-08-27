using System;
using AICBank.Core.DTOs;

namespace AICBank.Core.Interfaces;

public interface ICelCashClientService
{
    Task CreateSubBankAccount(BankAccountDTO bankAccountDTO);
    Task SendMandatoryDocuments(BankAccountDTO bankAccountDTO);
}
