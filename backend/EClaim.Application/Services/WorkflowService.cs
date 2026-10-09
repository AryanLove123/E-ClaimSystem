using EClaim.Application.DTOs.Workflow;
using EClaim.Application.Interfaces;
using EClaim.Domain.Entities;
using EClaim.Domain.Enums;
using EClaim.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EClaim.Application.Services;

public class WorkflowService : IWorkflowService
{
    private IEClaimDbContext _db;

    public WorkflowService(IEClaimDbContext db)
    {
        _db = db;
    }

    private static int Specificity(Workflow w) =>
        (w.ClaimType != null ? 1 : 0) + (w.MinAmount != null ? 1 : 0) + (w.MaxAmount != null ? 1 : 0) + (w.Severity != null ? 1 : 0);

    public async Task<Workflow> ResolveWorkflowAsync(ClaimType claimType, decimal amount, ClaimSeverity severity, CancellationToken ct = default)
    {
        var workflowCandidates = await _db.Workflows
            .Include(w => w.Steps)
            .Where(w => w.IsActive)
            .Where(w => w.ClaimType == null || w.ClaimType == claimType)
            .Where(w => w.MinAmount == null || w.MinAmount <= amount)
            .Where(w => w.MaxAmount == null || w.MaxAmount >= amount)
            .Where(w => w.Severity == null || w.Severity == severity)
            .ToListAsync(ct);
        
        if(workflowCandidates.Count == 0)
        {
            throw new NotFoundException($"No active workflow configured for ClaimType={claimType}, Amount={amount}, Severity={severity}.");        
        }

        return workflowCandidates.OrderByDescending(w => w.Priority).ThenByDescending(w => Specificity(w)).First();
    }

    public async Task<ClaimWorkflow> StartWorkflowAsync(Claim claim, CancellationToken ct = default)
    {
        var existing = await _db.ClaimWorkflows.FirstOrDefaultAsync(cw => cw.ClaimId == claim.Id, ct);
        if(existing != null)
        {
            throw new InvalidOperationException($"Claim {claim.ClaimNumber} already has an active workflow.");
        }

        var workflow = await ResolveWorkflowAsync(claim.ClaimType, claim.RequestedAmount, claim.Severity, ct);

        var claimWorkflow = new ClaimWorkflow
        {
            ClaimId = claim.Id,
            WorkflowId = workflow.Id,
            Status = WorkflowInstanceStatus.Active
        };

        foreach (var step in workflow.Steps.OrderBy(s => s.StepOrder))
        {
            var claimWorkflowStep = new ClaimWorkflowStep
            {
                WorkflowStepId = step.Id,
                StepOrder = step.StepOrder,
                Status = step.StepOrder == workflow.Steps.Min(s => s.StepOrder)
                    ? WorkflowStepStatus.Current
                    : WorkflowStepStatus.Pending,
                StartedAt = step.StepOrder == workflow.Steps.Min(s => s.StepOrder) ? DateTime.UtcNow : null
            };
            claimWorkflow.Steps.Add(claimWorkflowStep);
        }

        _db.ClaimWorkflows.Add(claimWorkflow);
        await _db.SaveChangesAsync(ct);

        return claimWorkflow;
    }
    
    public async Task<ClaimWorkflowStep?> GetCurrentStepAsync(int claimId, CancellationToken ct = default)
    {
        var claimWorkflow = await _db.ClaimWorkflows
            .Include(cw => cw.Steps).ThenInclude(s => s.WorkflowStep)
            .FirstOrDefaultAsync(cw => cw.ClaimId == claimId, ct);
        
        return claimWorkflow?.Steps.FirstOrDefault(s => s.Status == WorkflowStepStatus.Current);

    }

