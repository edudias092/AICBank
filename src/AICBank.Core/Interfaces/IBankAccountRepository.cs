using System;
using AICBank.Core.Entities;
using AICBank.Data.Repositories;

namespace AICBank.Core.Interfaces;

public interface IBankAccountRepository : IRepository<BankAccount>
{
}
