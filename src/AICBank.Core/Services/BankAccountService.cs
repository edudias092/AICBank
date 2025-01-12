using System.Net.Http.Headers;
using AICBank.Core.DTOs;
using AICBank.Core.DTOs.CelCash;
using AICBank.Core.Email;
using AICBank.Core.Entities;
using AICBank.Core.Interfaces;
using AICBank.Core.Repositories;
using AICBank.Core.Util;
using AICBank.Core.Util.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using static AICBank.Core.DTOs.MandatoryDocumentsDTO;

namespace AICBank.Core.Services;

public class BankAccountService : IBankAccountService
{
    private readonly HttpContext _httpContext;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IMapper _mapper;
    private readonly ICelCashClientService _celCashClientService;
    private readonly IEmailService _emailService;
    private readonly ISplitFactory _splitFactory;
    private readonly IMandatoryDocumentsRepository _mandatoryDocumentsRepository;

    public BankAccountService(IBankAccountRepository bankAccountRepository,
        IMapper mapper,
        IHttpContextAccessor contextAccessor,
        ICelCashClientService celCashClientService,
        IEmailService emailService,
        ISplitFactory splitFactory, 
        IMandatoryDocumentsRepository mandatoryDocumentsRepository)
    {
        _bankAccountRepository = bankAccountRepository;
        _mapper = mapper;
        _celCashClientService = celCashClientService;
        _emailService = emailService;
        _httpContext = contextAccessor.HttpContext ?? throw new ApplicationException("Couldn't get the httpContext.");
        _splitFactory = splitFactory;
        _mandatoryDocumentsRepository = mandatoryDocumentsRepository;
    }

