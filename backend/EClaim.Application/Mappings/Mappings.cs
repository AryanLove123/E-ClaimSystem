using EClaim.Application.DTOs.Claims;
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
}