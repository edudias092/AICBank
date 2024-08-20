using System;
using AICBank.Core.DTOs;
using AICBank.Core.Entities;
using AutoMapper;

namespace AICBank.Core.Mapping;

public class GlobalMappingProfile : Profile
{
    public GlobalMappingProfile()
    {
        CreateMap<BankAccount, BankAccountDTO>().ReverseMap();
        CreateMap<AccountUser, AccountUserDTO>().ReverseMap();
        CreateMap<Professional, ProfessionalDTO>().ReverseMap();
        CreateMap<Address, AddressDTO>().ReverseMap();
    }
}
