using EClaim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EClaim.Infrastructure.Persistence.Configurations;

public class ClaimHistoryConfiguration : IEntityTypeConfiguration<ClaimHistory>
{
    public void Configure(EntityTypeBuilder<ClaimHistory> builder)
    {
        builder.ToTable("ClaimHistories");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Action).IsRequired().HasMaxLength(100);
        builder.Property(h => h.OldStatus).HasConversion<string>().HasMaxLength(35);
        builder.Property(h => h.NewStatus).HasConversion<string>().HasMaxLength(35);
        builder.Property(h => h.Comments).HasMaxLength(2000);

        builder.HasOne(h => h.Claim)
            .WithMany(c => c.History)
            .HasForeignKey(h => h.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.PerformedBy)
            .WithMany()
            .HasForeignKey(h => h.PerformedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
