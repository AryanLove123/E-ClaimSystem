using EClaim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EClaim.Infrastructure.Persistence.Configurations;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("Claims");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.ClaimNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(c => c.ClaimNumber).IsUnique();
        builder.Property(c => c.PolicyNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(c => c.PolicyNumber);
        builder.Property(c => c.ClaimType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Severity).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(35);
        builder.HasIndex(c => c.Status);
        builder.Property(c => c.RequestedAmount).HasColumnType("decimal(18,2)");
        builder.Property(c => c.AdjustedAmount).HasColumnType("decimal(18,2)");
        builder.Property(c => c.ApprovedAmount).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Description).IsRequired().HasMaxLength(2000);
        builder.Property(c => c.Location).IsRequired().HasMaxLength(300);

        builder.Property(c => c.RowVersion).IsRowVersion();

        builder.HasOne(c => c.Claimant)
            .WithMany(u => u.ClaimsSubmitted)
            .HasForeignKey(c => c.ClaimantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.AssignedAdjuster)
            .WithMany()
            .HasForeignKey(c => c.AssignedAdjusterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.AssignedApprover)
            .WithMany()
            .HasForeignKey(c => c.AssignedApproverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ClaimWorkflow)
            .WithOne(cw => cw.Claim)
            .HasForeignKey<ClaimWorkflow>(cw => cw.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
