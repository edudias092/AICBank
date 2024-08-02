using AICBank.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AICBank.Data.Context
{
    public class AICBankDbContext : IdentityDbContext
    {
        public AICBankDbContext(DbContextOptions<AICBankDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AICBankDbContext).Assembly);
        }

        #region DbSets 
        public DbSet<Address> Addresses { get; set; }

        #endregion
    }

}