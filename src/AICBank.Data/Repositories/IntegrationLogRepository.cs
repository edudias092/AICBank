using AICBank.Core.Entities;
using AICBank.Core.Interfaces;
using AICBank.Data.Context;

namespace AICBank.Data.Repositories;

public class IntegrationLogRepository : Repository<IntegrationLog>, IIntegrationLogRepository
{
    public IntegrationLogRepository(AICBankDbContext db) : base(db) {}
}