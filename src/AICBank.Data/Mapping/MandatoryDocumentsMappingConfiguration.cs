using AICBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AICBank.Data.Mapping;

public class MandatoryDocumentsMappingConfiguration : IEntityTypeConfiguration<MandatoryDocuments>
{
    public void Configure(EntityTypeBuilder<MandatoryDocuments> builder)
    {
        builder.ToTable("MandatoryDocuments")
            .HasKey(x => x.Id);

        builder
            .Property(x => x.MonthlyIncome)
            .HasColumnType("int")
            .IsRequired();
        
        builder
            .Property(x => x.About)
            .HasColumnType("varchar(500)")
            .IsRequired();
        
        builder
            .Property(x => x.SocialMediaLink)
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder
            .Property(x => x.AssociateDocument)
            .HasColumnType("varchar(11)")
            .IsRequired();

        builder
            .Property(x => x.AssociateName)
            .HasColumnType("varchar(255)")
            .IsRequired();
        
        builder
            .Property(x => x.MotherName)
            .HasColumnType("varchar(255)")
            .IsRequired();
        
        builder
            .Property(x => x.BirthDate)
            .HasColumnType("datetime")
            .IsRequired();
        
        builder
            .Property(x => x.AssociateType)
            .HasColumnType("varchar(15)")
            .IsRequired();

        builder.HasOne(x => x.BankAccount)
            .WithMany(x => x.MandatoryDocuments)
            .HasForeignKey(x => x.BankAccountId);
    }
}