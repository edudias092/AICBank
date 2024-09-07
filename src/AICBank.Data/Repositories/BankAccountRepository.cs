using System;
using AICBank.Core.Entities;
using AICBank.Core.Interfaces;
using AICBank.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AICBank.Data.Repositories;

public class BankAccountRepository : Repository<BankAccount>, IBankAccountRepository
{
    public BankAccountRepository(AICBankDbContext db) : base(db)
    {
    }

    public async Task<BankAccount> GetBankAccountWithInfoByIdAsync(int id)
    { 
        var bankAccount = await _set.Where(b => b.Id == id)
                        .Include(b => b.Address)
                        .Include(b => b.Professional)
                        .Include(b => b.AccountUser)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
        
        return bankAccount;
    }
}
