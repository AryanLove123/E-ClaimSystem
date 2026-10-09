using EClaim.Application.DTOs.Workflow;
using EClaim.Domain.Entities;
using EClaim.Domain.Enums;

namespace EClaim.Application.Interfaces;

public interface IWorkflowService
{
    Task<Workflow> ResolveWorkflowAsync(ClaimType claimType, decimal amount, ClaimSeverity severity, CancellationToken ct = default);
    Task<ClaimWorkflow> StartWorkflowAsync(Claim claim, CancellationToken ct = default);
    Task<ClaimWorkflowStep?> GetCurrentStepAsync(int claimId, CancellationToken ct = default);
    Task<ClaimWorkflowStep> CompleteStepAsync(int claimWorkflowStepId, int performedByUserId, string? comments, CancellationToken ct = default);
    Task PauseWorkflowAsync(int claimId, string reason, CancellationToken ct = default);
    Task ResumeWorkflowAsync(int claimId, CancellationToken ct = default);
    Task RejectWorkflowAsync(int claimId, CancellationToken ct = default);
    Task<ClaimWorkflowStatusDto> GetWorkflowStatusAsync(int claimId, CancellationToken ct = default);
}
