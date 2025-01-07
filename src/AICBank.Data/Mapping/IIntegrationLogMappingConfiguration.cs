using AICBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AICBank.Data.Mapping;

public class IIntegrationLogMappingConfiguration : IEntityTypeConfiguration<IntegrationLog>
{
    public void Configure(EntityTypeBuilder<IntegrationLog> builder)
    {
        builder.ToTable("IntegrationLogs");

        builder.HasKey(x => x.Id);
        
        builder
            .Property(x => x.Action)
            .IsRequired()
            .HasColumnType("varchar(150)");

        builder
            .Property(x => x.StatusCode)
            .IsRequired()
            .HasColumnType("varchar(100)");

        builder
            .Property(x => x.RequestData)
            .HasColumnType("mediumtext");
        
        builder
            .Property(x => x.ResponseData)
            .HasColumnType("mediumtext");
        
        builder
            .Property(x => x.TimeStamp)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("current_timestamp");
    }
}