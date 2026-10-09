using EClaim.Application.Common;
using EClaim.Application.DTOs.Claims;
using EClaim.Application.Interfaces;
using ClaimEntity = EClaim.Domain.Entities.Claim;
using Microsoft.EntityFrameworkCore;
using EClaim.Domain.Enums;
using EClaim.Application.Mappings;
using EClaim.Domain.Exceptions;
using EClaim.Domain.Entities;

namespace EClaim.Application.Services;

public class ClaimService : IClaimService
{
    private IEClaimDbContext _db;
    private IWorkflowService _workflowService;
    private IFileStorageService _fileStorageService;

    public ClaimService(IEClaimDbContext db, IWorkflowService workflowService, IFileStorageService fileStorageService)
    {
        _db = db;
        _workflowService = workflowService;
        _fileStorageService = fileStorageService;
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

    private static void EnsureAssignedAdjuster(ClaimEntity claim, int adjusterId)
    {
        if(claim.AssignedAdjusterId != adjusterId)
        {
            throw new ForbiddenAccessException("You are not the assigned adjuster for this claim");
        }
    }

    private static void EnsureAssignedApprover(ClaimEntity claim, int approverId)
    {
        if(claim.AssignedApproverId != approverId)
        {
            throw new ForbiddenAccessException("You are not the assigned approver for this claim");
        }
    }

    private static void EnsureOwnedByClaimant(ClaimEntity claim, int claimantId)
    {
        if (claim.ClaimantId != claimantId)
            throw new ForbiddenAccessException("You can only act on your own claims.");
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

    public async Task<ClaimDocumentDto> UploadDocumentAsync(int claimId, int userId, Stream fileStream, string fileName, string contentType, long fileSize, CancellationToken ct = default)
    {
        var claim = await _db.Claims.FirstOrDefaultAsync(c => c.Id == claimId, ct) 
            ?? throw new NotFoundException("Claim", claimId);

        if(!_fileStorageService.IsAllowedFile(fileName, contentType, fileSize))
        {
            throw new ValidationException("Only files with size 10Mb and format PDF, JPG, PNG are allowed");
        }

        var storedName = await _fileStorageService.SaveFileAsync(fileStream, fileName, ct);

        var doc = new ClaimDocument
        {
            ClaimId = claimId,
            OriginalFileName = fileName,
            StoredFileName = storedName,
            ContentType = contentType,
            FileSizeBytes = fileSize,
            UploadedByUserId = userId
        };

        _db.ClaimDocuments.Add(doc);
        await _db.SaveChangesAsync(ct);

        return doc.ToDto();
    }

    public async Task<ClaimDto> StartReviewAsync(int claimId, int adjusterId, CancellationToken ct = default)
    {
        var claim = await ClaimsWithIncludes().FirstOrDefaultAsync(c => c.Id == claimId, ct)
            ?? throw new NotFoundException("Claim", claimId);

        EnsureAssignedAdjuster(claim, adjusterId);

        if(claim.Status != ClaimStatus.UnderReview)
        {
            throw new InvalidWorkflowTransitionException("Claim it not awaiting adjuster review.");
        }
        return claim.ToDto();
    }

    public async Task<ClaimDto> AdjustClaimAsync(int claimId, int adjusterId, AdjustClaimRequest request, CancellationToken ct = default)
    {
        var claim = await ClaimsWithIncludes().FirstOrDefaultAsync(c => c.Id == claimId, ct)
            ?? throw new NotFoundException("Claim", claimId);

        EnsureAssignedAdjuster(claim,adjusterId);

        if(claim.Status != ClaimStatus.UnderReview)
        {
            throw new InvalidWorkflowTransitionException("Claim is not awaiting adjuster review.");
        }

        var oldAmount = claim.AdjustedAmount?.ToString() ?? "null";
        claim.AdjustedAmount = request.AdjustedAmount;
        claim.Status = ClaimStatus.UnderAdjustment;
        claim.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return claim.ToDto();
    }

    public async Task<ClaimDto> RequestAdditionalDocumentsAsync(int claimId, int adjusterId, DocumentsRequest request, CancellationToken ct = default)
    {
        var claim = await ClaimsWithIncludes().FirstOrDefaultAsync(c => c.Id == claimId, ct)
            ?? throw new NotFoundException("Claim", claimId);

        EnsureAssignedAdjuster(claim, adjusterId);

        if (claim.Status is not (ClaimStatus.UnderReview or ClaimStatus.UnderAdjustment))
            throw new InvalidWorkflowTransitionException("Additional documents can only be requested while under review/adjustment.");

        var oldStatus = claim.Status;
        claim.Status = ClaimStatus.AdditionalDocumentsRequired;
        claim.UpdatedAt = DateTime.UtcNow;

        await _workflowService.PauseWorkflowAsync(claimId, request.Comments, ct);
        await _db.SaveChangesAsync(ct);

        return claim.ToDto();
    }

    public async Task<ClaimDto> ResubmitAfterDocumentsAsync(int claimId, int claimantId, CancellationToken ct = default)
    {
        var claim = await ClaimsWithIncludes().FirstOrDefaultAsync(c => c.Id == claimId, ct)
            ?? throw new NotFoundException("Claim", claimId);

        EnsureOwnedByClaimant(claim, claimantId);

        if(claim.Status != ClaimStatus.AdditionalDocumentsRequired)
        {
            throw new InvalidWorkflowTransitionException("Claim is not awaiting additional documents.");
        }

        var oldStaus = claim.Status;
        claim.Status = ClaimStatus.UnderReview;
        claim.UpdatedAt = DateTime.UtcNow;

        await _workflowService.ResumeWorkflowAsync(claimId,ct);
        await _db.SaveChangesAsync(ct);

        return claim.ToDto();
    }

    public async Task<ClaimDto> CompleteAdjusterReviewAsync(int claimId, int adjusterId, ApprovalDecisionRequest request, CancellationToken ct = default)
    {
        var claim = await ClaimsWithIncludes().FirstOrDefaultAsync(c => c.Id == claimId, ct)
            ?? throw new NotFoundException("Claim", claimId);

        EnsureAssignedAdjuster(claim, adjusterId);

        if (claim.Status is not (ClaimStatus.UnderReview or ClaimStatus.UnderAdjustment))
            throw new InvalidWorkflowTransitionException("Claim is not awaiting adjuster completion.");

        //Get the current step for this claimworkflow
        var currentStep = await _workflowService.GetCurrentStepAsync(claimId,ct) ?? throw new NotFoundException("Current workflow step for claim", claimId);

        var oldStatus = claim.Status;

        var completedStep = await _workflowService.CompleteStepAsync(currentStep.Id,adjusterId, request.Comments, ct);
        var nextStep = await _workflowService.GetCurrentStepAsync(claim.Id,ct);

        if(nextStep != null)
        {
            var roleName = nextStep.WorkflowStep.ResponsibleRole switch
            {
                ClaimStepRole.Approver or ClaimStepRole.SeniorApprover => RoleType.Approver,
                _ => RoleType.Adjuster
            };

            var assignee = await _db.Users.Include(u => u.Role)
                .Where(u => u.Role.Name == roleName && u.IsActive)
                .OrderBy(u => _db.ClaimWorkflowSteps.Count(s => s.AssignedToUserId == u.Id && s.Status == WorkflowStepStatus.Current))
                .FirstOrDefaultAsync(ct);

            if(assignee != null)
            {
                nextStep.AssignedToUserId = assignee.Id;
                if(roleName == RoleType.Approver) claim.AssignedApproverId = assignee.Id;
            }

            claim.Status = ClaimStatus.PendingApproval;
        }
        else
        {
            claim.Status = ClaimStatus.Approved;
            claim.ApprovedAmount = claim.AdjustedAmount;
        }

        claim.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        if(claim.Status == ClaimStatus.Approved)
        {
            await FinalizeApprovalAsync(claim, ct);
        }
        return claim.ToDto();
    }

    public async Task<ClaimDto> ApproveAsync(int claimId, int approverId, ApprovalDecisionRequest request, CancellationToken ct = default)
    {
        var claim = await ClaimsWithIncludes().FirstOrDefaultAsync(c => c.Id == claimId, ct)
            ?? throw new NotFoundException("Claim", claimId);

        EnsureAssignedApprover(claim, approverId);

        if(claim.Status != ClaimStatus.PendingApproval)
        {
            throw new InvalidWorkflowTransitionException("Claim is not pending approval");
        }

        var currentStep = await _workflowService.GetCurrentStepAsync(claimId, ct) 
            ?? throw new NotFoundException("Current workflow step for claim", claimId);
        
        var oldStatus = claim.Status;
        var completedStep = await _workflowService.CompleteStepAsync(currentStep.Id, approverId, request.Comments,ct);
        var nextStep = await _workflowService.GetCurrentStepAsync(claimId, ct);

        if(nextStep != null)
        {
            var assignee = await _db.Users.Include(u => u.Role)
                .Where(u => u.Role.Name == RoleType.Approver && u.IsActive && u.Id != approverId)
                .OrderBy(u => _db.ClaimWorkflowSteps.Count(s => s.AssignedToUserId == u.Id && s.Status == WorkflowStepStatus.Current))
                .FirstOrDefaultAsync(ct);

            if(assignee != null)
            {
                nextStep.AssignedToUserId = assignee.Id;
                claim.AssignedApproverId = assignee.Id;

                await _db.SaveChangesAsync(ct);
            }
        }
        else
        {
            claim.Status = ClaimStatus.Approved;
            claim.ApprovedAmount = claim.AdjustedAmount;
        }

        claim.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        if(claim.Status == ClaimStatus.Approved)
        {
            await FinalizeApprovalAsync(claim, ct);
        }

        return claim.ToDto();
    }

    public async Task<ClaimDto> RejectAsync(int claimId, int approverId, ApprovalDecisionRequest request, CancellationToken ct = default)
    {
        var claim = await ClaimsWithIncludes().FirstOrDefaultAsync(c => c.Id == claimId, ct)
            ?? throw new NotFoundException("Claim", claimId);

        EnsureAssignedApprover(claim, approverId);

        if (claim.Status != ClaimStatus.PendingApproval)
            throw new InvalidWorkflowTransitionException("Claim is not pending approval.");

        var oldStatus = claim.Status;
        claim.Status = ClaimStatus.Rejected;
        claim.UpdatedAt = DateTime.UtcNow;

        await _workflowService.RejectWorkflowAsync(claimId,ct);
        await _db.SaveChangesAsync();

        return claim.ToDto();
    }

    private async Task FinalizeApprovalAsync(ClaimEntity claim, CancellationToken ct)
    {
        claim.Status = ClaimStatus.PaymentPending;
        await _db.SaveChangesAsync(ct);
    }
}
