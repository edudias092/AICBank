using AICBank.Core.Entities;
using AICBank.Core.Interfaces;
using AICBank.Data.Context;

namespace AICBank.Data.Repositories
{
    public class AccountUserRepository : Repository<AccountUser>, IAccountUserRepository
    {
        public AccountUserRepository(AICBankDbContext db) : base(db)
        {
        }
    }
}