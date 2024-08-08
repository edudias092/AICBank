using System.Security.Cryptography.X509Certificates;
using AICBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AICBank.Data.Mapping
{
    public class AccountUserMappingConfiguration : IEntityTypeConfiguration<AccountUser>
    {
        public void Configure(EntityTypeBuilder<AccountUser> builder)
        {
            builder.ToTable("AccountUsers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                    .HasColumnType("varchar(255)")
                    .IsRequired();

            builder.Property(x => x.Phone)
                    .HasColumnType("varchar(20)")
                    .IsRequired();

            builder.Property(x => x.Email)
                    .HasColumnType("varchar(255)")
                    .IsRequired();

            builder.Property(x => x.IdentityUserId)
                    .HasColumnType("varchar(255)")
                    .IsRequired(false);

            builder.HasOne(x => x.BankAccount)
                    .WithOne(x => x.AccountUser)
                    .HasForeignKey<AccountUser>(x => x.BankAccountId)
                    .IsRequired(false);
        }
    }
}