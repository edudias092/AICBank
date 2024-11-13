using System;
using AICBank.Core.DTOs;
using AICBank.Core.DTOs.CelCash;

namespace AICBank.Core.Interfaces;

public interface ICelCashClientService
{
    Task<CelcashSubaccountResponseDTO> CreateSubBankAccount(BankAccountDTO bankAccountDTO);
    Task<CelcashSubaccountResponseDTO> SendMandatoryDocuments(CelcashSendMandatoryDocumentsDTO sendMandatoryDocumentsDTO, BankAccountDTO bankAccountDTO);
    Task<BankStatementDTO> Movements(BankAccountDTO bankAccountDTO, DateTime initialDate, DateTime finalDate);
    Task<CelcashChargeResponseDTO> CreateCharge(BankAccountDTO bankAccountDTO, ChargeDTO chargeDTO);
    Task<CelcashChargeDTO[]> GetCharges(BankAccountDTO bankAccountDTO, DateTime? initialDate, DateTime? finalDate);
    Task<CelcashChargeDTO> GetChargeById(BankAccountDTO bankAccountDTO, string chargeId);
}
