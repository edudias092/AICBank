using System;
using AICBank.Core.DTOs;

namespace AICBank.Core.Interfaces;

public interface IBankAccountService
{
    Task<ResponseDTO<BankAccountDTO>> CreateBankAccount(BankAccountDTO bankAccountDTO);
    Task<ResponseDTO<BankAccountDTO>> UpdateBankAccount(BankAccountDTO bankAccountDTO);
    Task<ResponseDTO<BankAccountDTO>> GetBankAccountById(int id);
    Task<ResponseDTO<BankAccountDTO>> IntegrateBankAccount(int id);
}
