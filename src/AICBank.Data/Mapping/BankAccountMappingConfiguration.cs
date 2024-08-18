using System.Security.Cryptography.X509Certificates;
using AICBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AICBank.Data.Mapping
{
    public class BankAccountMappingConfiguration : IEntityTypeConfiguration<BankAccount>
    {
        public void Configure(EntityTypeBuilder<BankAccount> builder)
        {
            builder.ToTable("BankAccounts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                    .HasColumnType("varchar(255)")
                    .IsRequired();

            builder.Property(x => x.Document)
                    .HasColumnType("varchar(255)")
                    .IsRequired();

            builder.Property(x => x.Phone)
                    .HasColumnType("varchar(20)")
                    .IsRequired();

            builder.Property(x => x.EmailContact)
                    .HasColumnType("varchar(255)")
                    .IsRequired();

            builder.Property(x => x.Logo)
                    .HasColumnType("text")
                    .IsRequired(false);

            builder.Property(x => x.SoftDescriptor)
                    .HasColumnType("varchar(18)")
                    .IsRequired();

            builder.Property(x => x.GalaxId)
                    .HasColumnType("varchar(255)")
                    .IsRequired(false);

            builder.Property(x => x.GalaxPayId)
                    .HasColumnType("int");

            builder.Property(x => x.GalaxHash)
                    .HasColumnType("varchar(255)")
                    .IsRequired(false);

            builder.Property(x => x.NameDisplay)
                    .HasColumnType("varchar(255)")
                    .IsRequired(false);

            builder.Property(x => x.ResponsibleDocument)
                    .HasColumnType("varchar(20)")
                    .IsRequired(false);

            builder.Property(x => x.TypeCompany)
                    .HasColumnType("varchar(50)")
                    .IsRequired(false);

            builder.Property(x => x.Cnae)
                    .HasColumnType("varchar(10)")
                    .IsRequired(false);

            builder.HasOne(x => x.Address)
                    .WithOne(x => x.BankAccount)
                    .HasForeignKey<BankAccount>(x => x.AddressId)
                    .IsRequired(false);

            builder.HasOne(x => x.Professional)
                    .WithOne(x => x.BankAccount)
                    .HasForeignKey<BankAccount>(x => x.ProfessionalId)
                    .IsRequired(false);

            builder.HasOne(x => x.AccountUser)
                    .WithOne(x => x.BankAccount)
                    .HasForeignKey<BankAccount>(x => x.AccountUserId)
                    .IsRequired(false);
        }
    }
}