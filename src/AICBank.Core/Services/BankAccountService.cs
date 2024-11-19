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

    public async Task<ResponseDTO<BankAccountDTO>> CreateBankAccount(BankAccountDTO bankAccountDTO)
    {
        var userId = _httpContext.GetAccountUserId();

        var existingBankAccounts = await bankAccountRepository.Get(x => x.AccountUserId.ToString() == userId);

        if (existingBankAccounts.Any())
        {
            throw new InvalidOperationException("Já existe uma conta para o seu usuário.");
        }

        bankAccountDTO.AccountUserId = int.Parse(userId);
        bankAccountDTO.Status = StatusBankAccount.Pending;
        
        //Todo: remove this when consolidating
        var cutLength = bankAccountDTO.Name?.Length > 17 ? 17 : bankAccountDTO.Name.Length; 
        bankAccountDTO.SoftDescriptor = bankAccountDTO.Name.Substring(0, cutLength);

        var bankAccount = mapper.Map<BankAccount>(bankAccountDTO);
        
        await bankAccountRepository.Add(bankAccount);


        bankAccountDTO = mapper.Map<BankAccountDTO>(bankAccount);

        return new ResponseDTO<BankAccountDTO>
        {
            Success = true,
            Data = bankAccountDTO
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> GetBankAccountById(int id)
    {
        var bankAccount = await GetBankAccount(id);
        var bankAccountDTO = mapper.Map<BankAccountDTO>(bankAccount);
        
        return new ResponseDTO<BankAccountDTO>
        {
            Success = true,
            Errors = [],
            Data = bankAccountDTO
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> UpdateBankAccount(BankAccountDTO bankAccountDTO)
    {
        //Todo: remove this when consolidating
        var cutLength = bankAccountDTO.Name?.Length > 17 ? 17 : bankAccountDTO.Name.Length; 
        bankAccountDTO.SoftDescriptor = bankAccountDTO.Name[..cutLength].RemoverAcentos();

        var bankAccount = mapper.Map<BankAccount>(bankAccountDTO);

        _ = await GetBankAccount(bankAccountDTO.Id);

        if (string.IsNullOrWhiteSpace(bankAccountDTO.GalaxId))
        {
            var responseDto = await SendToCelCash(bankAccountDTO);
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
            Data = bankAccountDTO
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
        var bankAccountDTO = mapper.Map<BankAccountDTO>(bankAccount);

        var response = await SendToCelCash(bankAccountDTO);

        if (response.Success)
        {
            bankAccount = mapper.Map<BankAccount>(response.Data);
            await bankAccountRepository.Update(bankAccount);
        }

        return response;
    }

    private async Task<ResponseDTO<BankAccountDTO>> SendToCelCash(BankAccountDTO bankAccountDTO)
    {
        var result = await celCashClientService.CreateSubBankAccount(bankAccountDTO);

        if (result.Type && result.Company != null)
        {
            bankAccountDTO.GalaxHash = result.Company.ApiAuth.GalaxHash;
            bankAccountDTO.GalaxId = result.Company.ApiAuth.GalaxId.ToString();

            return new ResponseDTO<BankAccountDTO>()
            {
                Success = true,
                Data = bankAccountDTO
            };
        }

        return new ResponseDTO<BankAccountDTO>()
        {
            Success = false,
            Data = null,
            Errors = [ErrorMapper.MapErrors(result.Error)],
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> SendMandatoryDocuments(int bankAccountId, MandatoryDocumentsDTO mandatoryDocumentsDTO)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = mapper.Map<BankAccountDTO>(existingBankAccount);

        var celcashSendMandatoryDocumentsDTO =
            CelcashSendMandatoryDocumentsDTO.FromMandatoryDocumentsDto(mandatoryDocumentsDTO);

        if (mandatoryDocumentsDTO.Type == DocumentType.CNH)
        {
            celcashSendMandatoryDocumentsDTO.Documents.Personal.CNH = new CNHDTO
            {
                Selfie = await ConvertToBase64(mandatoryDocumentsDTO.Selfie),
                Picture =
                [
                    await ConvertToBase64(mandatoryDocumentsDTO.Front),
                    await ConvertToBase64(mandatoryDocumentsDTO.Back, false),
                ],
                Address = await ConvertToBase64(mandatoryDocumentsDTO.Address)
            };
        }

        if (mandatoryDocumentsDTO.Type == DocumentType.RG)
        {
            celcashSendMandatoryDocumentsDTO.Documents.Personal.RG = new RGDTO
            {
                Selfie = await ConvertToBase64(mandatoryDocumentsDTO.Selfie),
                Front = await ConvertToBase64(mandatoryDocumentsDTO.Front),
                Back = await ConvertToBase64(mandatoryDocumentsDTO.Back, false),
                Address = await ConvertToBase64(mandatoryDocumentsDTO.Address)
            };
        }
        
        var result = await celCashClientService.SendMandatoryDocuments(celcashSendMandatoryDocumentsDTO, bankAccountDto);

        if (result.Type)
        {
            existingBankAccount.Status = StatusBankAccount.PendingAnalysis;
            await bankAccountRepository.Update(existingBankAccount);

            bankAccountDto = mapper.Map<BankAccountDTO>(existingBankAccount);
        }

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

        if (bankAccount != null)
        {
            if (bankAccount.AccountUserId.ToString() != _httpContext.GetAccountUserId())
            {
                throw new InvalidOperationException("Conta não pertence ao usuário.");
            }

            var bankAccountDTO = mapper.Map<BankAccountDTO>(bankAccount);
            return new ResponseDTO<BankAccountDTO>
            {
                Success = true,
                Errors = [],
                Data = bankAccountDTO
            };
        }
        else
        {
            return new ResponseDTO<BankAccountDTO>
            {
                Success = true,
                Errors = [],
                Data = null
            };
        }


    }

    public async Task<ResponseDTO<BankStatementDTO>> GetMovements(int bankAccountId, DateTime initialDate, DateTime finalDate)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = mapper.Map<BankAccountDTO>(existingBankAccount);

        var bankStatementDTO = await celCashClientService.Movements(bankAccountDto, initialDate, finalDate);

        return new ResponseDTO<BankStatementDTO>{
            Data = bankStatementDTO,
            Success = bankStatementDTO.Error == null,
            Errors = bankStatementDTO.Error?.Details != null 
                        ? [string.Join(", ", bankStatementDTO.Error?.Details)] 
                        : []
        };
    }

    public async Task<ResponseDTO<CelcashChargeDTO>> CreateCharge(int bankAccountId, ChargeDTO chargeDTO)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = mapper.Map<BankAccountDTO>(existingBankAccount);

        var chargeResponseDTO = await celCashClientService.CreateCharge(bankAccountDto, chargeDTO);

        return new ResponseDTO<CelcashChargeDTO>{
            Data = chargeResponseDTO.Charge,
            Success = chargeResponseDTO.Error == null,
            Errors = chargeResponseDTO.Error?.Details != null 
                        ? [string.Join(", ", chargeResponseDTO.Error?.Details)] 
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
            var groups = result.Charges.GroupBy(x => x.Transactions.FirstOrDefault().PayDay);
            
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

        using var memoryStream = new MemoryStream();

        await formFile.CopyToAsync(memoryStream);

        var contentBytes = memoryStream.ToArray();

        return Convert.ToBase64String(contentBytes);
    }
}
