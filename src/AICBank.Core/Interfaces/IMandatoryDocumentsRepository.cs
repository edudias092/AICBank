using AICBank.Core.Entities;
using AICBank.Data.Repositories;

namespace AICBank.Core.Repositories;

public interface IMandatoryDocumentsRepository : IRepository<MandatoryDocuments>
{
    Task<MandatoryDocuments> GetMandatoryDocumentsByBankAccountId(int bankAccountId);
}