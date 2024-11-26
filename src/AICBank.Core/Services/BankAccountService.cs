using System.Net.Http.Headers;
using AICBank.Core.DTOs;
using AICBank.Core.DTOs.CelCash;
using AICBank.Core.Entities;
using AICBank.Core.Interfaces;
using AICBank.Core.Util;
using AICBank.Core.Util.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using static AICBank.Core.DTOs.MandatoryDocumentsDTO;

namespace AICBank.Core.Services;

public class BankAccountService(
    IBankAccountRepository bankAccountRepository,
    IMapper mapper,
    IHttpContextAccessor contextAccessor,
    ICelCashClientService celCashClientService)
    : IBankAccountService
{
    private readonly HttpContext _httpContext = contextAccessor.HttpContext ?? throw new ApplicationException("Couldn't get the httpContext.");

    public async Task<ResponseDTO<BankAccountDTO>> CreateBankAccount(BankAccountDTO bankAccountDto)
    {
        var userId = _httpContext.GetAccountUserId();

        var existingBankAccounts = await bankAccountRepository.Get(x => x.AccountUserId.ToString() == userId);

        if (existingBankAccounts.Any())
        {
            throw new InvalidOperationException("Já existe uma conta para o seu usuário.");
        }

        bankAccountDto.AccountUserId = int.Parse(userId);
        bankAccountDto.Status = StatusBankAccount.Pending;
        
        //Todo: remove this when consolidating
        var cutLength = bankAccountDto.Name?.Length > 17 ? 17 : bankAccountDto.Name!.Length; 
        bankAccountDto.SoftDescriptor = bankAccountDto.Name.Substring(0, cutLength).RemoverAcentos();

        var bankAccount = mapper.Map<BankAccount>(bankAccountDto);
        
        await bankAccountRepository.Add(bankAccount);
        
        var responseDto = await SendToCelCash(bankAccountDto);
        if (!responseDto.Success)
        {
            return responseDto;
        }

        bankAccount.GalaxHash = responseDto.Data.GalaxHash;
        bankAccount.GalaxId = responseDto.Data.GalaxId;
        bankAccount.Status = StatusBankAccount.PendingDocuments;
        
        await bankAccountRepository.Update(bankAccount);
        
        bankAccountDto = mapper.Map<BankAccountDTO>(bankAccount);

        return new ResponseDTO<BankAccountDTO>
        {
            Success = true,
            Data = bankAccountDto
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> GetBankAccountById(int id)
    {
        var bankAccount = await GetBankAccount(id);
        var bankAccountDto = mapper.Map<BankAccountDTO>(bankAccount);
        
        return new ResponseDTO<BankAccountDTO>
        {
            Success = true,
            Errors = [],
            Data = bankAccountDto
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> UpdateBankAccount(BankAccountDTO bankAccountDto)
    {
        //Todo: remove this when consolidating
        var cutLength = bankAccountDto.Name?.Length > 17 ? 17 : bankAccountDto.Name!.Length; 
        bankAccountDto.SoftDescriptor = bankAccountDto.Name[..cutLength].RemoverAcentos();

        var bankAccount = mapper.Map<BankAccount>(bankAccountDto);

        if (string.IsNullOrWhiteSpace(bankAccountDto.GalaxId))
        {
            var responseDto = await SendToCelCash(bankAccountDto);
            if (!responseDto.Success)
            {
                return responseDto;
            }

            responseDto.Data.Status = StatusBankAccount.PendingDocuments;
            bankAccount = mapper.Map<BankAccount>(responseDto.Data);
        }

        await bankAccountRepository.Update(bankAccount);

        return new ResponseDTO<BankAccountDTO>
        {
            Success = true,
            Data = bankAccountDto
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> IntegrateBankAccount(int id)
    {
        var bankAccount = await bankAccountRepository.GetBankAccountWithInfoByIdAsync(id);

        if (bankAccount == null)
        {
            throw new InvalidOperationException("Conta não encontrada.");
        }

        if (bankAccount.AccountUserId.ToString() != _httpContext.GetAccountUserId())
        {
            throw new InvalidOperationException("Conta não pertence ao usuário.");
        }

        if (!string.IsNullOrWhiteSpace(bankAccount.GalaxHash))
        {
            throw new InvalidOperationException("Conta já existe no serviço");
        }
        var bankAccountDto = mapper.Map<BankAccountDTO>(bankAccount);

        var response = await SendToCelCash(bankAccountDto);

        if (!response.Success) return response;
        
        bankAccount = mapper.Map<BankAccount>(response.Data);
        await bankAccountRepository.Update(bankAccount);

        return response;
    }

    private async Task<ResponseDTO<BankAccountDTO>> SendToCelCash(BankAccountDTO bankAccountDto)
    {
        var result = await celCashClientService.CreateSubBankAccount(bankAccountDto);

        if (!result.Type || result.Company == null)
            return new ResponseDTO<BankAccountDTO>()
            {
                Success = false,
                Data = null,
                Errors = [ErrorMapper.MapErrors(result.Error)],
            };
        
        bankAccountDto.GalaxHash = result.Company.ApiAuth.GalaxHash;
        bankAccountDto.GalaxId = result.Company.ApiAuth.GalaxId.ToString();

        return new ResponseDTO<BankAccountDTO>()
        {
            Success = true,
            Data = bankAccountDto
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> SendMandatoryDocuments(int bankAccountId, MandatoryDocumentsDTO mandatoryDocumentsDto)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = mapper.Map<BankAccountDTO>(existingBankAccount);

        var celcashSendMandatoryDocumentsDto =
            CelcashSendMandatoryDocumentsDTO.FromMandatoryDocumentsDto(mandatoryDocumentsDto);

        switch (mandatoryDocumentsDto.Type)
        {
            case DocumentType.Cnh:
                celcashSendMandatoryDocumentsDto.Documents.Personal.CNH = new CNHDTO
                {
                    Selfie = await ConvertToBase64(mandatoryDocumentsDto.Selfie),
                    Picture =
                    [
                        await ConvertToBase64(mandatoryDocumentsDto.Front),
                        await ConvertToBase64(mandatoryDocumentsDto.Back, false),
                    ],
                    Address = await ConvertToBase64(mandatoryDocumentsDto.Address)
                };
                break;
            case DocumentType.Rg:
                celcashSendMandatoryDocumentsDto.Documents.Personal.RG = new RGDTO
                {
                    Selfie = await ConvertToBase64(mandatoryDocumentsDto.Selfie),
                    Front = await ConvertToBase64(mandatoryDocumentsDto.Front),
                    Back = await ConvertToBase64(mandatoryDocumentsDto.Back, false),
                    Address = await ConvertToBase64(mandatoryDocumentsDto.Address)
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mandatoryDocumentsDto.Type));
        }
        celcashSendMandatoryDocumentsDto.Documents.Company.LastContract = await ConvertToBase64(mandatoryDocumentsDto.LastContract, false);
        celcashSendMandatoryDocumentsDto.Documents.Company.CnpjCard = await ConvertToBase64(mandatoryDocumentsDto.CnpjCard, false);
        celcashSendMandatoryDocumentsDto.Documents.Company.ElectionRecord = await ConvertToBase64(mandatoryDocumentsDto.ElectionRecord, false);
        celcashSendMandatoryDocumentsDto.Documents.Company.Statute = await ConvertToBase64(mandatoryDocumentsDto.Statute, false);
        
        var result = await celCashClientService.SendMandatoryDocuments(celcashSendMandatoryDocumentsDto, bankAccountDto);

        if (result == null || !result.Type)
            return new ResponseDTO<BankAccountDTO>
            {
                Success = false,
                Errors = [ErrorMapper.MapErrors(result?.Error)],
                Data = bankAccountDto
            };
        
        existingBankAccount.Status = StatusBankAccount.PendingAnalysis;
        await bankAccountRepository.Update(existingBankAccount);

        bankAccountDto = mapper.Map<BankAccountDTO>(existingBankAccount);

        return new ResponseDTO<BankAccountDTO>
        {
            Success = result.Type,
            Errors = [ErrorMapper.MapErrors(result.Error)],
            Data = bankAccountDto
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> GetBankAccountByAccountUserId(int accountUserId)
    {
        var bankAccount = await bankAccountRepository.GetByAccountUserWithInfoAsync(accountUserId);

        if (bankAccount == null)
            return new ResponseDTO<BankAccountDTO>
            {
                Success = true,
                Errors = [],
                Data = null
            };
        
        if (bankAccount.AccountUserId.ToString() != _httpContext.GetAccountUserId())
        {
            throw new InvalidOperationException("Conta não pertence ao usuário.");
        }

        var bankAccountDto = mapper.Map<BankAccountDTO>(bankAccount);
        return new ResponseDTO<BankAccountDTO>
        {
            Success = true,
            Errors = [],
            Data = bankAccountDto
        };

    }

    public async Task<ResponseDTO<BankStatementDTO>> GetMovements(int bankAccountId, DateTime initialDate, DateTime finalDate)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = mapper.Map<BankAccountDTO>(existingBankAccount);

        var bankStatementDto = await celCashClientService.Movements(bankAccountDto, initialDate, finalDate);

        return new ResponseDTO<BankStatementDTO>{
            Data = bankStatementDto,
            Success = bankStatementDto.Error == null,
            Errors = bankStatementDto.Error?.Details != null 
                        ? [string.Join(", ", bankStatementDto.Error?.Details!)] 
                        : []
        };
    }

    public async Task<ResponseDTO<CelcashChargeDTO>> CreateCharge(int bankAccountId, ChargeDTO chargeDto)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = mapper.Map<BankAccountDTO>(existingBankAccount);

        var chargeResponseDto = await celCashClientService.CreateCharge(bankAccountDto, chargeDto);

        return new ResponseDTO<CelcashChargeDTO>{
            Data = chargeResponseDto.Charge,
            Success = chargeResponseDto.Error == null,
            Errors = chargeResponseDto.Error?.Details != null 
                        ? [string.Join(", ", chargeResponseDto.Error?.Details!)] 
                        : []
        };
    }

    public async Task<ResponseDTO<CelcashChargeDTO[]>> GetCharges(int bankAccountId, DateTime? initialDate, DateTime? finalDate)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = mapper.Map<BankAccountDTO>(existingBankAccount);

        var chargeListDto = await celCashClientService.GetCharges(bankAccountDto, initialDate, finalDate);

        return new ResponseDTO<CelcashChargeDTO[]>{
            Data = chargeListDto?.Charges,
            Success = chargeListDto is { Error: null },
            Errors = chargeListDto == null ? ["Erro ao obter cobranças. Por favor entre em contato com o suporte"] : null
        };
    }
    
    public async Task<ResponseDTO<CelcashChargeDTO>> GetChargeById(int bankAccountId, string chargeId)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = mapper.Map<BankAccountDTO>(existingBankAccount);

        var chargeListDto = await celCashClientService.GetChargeById(bankAccountDto, chargeId);
        
        return new ResponseDTO<CelcashChargeDTO>{
            Data = chargeListDto?.Charges?.FirstOrDefault(),
            Success = chargeListDto is { Error: null },
            Errors = chargeListDto == null ? ["Erro ao obter cobranças. Por favor entre em contato com o suporte"] : null
        };
    }
    
    public async Task<ResponseDTO<bool>> CancelCharge(int bankAccountId, string chargeId)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = mapper.Map<BankAccountDTO>(existingBankAccount);
        
        var result = await celCashClientService.CancelCharge(bankAccountDto, chargeId);
        
        return new ResponseDTO<bool>{
            Data = result,
            Success = result,
            Errors = !result ? ["Erro ao obter cobranças. Por favor entre em contato com o suporte"] : null
        };
    }

    public async Task<ResponseDTO<CelcashBalanceResponseDto>> GetBalance(int bankAccountId)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = mapper.Map<BankAccountDTO>(existingBankAccount);

        var result = await celCashClientService.GetBalance(bankAccountDto);

        return new ResponseDTO<CelcashBalanceResponseDto>
        {
            Data = result,
            Success = result.Type,
            Errors = !result.Type ? ["Erro ao obter saldo. Por favor entre em contato com o suporte"] : null
        };
    }

    public async Task<ResponseDTO<CelcashPaymentResponseDto>> MakePayment(int bankAccountId, CelcashPaymentRequestDto paymentRequest)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = mapper.Map<BankAccountDTO>(existingBankAccount);
        
        var result = await celCashClientService.MakePayment(bankAccountDto, paymentRequest);

        return new ResponseDTO<CelcashPaymentResponseDto>
        {
            Data = result,
            Success = result.Type,
            Errors = !result.Type ? ["Erro ao realizar Transferencia/Pagamento. Por favor entre em contato com o suporte"] : null
        };
    }
    
    public async Task<ResponseDTO<IEnumerable<IGrouping<DateTime, CelcashChargeDTO>>>> GetChargesGroup(int bankAccountId)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = mapper.Map<BankAccountDTO>(existingBankAccount);
        
        var result = await celCashClientService.GetCharges(bankAccountDto, DateTime.Today.AddDays(-7), DateTime.Today);
        var chargeSumByDate = new Dictionary<string, decimal>();
        if (result.Error == null)
        {
            var groups = result.Charges.GroupBy(x => x.Transactions.FirstOrDefault()!.PayDay);
            
            return new ResponseDTO<IEnumerable<IGrouping<DateTime, CelcashChargeDTO>>>() { Data = groups, Success = true };
        }

        return new ResponseDTO<IEnumerable<IGrouping<DateTime, CelcashChargeDTO>>>() { Data = null, Success = false };
    }
    
    private async Task<BankAccount> GetBankAccount(int bankAccountId)
    {
        var existingBankAccount = await bankAccountRepository.GetBankAccountWithInfoByIdAsync(bankAccountId);

        if (existingBankAccount == null
            || existingBankAccount.AccountUserId.ToString() != _httpContext.GetAccountUserId())
        {
            throw new InvalidOperationException("Conta não encontrada para esse usuário.");
        }

        return existingBankAccount;
    }
    
    private static async Task<string> ConvertToBase64(IFormFile formFile, bool validate = true)
    {
        if (validate && (formFile == null || formFile.Length == 0))
            throw new InvalidOperationException("Arquivo inválido.");
        
        if(formFile == null || formFile.Length == 0)
            return null;
        
        using var memoryStream = new MemoryStream();

        await formFile.CopyToAsync(memoryStream);

        var contentBytes = memoryStream.ToArray();

        return Convert.ToBase64String(contentBytes);
    }
}
