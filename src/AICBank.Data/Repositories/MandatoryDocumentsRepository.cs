using AICBank.Core.Entities;
using AICBank.Core.Repositories;
using AICBank.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AICBank.Data.Repositories;

public class MandatoryDocumentsRepository : Repository<MandatoryDocuments>, IMandatoryDocumentsRepository
{
    public MandatoryDocumentsRepository(AICBankDbContext db) : base(db)
    {
    }


    public async Task<MandatoryDocuments> GetMandatoryDocumentsByBankAccountId(int bankAccountId)
    {
        var mandatoryDocuments = await _set
            .Where(x => x.BankAccountId == bankAccountId)
            .Include(x => x.BankAccount)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return mandatoryDocuments!;
    }
}