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

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IMapper _mapper;
    private readonly HttpContext _httpContext;
    private readonly ICelCashClientService _celCashClientService;
    public BankAccountService(IBankAccountRepository bankAccountRepository, IMapper mapper, IHttpContextAccessor contextAccessor, ICelCashClientService celCashClientService)
    {
        _bankAccountRepository = bankAccountRepository;
        _mapper = mapper;
        _celCashClientService = celCashClientService;
        _httpContext = contextAccessor.HttpContext ?? throw new ApplicationException("Couldn't get the httpContext.");
    }

    public async Task<ResponseDTO<BankAccountDTO>> CreateBankAccount(BankAccountDTO bankAccountDTO)
    {
        var userId = _httpContext.GetAccountUserId();

        var existingBankAccounts = await _bankAccountRepository.Get(x => x.AccountUserId.ToString() == userId);

        if (existingBankAccounts.Any())
        {
            throw new InvalidOperationException("Já existe uma conta para o seu usuário.");
        }

        bankAccountDTO.AccountUserId = int.Parse(userId);
        bankAccountDTO.Status = StatusBankAccount.Pending;
        
        //Todo: remove this when consolidating
        var cutLength = bankAccountDTO.Name?.Length > 17 ? 17 : bankAccountDTO.Name.Length; 
        bankAccountDTO.SoftDescriptor = bankAccountDTO.Name.Substring(0, cutLength);

        var bankAccount = _mapper.Map<BankAccount>(bankAccountDTO);
        
        await _bankAccountRepository.Add(bankAccount);


        bankAccountDTO = _mapper.Map<BankAccountDTO>(bankAccount);

        return new ResponseDTO<BankAccountDTO>
        {
            Success = true,
            Data = bankAccountDTO
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> GetBankAccountById(int id)
    {
        var bankAccount = await _bankAccountRepository.GetBankAccountWithInfoByIdAsync(id);

        if (bankAccount.AccountUserId.ToString() != _httpContext.GetAccountUserId())
        {
            throw new InvalidOperationException("Conta não pertence ao usuário.");
        }

        var bankAccountDTO = _mapper.Map<BankAccountDTO>(bankAccount);

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
        bankAccountDTO.SoftDescriptor = bankAccountDTO.Name.Substring(0, cutLength).RemoverAcentos();

        var bankAccount = _mapper.Map<BankAccount>(bankAccountDTO);

        var existingBankAccount = await _bankAccountRepository.GetBankAccountWithInfoByIdAsync(bankAccountDTO.Id);

        if (existingBankAccount == null
            || existingBankAccount.AccountUserId.ToString() != _httpContext.GetAccountUserId())
        {
            throw new InvalidOperationException("Conta não encontrada para esse usuário.");
        }

        if (string.IsNullOrWhiteSpace(bankAccountDTO.GalaxId))
        {
            var responseDto = await SendToCelCash(bankAccountDTO);
            if (!responseDto.Success)
            {
                return responseDto;
            }

            responseDto.Data.Status = StatusBankAccount.PendingDocuments;
            bankAccount = _mapper.Map<BankAccount>(responseDto.Data);
        }

        await _bankAccountRepository.Update(bankAccount);

        return new ResponseDTO<BankAccountDTO>
        {
            Success = true,
            Data = bankAccountDTO
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> IntegrateBankAccount(int id)
    {
        var bankAccount = await _bankAccountRepository.GetBankAccountWithInfoByIdAsync(id);

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
        var bankAccountDTO = _mapper.Map<BankAccountDTO>(bankAccount);

        var response = await SendToCelCash(bankAccountDTO);

        if (response.Success)
        {
            bankAccount = _mapper.Map<BankAccount>(response.Data);
            await _bankAccountRepository.Update(bankAccount);
        }

        return response;
    }

    private async Task<ResponseDTO<BankAccountDTO>> SendToCelCash(BankAccountDTO bankAccountDTO)
    {
        var result = await _celCashClientService.CreateSubBankAccount(bankAccountDTO);

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
        //Create CelcashDTO
        var celcashSendMandatoryDocumentsDTO = new CelcashSendMandatoryDocumentsDTO
        {
            Fields = new FieldsDTO
            {
                About = mandatoryDocumentsDTO.About,
                BirthDate = mandatoryDocumentsDTO.BirthDate,
                MonthlyIncome = mandatoryDocumentsDTO.MonthlyIncome,
                MotherName = mandatoryDocumentsDTO.MotherName,
                SocialMediaLink = mandatoryDocumentsDTO.SocialMediaLink
            },
            Documents = new DocumentsDTO
            {
                Personal = new PersonalDocumentsDTO()
            }
        };

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

        var existingBankAccount = await _bankAccountRepository.GetBankAccountWithInfoByIdAsync(bankAccountId);

        if (existingBankAccount == null
            || existingBankAccount.AccountUserId.ToString() != _httpContext.GetAccountUserId())
        {
            throw new InvalidOperationException("Conta não encontrada para esse usuário.");
        }

        var bankAccountDTO = _mapper.Map<BankAccountDTO>(existingBankAccount);

        var result = await _celCashClientService.SendMandatoryDocuments(celcashSendMandatoryDocumentsDTO, bankAccountDTO);

        if (result.Type)
        {
            existingBankAccount.Status = StatusBankAccount.PendingAnalysis;
            await _bankAccountRepository.Update(existingBankAccount);

            bankAccountDTO = _mapper.Map<BankAccountDTO>(existingBankAccount);
        }

        return new ResponseDTO<BankAccountDTO>
        {
            Success = result.Type,
            Errors = [ErrorMapper.MapErrors(result.Error)],
            Data = bankAccountDTO
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> GetBankAccountByAccountUserId(int accountUserId)
    {
        var bankAccount = await _bankAccountRepository.GetByAccountUserWithInfoAsync(accountUserId);

        if (bankAccount != null)
        {
            if (bankAccount.AccountUserId.ToString() != _httpContext.GetAccountUserId())
            {
                throw new InvalidOperationException("Conta não pertence ao usuário.");
            }

            var bankAccountDTO = _mapper.Map<BankAccountDTO>(bankAccount);
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
        var existingBankAccount = await _bankAccountRepository.GetBankAccountWithInfoByIdAsync(bankAccountId);

        if (existingBankAccount == null
            || existingBankAccount.AccountUserId.ToString() != _httpContext.GetAccountUserId())
        {
            throw new InvalidOperationException("Conta não encontrada para esse usuário.");
        }

        var bankAccountDTO = _mapper.Map<BankAccountDTO>(existingBankAccount);

        var bankStatementDTO = await _celCashClientService.Movements(bankAccountDTO, initialDate, finalDate);

        return new ResponseDTO<BankStatementDTO>{
            Data = bankStatementDTO,
            Success = bankStatementDTO.Error == null,
            Errors = bankStatementDTO.Error?.Details != null 
                        ? [string.Join(", ", bankStatementDTO.Error?.Details)] 
                        : []
        };
    }

    
    private async Task<string> ConvertToBase64(IFormFile formFile, bool validate = true)
    {
        if (validate && (formFile == null || formFile.Length == 0))
            throw new InvalidOperationException("Arquivo inválido.");

        using var memoryStream = new MemoryStream();

        await formFile.CopyToAsync(memoryStream);

        var contentBytes = memoryStream.ToArray();

        return Convert.ToBase64String(contentBytes);
    }
}
