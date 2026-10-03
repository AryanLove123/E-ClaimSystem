using EClaim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EClaim.Infrastructure.Persistence.Configurations;

public class ClaimWorkflowConfiguration : IEntityTypeConfiguration<ClaimWorkflow>
{
    public void Configure(EntityTypeBuilder<ClaimWorkflow> builder)
    {
        builder.ToTable("ClaimWorkflows");
        builder.HasKey(cw => cw.Id);
        builder.HasIndex(cw => cw.ClaimId).IsUnique(); // one workflow instance per claim
        builder.Property(cw => cw.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder.HasOne(cw => cw.Workflow)
            .WithMany()
            .HasForeignKey(cw => cw.WorkflowId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
