using EClaim.Application.Common;
using EClaim.Application.DTOs.Claims;
using EClaim.Application.Interfaces;
using ClaimEntity = EClaim.Domain.Entities.Claim;
using Microsoft.EntityFrameworkCore;
using EClaim.Domain.Enums;
using EClaim.Application.Mappings;
using EClaim.Domain.Exceptions;

namespace EClaim.Application.Services;

public class ClaimService : IClaimService
{
    private IEClaimDbContext _db;
    private IWorkflowService _workflowService;

    public ClaimService(IEClaimDbContext db, IWorkflowService workflowService)
    {
        _db = db;
        _workflowService = workflowService;
    }

    private static void EnsureCanView(ClaimEntity claim, CurrentUser currentUser)
    {
        var canView = currentUser.Role switch
        {
            nameof(RoleType.Claimant) => claim.ClaimantId == currentUser.UserId,
            nameof(RoleType.Adjuster) => claim.AssignedAdjusterId == currentUser.UserId,
            nameof(RoleType.Approver) => claim.AssignedApproverId == currentUser.UserId,
            nameof(RoleType.Admin) => true,
            _ => false
        };

        if (!canView)
            throw new ForbiddenAccessException("You do not have access to this claim.");
    }

    private IQueryable<ClaimEntity> ClaimsWithIncludes() =>
        _db.Claims
            .Include(c => c.Claimant)
            .Include(c => c.AssignedAdjuster)
            .Include(c => c.AssignedApprover)
            .Include(c => c.Documents);

    public async Task<ClaimDto> CreateClaimAsync(int claimantId, CreateClaimRequest request, CancellationToken ct = default)
    {
        var claimType = Enum.Parse<ClaimType>(request.ClaimType, ignoreCase: true);
        var severity = SeverityCalculator.Calculate(request.RequestedAmount);

        var claim = new ClaimEntity
        {
            ClaimantId = claimantId,
            PolicyNumber = request.PolicyNumber.Trim(),
            ClaimType = claimType,
            IncidentDate = request.IncidentDate,
            Description = request.Description.Trim(),
            Location = request.Location.Trim(),
            RequestedAmount = request.RequestedAmount,
            Severity = severity,
            ClaimNumber = string.Empty
        };

        _db.Claims.Add(claim);
        await _db.SaveChangesAsync(ct);

        claim.ClaimNumber = ClaimNumberGenerator.Generate(claim.Id);
        await _db.SaveChangesAsync(ct);

        var reloaded = await ClaimsWithIncludes().FirstAsync(c => c.Id == claim.Id, ct);
        return reloaded.ToDto();
    }

    public async Task<ClaimDto> SubmitClaimAsync(int claimId, int claimantId, CancellationToken ct = default)
    {
        var claim = _db.Claims.FirstOrDefault(c => c.Id == claimId) ?? throw new NotFoundException("Claim", claimId);
        if(claim.ClaimantId != claimantId)
        {
            throw new ForbiddenAccessException("You do not have permission to submit this claim.");
        }

        if (claim.Status != ClaimStatus.Draft)
        {
            throw new InvalidWorkflowTransitionException($"Claim {claim.ClaimNumber} is not in Draft status and cannot be submitted.");
        }

        claim.Status = ClaimStatus.UnderReview;
        claim.UpdatedAt = DateTime.UtcNow;

        var claimWorkflow = await _workflowService.StartWorkflowAsync(claim, ct);

        var firstStep = claimWorkflow.Steps.OrderBy(s => s.StepOrder).First();
        var adjuster = await _db.Users
            .Include(u => u.Role)
            .Where(u => u.Role.Name == RoleType.Adjuster && u.IsActive)
            .OrderBy(u => _db.ClaimWorkflowSteps.Count(s => s.AssignedToUserId == u.Id && s.Status == WorkflowStepStatus.Current))
            .FirstOrDefaultAsync(ct);

        if(adjuster != null)
        {
            firstStep.AssignedToUserId = adjuster.Id;
            claim.AssignedAdjusterId = adjuster.Id;
        }

        await _db.SaveChangesAsync(ct);

        return claim.ToDto();
    }

    public async Task<ClaimDto> GetClaimByIdAsync(int claimId, CurrentUser currentUser, CancellationToken ct = default)
    {
        var claim = await ClaimsWithIncludes().FirstOrDefaultAsync(c => c.Id == claimId, ct) ?? throw new NotFoundException("Claim", claimId);
        EnsureCanView(claim, currentUser);
        return claim.ToDto();
    }

    public async Task<PagedResult<ClaimDto>> SearchClaimsAsync(ClaimFilterRequest filter, CurrentUser currentUser, CancellationToken ct = default)
    {
        var query = ClaimsWithIncludes();

        switch (currentUser.Role)
        {
            case "Claimant":
                query = query.Where(c => c.ClaimantId == currentUser.UserId);
                break;

            case "Adjuster":
                query = query.Where(c => c.AssignedAdjusterId == currentUser.UserId);
                break;

            case "Approver":
                query = query.Where(c => c.AssignedApproverId == currentUser.UserId);
                break;

            default:
                break;
        }

        if (!string.IsNullOrWhiteSpace(filter.ClaimNumber))
        {
            query = query.Where(c => c.ClaimNumber.Contains(filter.ClaimNumber));
        }
        if(!string.IsNullOrWhiteSpace(filter.PolicyNumber))
        {
            query = query.Where(c => c.PolicyNumber.Contains(filter.PolicyNumber));
        }
        if(!string.IsNullOrWhiteSpace(filter.ClaimType))
        {
            var claimType = Enum.Parse<ClaimType>(filter.ClaimType, ignoreCase: true);
            query = query.Where(c => c.ClaimType == claimType);
        }
        if(!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = Enum.Parse<ClaimStatus>(filter.Status, ignoreCase: true);
            query = query.Where(c => c.Status == status);
        }
        if(!string.IsNullOrWhiteSpace(filter.Severity))
        {
            var severity = Enum.Parse<ClaimSeverity>(filter.Severity, ignoreCase: true);
            query = query.Where(c => c.Severity == severity);
        }
        if(filter.FromDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= filter.FromDate.Value);
        }
        if(filter.ToDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= filter.ToDate.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return new PagedResult<ClaimDto>
        {
            Items = items.Select(c => c.ToDto()).ToList(),
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }
}
