using System;
using AICBank.Core.DTOs;
using AICBank.Core.DTOs.CelCash;
using AICBank.Core.Services;

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
    Task<ResponseDTO<CelcashChargeDTO>> GetChargeById(int bankAccountId, string chargeId);
    Task<ResponseDTO<bool>> CancelCharge(int bankAccountId, string chargeId);
    Task<ResponseDTO<CelcashBalanceResponseDto>> GetBalance(int bankAccountId);
    Task<ResponseDTO<CelcashPaymentResponseDto>> MakePayment(int bankAccountId, CelcashPaymentRequestDto paymentRequest);
    Task<ResponseDTO<IEnumerable<IGrouping<DateTime, CelcashChargeDTO>>>> GetChargesGroup(int bankAccountId);
}
