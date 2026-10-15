using EClaim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EClaim.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(n => n.Subject).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Message).IsRequired().HasMaxLength(2000);
                
        builder.HasIndex(n => new { n.UserId, n.Type, n.IsRead, n.CreatedAt });

        builder.HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(n => n.Claim).WithMany().HasForeignKey(n => n.ClaimId).OnDelete(DeleteBehavior.SetNull);
    }
}