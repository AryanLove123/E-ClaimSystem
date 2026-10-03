using EClaim.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EClaim.Application.Interfaces;

public interface IEClaimDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Claim> Claims { get; }
    DbSet<ClaimDocument> ClaimDocuments { get; }
    DbSet<ClaimHistory> ClaimHistories { get; }
    DbSet<Workflow> Workflows { get; }
    DbSet<WorkflowStep> WorkflowSteps { get; }
    DbSet<ClaimWorkflow> ClaimWorkflows { get; }
    DbSet<ClaimWorkflowStep> ClaimWorkflowSteps { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