    public async Task<ResponseDTO<BankAccountDTO>> CreateBankAccount(BankAccountDTO bankAccountDto)
    {
        var userId = _httpContext.GetAccountUserId();
        var existingBankAccounts = await _bankAccountRepository
            .Get(x => x.AccountUserId.ToString() == userId);

        if (existingBankAccounts.Any())
        {
            throw new InvalidOperationException("Já existe uma conta para o seu usuário.");
        }

        bankAccountDto.AccountUserId = int.Parse(userId);
        bankAccountDto.Status = StatusBankAccount.Pending;
        
        //Todo: remove this when consolidating
        var cutLength = bankAccountDto.Name?.Length > 17 ? 17 : bankAccountDto.Name!.Length; 
        bankAccountDto.SoftDescriptor = bankAccountDto.Name
            .Substring(0, cutLength)
            .Sanitize();

        var bankAccount = _mapper.Map<BankAccount>(bankAccountDto);
        
        await _bankAccountRepository.Add(bankAccount);
        var responseDto = await SendToCelCash(bankAccountDto);

        bankAccount.GalaxHash = responseDto.Data.GalaxHash;
        bankAccount.GalaxId = responseDto.Data.GalaxId;
        bankAccount.Status = StatusBankAccount.PendingDocuments;
        
        await _bankAccountRepository.Update(bankAccount);
        bankAccountDto = _mapper.Map<BankAccountDTO>(bankAccount);

        await _emailService.SendEmailAsync(new BankAccountSavedMessageBuilder(bankAccountDto));
        
        return new ResponseDTO<BankAccountDTO>
        {
            Success = true,
            Data = bankAccountDto
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> GetBankAccountById(int id)
    {
        var bankAccount = await GetBankAccount(id);
        var bankAccountDto = _mapper.Map<BankAccountDTO>(bankAccount);
        
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
        bankAccountDto.SoftDescriptor = bankAccountDto.Name[..cutLength].Sanitize();

        var bankAccount = _mapper.Map<BankAccount>(bankAccountDto);

        if (string.IsNullOrWhiteSpace(bankAccountDto.GalaxId))
        {
            var responseDto = await SendToCelCash(bankAccountDto);

            responseDto.Data.Status = StatusBankAccount.PendingDocuments;
            bankAccount = _mapper.Map<BankAccount>(responseDto.Data);
        }

        await _bankAccountRepository.Update(bankAccount);

        return new ResponseDTO<BankAccountDTO>
        {
            Success = true,
            Data = bankAccountDto
        };
    }

    private async Task<ResponseDTO<BankAccountDTO>> SendToCelCash(BankAccountDTO bankAccountDto)
    {
        var result = await _celCashClientService.CreateSubBankAccount(bankAccountDto);

        if (result?.CelcashCompany == null || result?.Type == false)
        {
            throw new InvalidOperationException(ErrorMapper.MapErrors(result!.Error));
        }
        
        bankAccountDto.GalaxHash = result.CelcashCompany.ApiAuth.GalaxHash;
        bankAccountDto.GalaxId = result.CelcashCompany.ApiAuth.GalaxId.ToString();

        return new ResponseDTO<BankAccountDTO>()
        {
            Success = true,
            Data = bankAccountDto
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> SendMandatoryDocuments(int bankAccountId, MandatoryDocumentsDTO mandatoryDocumentsDto)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = _mapper.Map<BankAccountDTO>(existingBankAccount);
        
        mandatoryDocumentsDto.BankAccountId = bankAccountId;
        var mandatoryDocuments = _mapper.Map<MandatoryDocuments>(mandatoryDocumentsDto);
        
        if (mandatoryDocuments.Id != 0)
            await _mandatoryDocumentsRepository.Update(mandatoryDocuments);
        else
            await _mandatoryDocumentsRepository.Add(mandatoryDocuments);
        
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
        
        var result = await _celCashClientService.SendMandatoryDocuments(celcashSendMandatoryDocumentsDto, bankAccountDto);

        if (result == null || !result.Type)
            throw new InvalidOperationException(ErrorMapper.MapErrors(result?.Error));
        
        existingBankAccount.Status = StatusBankAccount.PendingAnalysis;
        await _bankAccountRepository.Update(existingBankAccount);

        bankAccountDto = _mapper.Map<BankAccountDTO>(existingBankAccount);

        return new ResponseDTO<BankAccountDTO>
        {
            Success = result.Type,
            Errors = [ErrorMapper.MapErrors(result.Error)],
            Data = bankAccountDto
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> GetBankAccountByAccountUserId(int accountUserId)
    {
        var bankAccount = await _bankAccountRepository.GetByAccountUserWithInfoAsync(accountUserId);

        if (bankAccount == null)
            return new ResponseDTO<BankAccountDTO>
            {
                Success = true,
                Errors = ["Conta não encontrada."],
                Data = null
            };
        
        if (bankAccount.AccountUserId.ToString() != _httpContext.GetAccountUserId())
        {
            throw new InvalidOperationException("Conta não pertence ao usuário.");
        }

        var bankAccountDto = _mapper.Map<BankAccountDTO>(bankAccount);
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
        var bankAccountDto = _mapper.Map<BankAccountDTO>(existingBankAccount);

        var bankStatementDto = await _celCashClientService.GetMovements(bankAccountDto, initialDate, finalDate);

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
        var bankAccountDto = _mapper.Map<BankAccountDTO>(existingBankAccount);

        chargeDto.Split = _splitFactory.CreateDefaultBoletoSplitPaymentMethod();
        var chargeResponseDto = await _celCashClientService.CreateCharge(bankAccountDto, chargeDto);

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
        var bankAccountDto = _mapper.Map<BankAccountDTO>(existingBankAccount);

        var chargeListDto = await _celCashClientService.GetCharges(bankAccountDto, initialDate, finalDate);

        return new ResponseDTO<CelcashChargeDTO[]>{
            Data = chargeListDto?.Charges,
            Success = chargeListDto is { Error: null },
            Errors = chargeListDto == null ? ["Erro ao obter cobranças. Por favor entre em contato com o suporte"] : null
        };
    }
    
    public async Task<ResponseDTO<CelcashChargeDTO>> GetChargeById(int bankAccountId, string chargeId)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = _mapper.Map<BankAccountDTO>(existingBankAccount);

        var chargeListDto = await _celCashClientService.GetChargeById(bankAccountDto, chargeId);
        
        return new ResponseDTO<CelcashChargeDTO>{
            Data = chargeListDto?.Charges?.FirstOrDefault(),
            Success = chargeListDto is { Error: null },
            Errors = chargeListDto == null ? ["Erro ao obter cobranças. Por favor entre em contato com o suporte"] : null
        };
    }
    
    public async Task<ResponseDTO<bool>> CancelCharge(int bankAccountId, string chargeId)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = _mapper.Map<BankAccountDTO>(existingBankAccount);
        
        var result = await _celCashClientService.CancelCharge(bankAccountDto, chargeId);
        
        return new ResponseDTO<bool>{
            Data = result,
            Success = result,
            Errors = !result ? ["Erro ao obter cobranças. Por favor entre em contato com o suporte"] : null
        };
    }

    public async Task<ResponseDTO<CelcashBalanceResponseDto>> GetBalance(int bankAccountId)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = _mapper.Map<BankAccountDTO>(existingBankAccount);

        var result = await _celCashClientService.GetBalance(bankAccountDto);
        
        if(result == null)
            throw new InvalidOperationException("Erro ao obter saldo. Por favor entre em contato com o suporte");
        
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
        var bankAccountDto = _mapper.Map<BankAccountDTO>(existingBankAccount);
        
        var result = await _celCashClientService.MakePayment(bankAccountDto, paymentRequest);

        if(result == null)
            throw new InvalidOperationException("Erro ao fazer transferência/pix. Por favor entre em contato com o suporte");
        
        return new ResponseDTO<CelcashPaymentResponseDto>
        {
            Data = result,
            Success = result.Type,
            Errors = !result.Type ? ["Erro ao realizar Transferencia/Pagamento. Por favor entre em contato com o suporte"] : null
        };
    }
    
    public async Task<ResponseDTO<Dictionary<string, decimal>>> GetChargesSumByDate(int bankAccountId)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = _mapper.Map<BankAccountDTO>(existingBankAccount);

        var startDate = DateTime.Today.AddDays(-7);
        var endDate = DateTime.Today.AddDays(7);
        var result = await _celCashClientService.GetCharges(bankAccountDto, startDate, endDate);
        
        var chargeSumByDate = new Dictionary<string, decimal>();
        var countOfDays = (endDate - startDate).Days;
        
        if (result.Error == null)
        {
            var groups = result.Charges.GroupBy(x => x.Transactions.FirstOrDefault()!.PayDay);
            
            for (var i = 0; i <= countOfDays; i++)
            {
                var date = startDate.AddDays(i);
                var group = groups.FirstOrDefault(x => x.Key == date);
                chargeSumByDate.Add(date.ToString("dd/MM/yyyy"), group?.Sum(x => Convert.ToDecimal(x.Value)) ?? 0m);
            }
            
            return new ResponseDTO<Dictionary<string, decimal>> { Data = chargeSumByDate, Success = true };
        }

        return new ResponseDTO<Dictionary<string, decimal>> { Data = null, Success = false };
    }
    
    public async Task<ResponseDTO<Dictionary<string, decimal>>> GetChargesSumWeekly(int bankAccountId)
    {
        var existingBankAccount = await GetBankAccount(bankAccountId);
        var bankAccountDto = _mapper.Map<BankAccountDTO>(existingBankAccount);

        var startDate = DateTime.Today.AddDays(-7);
        var endDate = DateTime.Today;
        var result = await _celCashClientService.GetCharges(bankAccountDto, startDate, endDate);
        
        var chargeSumByDate = new Dictionary<string, decimal>();
        var countOfDays = (endDate - startDate).Days;
        
        if (result.Error == null)
        {
            var groups = result.Charges.GroupBy(x => x.Transactions.FirstOrDefault()!.PayDay);
            
            for (var i = 0; i <= countOfDays; i++)
            {
                var date = startDate.AddDays(i);
                var group = groups.FirstOrDefault(x => x.Key == date);
                chargeSumByDate.Add(date.ToString("dd/MM/yyyy"), group?.Sum(x => Convert.ToDecimal(x.Value)) ?? 0m);
            }
            
            return new ResponseDTO<Dictionary<string, decimal>> { Data = chargeSumByDate, Success = true };
        }

        return new ResponseDTO<Dictionary<string, decimal>> { Data = null, Success = false };
    }

    public async Task<ResponseDTO<MandatoryDocumentsDTO>> GetMandatoryDocuments(int bankAccountId)
    {
        var mandatoryDocuments =
            await _mandatoryDocumentsRepository.GetMandatoryDocumentsByBankAccountId(bankAccountId);

        if (mandatoryDocuments == null)
        {
            return new ResponseDTO<MandatoryDocumentsDTO>
            {
                Data = null,
                Success = true
            };
        }
        var mandatoryDocumentsDto = _mapper.Map<MandatoryDocumentsDTO>(mandatoryDocuments);
        
        var filter = new CelcashFilterSubaccountDto
        {
            GalaxPayIds = [mandatoryDocuments.BankAccount.GalaxId],
            StartAt = 0,
            Limit = 1
        };

        var celcashBankAccountList = await _celCashClientService.GetSubaccountList(filter);

        if (celcashBankAccountList != null && celcashBankAccountList.Subaccounts.Any())
        {
            var subAccount = celcashBankAccountList.Subaccounts.First();
            mandatoryDocumentsDto.ReasonsStatus = subAccount.Verification.Reasons;
            mandatoryDocumentsDto.StatusIntegration = subAccount.Verification.Status;
        }
        
        return new ResponseDTO<MandatoryDocumentsDTO> { Data = mandatoryDocumentsDto, Success = true };
    }

    public async Task<bool> CheckSubaccountStatus(string galaxId,  bool approved)
    {
        var bankAccount = await _bankAccountRepository.GetBankAccountWithInfoByGalaxIdAsync(galaxId);
        
        bankAccount.Status = 
            approved ? StatusBankAccount.Activated 
                : (bankAccount.Status == StatusBankAccount.PendingAnalysis) 
                    ? StatusBankAccount.PendingDocuments
                    : bankAccount.Status;

        var bankAccountDto = _mapper.Map<BankAccountDTO>(bankAccount);
        IEmailMessageBuilder messageBuilder = approved
            ? new BankAccountApprovedMessageBuilder(bankAccountDto)
            : new BankAccountReprovedMessageBuilder(bankAccountDto);
        
        await _bankAccountRepository.Update(bankAccount);
        await _emailService.SendEmailAsync(messageBuilder);
        
        return true;
    }

    private async Task<BankAccount> GetBankAccount(int bankAccountId)
    {
        var existingBankAccount = await _bankAccountRepository.GetBankAccountWithInfoByIdAsync(bankAccountId);

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
