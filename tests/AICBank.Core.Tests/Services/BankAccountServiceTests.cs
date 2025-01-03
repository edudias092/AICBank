using System.Linq.Expressions;
using System.Security.Claims;
using AICBank.Core.DTOs;
using AICBank.Core.DTOs.CelCash;
using AICBank.Core.Email;
using AICBank.Core.Entities;
using AICBank.Core.Interfaces;
using AICBank.Core.Mapping;
using AICBank.Core.Services;
using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Http;

namespace AICBank.Core.Tests.Services;

public class BankAccountServiceTests
{
    private BankAccountService _sut;
    private IBankAccountRepository _bankAccountRepository;
    private IHttpContextAccessor _httpContextAccessor;
    private ICelCashClientService _celCashClientService;
    private IMapper _mapper;
    private IEmailService _emailService;
    private HttpContext _httpContext;
    
    public BankAccountServiceTests()
    {
        _bankAccountRepository = A.Fake<IBankAccountRepository>();
        
        _celCashClientService = A.Fake<ICelCashClientService>();
        _mapper = new MapperConfiguration(cfg => 
                cfg.AddProfile<GlobalMappingProfile>())
            .CreateMapper();
        _emailService = A.Fake<IEmailService>();
        
        _httpContextAccessor = new HttpContextAccessor()
        {
            HttpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new ("AccountUserId", "1")
                }))
            }
        };
        
        _sut = new BankAccountService(_bankAccountRepository,
            _mapper,
            _httpContextAccessor,
            _celCashClientService,
            _emailService);
    }

    [Fact]
    public void CreateBankAccount_WithValidData_ShouldCreateBankAndIntegrate()
    {
        //Arrange
        var userId = "1";
        var bankAccountDto = new BankAccountDTO()
        {
            AccountUserId = int.Parse(userId),
            Cnae = "12345567",
            Document = "213454323454",
            Address = new AddressDTO(),
            EmailContact = "test@test.com",
            Name = "test test test test test test test",
            NameDisplay = "test"
        };
        var responseDto = new CelcashCreatedSubaccountResponseDTO()
        {
            Error = null,
            Type = true,
            CelcashCompany = new CelcashCompanyDTO()
            {
                ApiAuth = new ApiAuthData
                {
                    GalaxHash = "123454335",
                    GalaxId = 1,
                }
            }
        };
        
        A.CallTo(() => _bankAccountRepository
                .Get(A<Expression<Func<BankAccount, bool>>>.Ignored))
            .Returns(Task.FromResult(Enumerable.Empty<BankAccount>().ToList()));
        
        A.CallTo(() => _celCashClientService.CreateSubBankAccount(A<BankAccountDTO>.Ignored))
            .Returns(Task.FromResult(responseDto));
        
        //Act
        var result = _sut.CreateBankAccount(bankAccountDto).Result;
        
        //Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Success);
        Assert.Equal(StatusBankAccount.PendingDocuments, result.Data.Status);
        
        A.CallTo(() => _bankAccountRepository.Add(A<BankAccount>.Ignored))
            .MustHaveHappened();
        
        A.CallTo(() => _celCashClientService.CreateSubBankAccount(A<BankAccountDTO>.Ignored))
            .MustHaveHappened();
        
        A.CallTo(() => _bankAccountRepository.Update(A<BankAccount>.That.Matches(b => b.Status == StatusBankAccount.PendingDocuments)))
            .MustHaveHappened();

        A.CallTo(() => _emailService.SendEmailAsync(A<BankAccountSavedMessageBuilder>.Ignored))
            .MustHaveHappened();
    }
}