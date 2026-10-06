using EClaim.Application.Common;
using EClaim.Application.DTOs.Claims;

namespace EClaim.Application.Interfaces;

public interface IClaimService
{
    Task<ClaimDto> CreateClaimAsync(int claimantId, CreateClaimRequest request, CancellationToken ct = default);
    Task<ClaimDto> SubmitClaimAsync(int claimId, int claimantId, CancellationToken ct = default);
    Task<ClaimDto> GetClaimByIdAsync(int claimId, CurrentUser currentUser, CancellationToken ct = default);
    Task<PagedResult<ClaimDto>> SearchClaimsAsync(ClaimFilterRequest request, CurrentUser currentUser, CancellationToken ct = default);
    
}
