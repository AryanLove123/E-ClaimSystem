using EClaim.Application.Common;
using EClaim.Application.DTOs.Claims;

namespace EClaim.Application.Interfaces;

public interface IClaimService
{
    Task<ClaimDto> CreateClaimAsync(int claimantId, CreateClaimRequest request, CancellationToken ct = default);
    Task<ClaimDto> SubmitClaimAsync(int claimId, int claimantId, CancellationToken ct = default);
    Task<ClaimDto> GetClaimByIdAsync(int claimId, CurrentUser currentUser, CancellationToken ct = default);
    Task<PagedResult<ClaimDto>> SearchClaimsAsync(ClaimFilterRequest request, CurrentUser currentUser, CancellationToken ct = default);
    Task<ClaimDocumentDto> UploadDocumentAsync(int claimId, int userId, Stream fileStream, string filename, string contentType, long fileSize, CancellationToken ct = default);
    Task<ClaimDto> StartReviewAsync(int claimId, int adjusterId, CancellationToken ct = default);
    Task<ClaimDto> AdjustClaimAsync(int claimId, int adjusterId, AdjustClaimRequest request, CancellationToken ct = default);
    Task<ClaimDto> RequestAdditionalDocumentsAsync(int claimId, int adjusterId, DocumentsRequest request, CancellationToken ct = default);
    Task<ClaimDto> CompleteAdjusterReviewAsync(int claimId, int adjusterId, ApprovalDecisionRequest request, CancellationToken ct = default);
    Task<ClaimDto> ResubmitAfterDocumentAsync(int claimId, int claimantId, CancellationToken ct = default);
    Task<ClaimDto> ApproveAsync(int claimId, int approverId, ApprovalDecisionRequest request, CancellationToken ct = default);
    Task<ClaimDto> RejectAsync(int claimId, int approverId, ApprovalDecisionRequest request, CancellationToken ct = default);
}
