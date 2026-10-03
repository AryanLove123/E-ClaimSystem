using EClaim.Application.Interfaces;
using EClaim.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EClaim.Infrastructure.Persistence;

public class EClaimDbContext : DbContext, IEClaimDbContext
{
    public EClaimDbContext(DbContextOptions<EClaimDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimDocument> ClaimDocuments => Set<ClaimDocument>();
    public DbSet<ClaimHistory> ClaimHistories => Set<ClaimHistory>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<ClaimWorkflow> ClaimWorkflows => Set<ClaimWorkflow>();
    public DbSet<ClaimWorkflowStep> ClaimWorkflowSteps => Set<ClaimWorkflowStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EClaimDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
