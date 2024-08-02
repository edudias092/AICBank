using System.Security.Cryptography.X509Certificates;
using AICBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AICBank.Data.Mapping
{
    public class ProfessionalMappingConfiguration : IEntityTypeConfiguration<Professional>
    {
        public void Configure(EntityTypeBuilder<Professional> builder)
        {
            builder.ToTable("Professionals");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Inscription)
                    .HasColumnType("varchar(255)")
                    .IsRequired();

            builder.Property(x => x.InternalName)
                    .HasColumnType("varchar(150)")
                    .IsRequired();

            builder.HasOne(x => x.BankAccount)
                    .WithOne(x => x.Professional)
                    .HasForeignKey<Professional>(x => x.BankAccountId)
                    .IsRequired();
        }
    }
}