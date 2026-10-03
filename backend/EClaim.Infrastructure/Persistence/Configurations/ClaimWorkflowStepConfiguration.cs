using EClaim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EClaim.Infrastructure.Persistence.Configurations;

public class ClaimWorkflowStepConfiguration : IEntityTypeConfiguration<ClaimWorkflowStep>
{
    public void Configure(EntityTypeBuilder<ClaimWorkflowStep> builder)
    {
        builder.ToTable("ClaimWorkflowSteps");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Comments).HasMaxLength(2000);

        builder.HasOne(s => s.ClaimWorkflow)
            .WithMany(cw => cw.Steps)
            .HasForeignKey(s => s.ClaimWorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.WorkflowStep)
            .WithMany()
            .HasForeignKey(s => s.WorkflowStepId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.AssignedToUser)
            .WithMany()
            .HasForeignKey(s => s.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
