using EClaim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EClaim.Infrastructure.Persistence.Configurations;

public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{   
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.ToTable("Workflows");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(150);
        builder.Property(w => w.ClaimType).HasConversion<string>().HasMaxLength(20);
        builder.Property(w => w.Severity).HasConversion<string>().HasMaxLength(20);
        builder.Property(w => w.MinAmount).HasColumnType("decimal(18,2)");
        builder.Property(w => w.MaxAmount).HasColumnType("decimal(18,2)");
    }
}
