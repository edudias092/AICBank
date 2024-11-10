using System;
using AICBank.Core.DTOs;
using AICBank.Core.DTOs.CelCash;

namespace AICBank.Core.Interfaces;

public interface IBankAccountService
{
    Task<ResponseDTO<BankAccountDTO>> CreateBankAccount(BankAccountDTO bankAccountDTO);
    Task<ResponseDTO<BankAccountDTO>> UpdateBankAccount(BankAccountDTO bankAccountDTO);
    Task<ResponseDTO<BankAccountDTO>> GetBankAccountById(int id);
    Task<ResponseDTO<BankAccountDTO>> IntegrateBankAccount(int id);
    Task<ResponseDTO<BankAccountDTO>> SendMandatoryDocuments(int bankAccountId, MandatoryDocumentsDTO mandatoryDocumentsDTO);
    Task<ResponseDTO<BankAccountDTO>> GetBankAccountByAccountUserId(int accountUserId);
    Task<ResponseDTO<BankStatementDTO>> GetMovements(int bankAccountId, DateTime initialDate, DateTime finalDate);
    Task<ResponseDTO<CelcashChargeDTO>> CreateCharge(int bankAccountId, ChargeDTO chargeDTO);
    Task<ResponseDTO<CelcashChargeDTO[]>> GetCharges(int bankAccountId, DateTime? initialDate, DateTime? finalDate);
}
