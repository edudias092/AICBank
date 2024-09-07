using AICBank.Core.DTOs;
using AICBank.Core.Entities;
using AICBank.Core.Interfaces;
using AICBank.Core.Util.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

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

    //TODO: Check if the bankaccount was created for the user already
    public async Task<ResponseDTO<BankAccountDTO>> CreateBankAccount(BankAccountDTO bankAccountDTO)
    {
        var userId = _httpContext.GetAccountUserId();

        var existingBankAccounts = await _bankAccountRepository.Get(x => x.AccountUserId.ToString() == userId);
        
        if(existingBankAccounts.Any())
        {
            throw new InvalidOperationException("Já existe uma conta para o seu usuário.");
        }
        
        bankAccountDTO.AccountUserId = int.Parse(userId);
        var bankAccount = _mapper.Map<BankAccount>(bankAccountDTO);
        await _bankAccountRepository.Add(bankAccount);

        bankAccountDTO = _mapper.Map<BankAccountDTO>(bankAccount);
        
        return new ResponseDTO<BankAccountDTO>{
            Success = true,
            Data = bankAccountDTO
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> GetBankAccountById(int id)
    {
        var bankAccount = await _bankAccountRepository.GetBankAccountWithInfoByIdAsync(id);

        if(bankAccount.AccountUserId.ToString() != _httpContext.GetAccountUserId())
        {
            throw new InvalidOperationException("Conta não pertence ao usuário.");
        }

        var bankAccountDTO = _mapper.Map<BankAccountDTO>(bankAccount);

        return new ResponseDTO<BankAccountDTO>{
            Success = true,
            Errors = [],
            Data = bankAccountDTO
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> UpdateBankAccount(BankAccountDTO bankAccountDTO)
    {
        var bankAccount = _mapper.Map<BankAccount>(bankAccountDTO);

        var existingBankAccount = await _bankAccountRepository.GetBankAccountWithInfoByIdAsync(bankAccountDTO.Id);

        if(existingBankAccount == null 
            || existingBankAccount.AccountUserId.ToString() != _httpContext.GetAccountUserId())
        {
            throw new InvalidOperationException("Conta não encontrada para esse usuário.");
        }

        await _bankAccountRepository.Update(bankAccount);

        return new ResponseDTO<BankAccountDTO>{
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
        
        if(bankAccount.AccountUserId.ToString() != _httpContext.GetAccountUserId())
        {
            throw new InvalidOperationException("Conta não pertence ao usuário.");
        }

        if (!string.IsNullOrWhiteSpace(bankAccount.GalaxHash))
        {
            throw new InvalidOperationException("Conta já existe no serviço");
        }
        var bankAccountDTO = _mapper.Map<BankAccountDTO>(bankAccount);
        var result = await _celCashClientService.CreateSubBankAccount(bankAccountDTO);

        if (result.Type && result.Company != null)
        {
            bankAccountDTO.GalaxHash = result.Company.GalaxHash;
            bankAccountDTO.GalaxId = result.Company.GalaxId;

            await _bankAccountRepository.Update(bankAccount);
            
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
            Errors = [result.Error.Message],
        };
    }
}
