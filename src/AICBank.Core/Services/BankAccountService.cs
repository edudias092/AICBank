using AICBank.Core.DTOs;
using AICBank.Core.Entities;
using AICBank.Core.Interfaces;
using AICBank.Core.Util.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace AICBank.Core.Services;

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IMapper _mapper;
    private readonly HttpContext _httpContext;
    public BankAccountService(IBankAccountRepository bankAccountRepository, IMapper mapper, IHttpContextAccessor contextAccessor)
    {
        _bankAccountRepository = bankAccountRepository;
        _mapper = mapper;
        _httpContext = contextAccessor.HttpContext ?? throw new ApplicationException("Couldn't get the httpContext.");
    }

    //TODO: Check if the bankaccount was created for the user already
    public async Task<ResponseDTO<BankAccountDTO>> CreateBankAccount(BankAccountDTO bankAccountDTO)
    {
        var userId = _httpContext.GetAccountUserId();

        bankAccountDTO.AccountUserId = int.Parse(userId);
        
        var bankAccount = _mapper.Map<BankAccount>(bankAccountDTO);
        await _bankAccountRepository.Add(bankAccount);

        return new ResponseDTO<BankAccountDTO>{
            Success = true,
            Data = bankAccountDTO
        };
    }

    public async Task<ResponseDTO<BankAccountDTO>> GetBankAccountById(int id)
    {
        var bankAccount = await _bankAccountRepository.GetById(id);

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

        var existingBankAccount = await _bankAccountRepository.GetById(bankAccountDTO.Id);

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
}
