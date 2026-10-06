using EClaim.Application.Common;
using EClaim.Application.DTOs.Claims;
using EClaim.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EClaim.API.Controllers;

[Authorize]
public class ClaimsController : BaseApiController
{
    private readonly IClaimService _claimService;

    public ClaimsController(IClaimService claimService)
    {
        _claimService = claimService;
    }

    [HttpPost]
    [Authorize(Roles = "Claimant")]
    public async Task<IActionResult> CreateClaim([FromBody] CreateClaimRequest request, CancellationToken ct)
    {
        var claim = await _claimService.CreateClaimAsync(CurrentUserId, request, ct);
        return CreatedAtAction(nameof(GetClaimById), new { id = claim.Id }, ApiResponse<ClaimDto>.Ok(claim, "Claim created as Draft."));
    }

    [HttpPost("{id:int}/submit")]
    [Authorize(Roles = "Claimant")]
    public async Task<IActionResult> SubmitClaim(int id, CancellationToken ct)
    {
        var claim = await _claimService.SubmitClaimAsync(id, CurrentUserId, ct);
        return Ok(ApiResponse<ClaimDto>.Ok(claim, "Claim submitted successfully for review."));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetClaimById(int id, CancellationToken ct)
    {
        var claim = await _claimService.GetClaimByIdAsync(id, CurrentUser, ct);
        return Ok(ApiResponse<ClaimDto>.Ok(claim, "Claim retrieved successfully."));
    }

    [HttpGet]
    public async Task<IActionResult> SearchClaims([FromQuery] ClaimFilterRequest filter, CancellationToken ct)
    {
        var claims = await _claimService.SearchClaimsAsync(filter, CurrentUser, ct);
        return Ok(ApiResponse<PagedResult<ClaimDto>>.Ok(claims, "Claims retrieved successfully."));
    }
}