    public async Task<ClaimWorkflowStep> CompleteStepAsync(int claimWorkflowStepId, int performedByUserId, string? comments, CancellationToken ct = default)
    {
        var step = await _db.ClaimWorkflowSteps
            .Include(s => s.ClaimWorkflow).ThenInclude(cw => cw.Steps)
            .FirstOrDefaultAsync(s => s.Id == claimWorkflowStepId, ct)
            ?? throw new NotFoundException("ClaimWorkflowStep", claimWorkflowStepId);
        
        if (step.Status != WorkflowStepStatus.Current)
        {
            throw new InvalidWorkflowTransitionException("Only the current step can be completed.");
        }

        step.Status = WorkflowStepStatus.Completed;
        step.CompletedAt = DateTime.UtcNow;
        step.Comments = comments;

        var nextStep = step.ClaimWorkflow.Steps
            .Where(s => s.StepOrder > step.StepOrder)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefault();

        if (nextStep != null)
        {
            nextStep.Status = WorkflowStepStatus.Current;
            nextStep.StartedAt = DateTime.UtcNow;
        }
        else
        {
            step.ClaimWorkflow.Status = WorkflowInstanceStatus.Completed;
        }

        await _db.SaveChangesAsync(ct);
        return step;
    }

    public async Task PauseWorkflowAsync(int claimId, string reason, CancellationToken ct = default)
    {
        var claimWorkflow = await _db.ClaimWorkflows
            .FirstOrDefaultAsync(cw => cw.ClaimId == claimId, ct)
            ?? throw new NotFoundException("ClaimWorkflow", claimId);

        if (claimWorkflow.Status != WorkflowInstanceStatus.Active)
        {
            throw new InvalidWorkflowTransitionException("Only active workflows can be paused.");
        }

        claimWorkflow.Status = WorkflowInstanceStatus.Paused;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ResumeWorkflowAsync(int claimId, CancellationToken ct = default)
    {
        var claimWorkflow = await _db.ClaimWorkflows
            .FirstOrDefaultAsync(cw => cw.ClaimId == claimId, ct)
            ?? throw new NotFoundException("ClaimWorkflow", claimId);

        if (claimWorkflow.Status != WorkflowInstanceStatus.Paused)
        {
            throw new InvalidWorkflowTransitionException("Only paused workflows can be resumed.");
        }

        claimWorkflow.Status = WorkflowInstanceStatus.Active;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RejectWorkflowAsync(int claimId, CancellationToken ct = default)
    {
        var claimWorkflow = await _db.ClaimWorkflows
            .Include(cw => cw.Steps)
            .FirstOrDefaultAsync(cw => cw.ClaimId == claimId, ct)
            ?? throw new NotFoundException("ClaimWorkflow for claim", claimId);

        claimWorkflow.Status = WorkflowInstanceStatus.Rejected;

        var currentStep = claimWorkflow.Steps.FirstOrDefault(s => s.Status == WorkflowStepStatus.Current);
        currentStep?.Status = WorkflowStepStatus.Skipped;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<ClaimWorkflowStatusDto> GetWorkflowStatusAsync(int claimId, CancellationToken ct = default)
    {
        var claimWorkflow = await _db.ClaimWorkflows
            .Include(cw => cw.Workflow)
            .Include(cw => cw.Steps).ThenInclude(s => s.WorkflowStep)
            .Include(cw => cw.Steps).ThenInclude(s => s.AssignedToUser)
            .FirstOrDefaultAsync(cw => cw.ClaimId == claimId, ct)
            ?? throw new NotFoundException("ClaimWorkflow for claim", claimId);

        return new ClaimWorkflowStatusDto
        {
            WorkflowName = claimWorkflow.Workflow.Name,
            Status = claimWorkflow.Status.ToString(),
            Steps = claimWorkflow.Steps.OrderBy(s => s.StepOrder).Select(s => new ClaimWorkflowStepStatusDto
            {
                StepOrder = s.StepOrder,
                Name = s.WorkflowStep.Name,
                ResponsibleRole = s.WorkflowStep.ResponsibleRole.ToString(),
                Status = s.Status.ToString(),
                AssignedToName = s.AssignedToUser?.FullName,
                CompletedAt = s.CompletedAt
            }).ToList()
        };
    }
}
