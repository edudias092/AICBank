using AICBank.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AICBank.Data.Context
{
    public class AICBankDbContext : DbContext
    {
        public AICBankDbContext(DbContextOptions<AICBankDbContext> options) : base(options)
        {
            
        }
        
        #region DbSets 
        public DbSet<Address> Addresses { get; set; }

        #endregion
    }

}