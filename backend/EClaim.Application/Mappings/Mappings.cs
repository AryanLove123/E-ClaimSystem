using EClaim.Application.DTOs.Claims;
using EClaim.Application.DTOs.Users;
using EClaim.Application.DTOs.Workflow;
using EClaim.Domain.Entities;

namespace EClaim.Application.Mappings;

public static class Mappings
{
    public static ClaimDto ToDto(this Claim claim) => new()
    {
        Id = claim.Id,
        ClaimantId = claim.ClaimantId,
        ClaimantName = claim.Claimant?.FullName ?? string.Empty,
        ClaimNumber = claim.ClaimNumber,
        PolicyNumber = claim.PolicyNumber,
        ClaimType = claim.ClaimType.ToString(),
        IncidentDate = claim.IncidentDate,
        Severity = claim.Severity.ToString(),
        Description = claim.Description,
        Location = claim.Location,
        RequestedAmount = claim.RequestedAmount,
        AdjustedAmount = claim.AdjustedAmount,
        ApprovedAmount = claim.ApprovedAmount,
        Status = claim.Status.ToString(),
        AssignedAdjusterName = claim.AssignedAdjuster?.FullName ?? string.Empty,
        AssignedApproverName = claim.AssignedApprover?.FullName ?? string.Empty,
        CreatedAt = claim.CreatedAt,
        UpdatedAt = claim.UpdatedAt,
        Documents = claim.Documents?.Select(d => d.ToDto()).ToList() ?? new List<ClaimDocumentDto>()
    };
    
    public static ClaimDocumentDto ToDto(this ClaimDocument doc) => new()
    {
        Id = doc.Id,
        OriginalFileName = doc.OriginalFileName,
        ContentType = doc.ContentType,
        FileSizeBytes = doc.FileSizeBytes,
        CreatedAt = doc.CreatedAt
    };

    public static ClaimHistoryDto ToDto(this ClaimHistory h) => new()
    {
        Action = h.Action,
        OldStatus = h.OldStatus?.ToString(),
        NewStatus = h.NewStatus?.ToString(),
        PerformedByName = h.PerformedBy?.FullName,
        Comments = h.Comments,
        CreatedAt = h.CreatedAt
    };
    public static UserProfileDto ToProfileDto(this User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        Role = user.Role.Name.ToString(),
        EmailVerified = user.EmailVerified
    };

    public static UserListDto ToAdminListDto(this User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role.Name.ToString(),
        EmailVerified = user.EmailVerified,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };

    public static WorkflowDto ToDto(this Workflow w) => new()
    {
        Id = w.Id,
        Name = w.Name,
        Description = w.Description,
        ClaimType = w.ClaimType?.ToString(),
        MinAmount = w.MinAmount,
        MaxAmount = w.MaxAmount,
        Severity = w.Severity?.ToString(),
        Priority = w.Priority,
        IsActive = w.IsActive,
        Steps = w.Steps?.OrderBy(s => s.StepOrder).Select(s => s.ToDto()).ToList() ?? new List<WorkflowStepDto>()
    };

    public static WorkflowStepDto ToDto(this WorkflowStep s) => new()
    {
        Id = s.Id,
        StepOrder = s.StepOrder,
        Name = s.Name,
        ResponsibleRole = s.ResponsibleRole.ToString(),
        AllowReject = s.AllowReject
    };
}