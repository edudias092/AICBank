using System;
using AICBank.Core.Entities;
using AICBank.Core.Interfaces;
using AICBank.Data.Context;

namespace AICBank.Data.Repositories;

public class BankAccountRepository : Repository<BankAccount>, IBankAccountRepository
{
    public BankAccountRepository(AICBankDbContext db) : base(db)
    {
    }
}
