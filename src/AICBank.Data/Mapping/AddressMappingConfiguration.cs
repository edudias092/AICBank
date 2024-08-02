using System.Security.Cryptography.X509Certificates;
using AICBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AICBank.Data.Mapping
{
    public class AddressMappingConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("Addresses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ZipCode)
                    .HasColumnType("varchar(20)")
                    .IsRequired();

            builder.Property(x => x.Street)
                    .HasColumnType("varchar(255)")
                    .IsRequired();

            builder.Property(x => x.Number)
                    .HasColumnType("varchar(20)")
                    .IsRequired();

            builder.Property(x => x.Complement)
                    .HasColumnType("varchar(255)")
                    .IsRequired(false);

            builder.Property(x => x.Neighborhood)
                    .HasColumnType("varchar(255)")
                    .IsRequired();

            builder.Property(x => x.City)
                    .HasColumnType("varchar(255)")
                    .IsRequired();

            builder.Property(x => x.State)
                    .HasColumnType("varchar(255)")
                    .IsRequired();

            builder.HasOne(x => x.BankAccount)
                    .WithOne(x => x.Address)
                    .HasForeignKey<Address>(x => x.BankAccountId)
                    .IsRequired();
        }
    }
}