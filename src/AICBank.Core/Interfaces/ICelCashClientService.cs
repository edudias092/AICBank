using System;
using AICBank.Core.DTOs;
using AICBank.Core.DTOs.CelCash;
using AICBank.Core.Services;

namespace AICBank.Core.Interfaces;

public interface ICelCashClientService
{
    Task<CelcashCreatedSubaccountResponseDTO> CreateSubBankAccount(BankAccountDTO bankAccountDto);
    Task<CelcashCreatedSubaccountResponseDTO> SendMandatoryDocuments(CelcashSendMandatoryDocumentsDTO sendMandatoryDocumentsDto, BankAccountDTO bankAccountDto);
    Task<BankStatementDTO> Movements(BankAccountDTO bankAccountDto, DateTime initialDate, DateTime finalDate);
    Task<CelcashChargeResponseDTO> CreateCharge(BankAccountDTO bankAccountDto, ChargeDTO chargeDto);
    Task<CelcashListChargeResponseDTO> GetCharges(BankAccountDTO bankAccountDto, DateTime? initialDate, DateTime? finalDate);
    Task<CelcashListChargeResponseDTO> GetChargeById(BankAccountDTO bankAccountDto, string chargeId);
    Task<bool> CancelCharge(BankAccountDTO bankAccountDto, string chargeId);
    Task<CelcashBalanceResponseDto> GetBalance(BankAccountDTO bankAccountDto);
    Task<CelcashPaymentResponseDto> MakePayment(BankAccountDTO bankAccountDto, CelcashPaymentRequestDto paymentRequest);

    Task<CelcashListSubaccountResponseDto> GetSubaccountList(CelcashFilterSubaccountDto filterSubaccountDto);
}
