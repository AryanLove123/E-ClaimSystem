using EClaim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EClaim.Infrastructure.Persistence.Configurations;

public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.ToTable("ErrorLogs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Level).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Message).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.TraceId).HasMaxLength(50);
        builder.HasIndex(e => e.TraceId);
    }
}
