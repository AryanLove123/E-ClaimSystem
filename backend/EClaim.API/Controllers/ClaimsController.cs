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

    [HttpPost("{id:int}/documents")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument(int id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("No file was provided."));

        await using var stream = file.OpenReadStream();
        var doc = await _claimService.UploadDocumentAsync(id, CurrentUserId, stream, file.FileName, file.ContentType, file.Length, ct);
        return Ok(ApiResponse<ClaimDocumentDto>.Ok(doc, "Document uploaded."));
    }

    [HttpPost("{id:int}/adjust")]
    [Authorize(Roles = "Adjuster")]
    public async Task<IActionResult> Adjust(int id, AdjustClaimRequest request, CancellationToken ct)
    {
        var claim = await _claimService.AdjustClaimAsync(id, CurrentUserId, request, ct);
        return Ok(ApiResponse<ClaimDto>.Ok(claim, "Claim amount adjusted."));
    }

    [HttpPost("{id:int}/request-documents")]
    [Authorize(Roles = "Adjuster")]
    public async Task<IActionResult> RequestDocuments(int id, DocumentsRequest request, CancellationToken ct)
    {
        var claim = await _claimService.RequestAdditionalDocumentsAsync(id, CurrentUserId, request, ct);
        return Ok(ApiResponse<ClaimDto>.Ok(claim, "Additional documents requested; workflow paused."));
    }

    [HttpPost("{id:int}/resubmit-documents")]
    [Authorize(Roles = "Claimant")]
    public async Task<IActionResult> ResubmitAfterDocuments(int id, CancellationToken ct)
    {
        var claim = await _claimService.ResubmitAfterDocumentsAsync(id, CurrentUserId, ct);
        return Ok(ApiResponse<ClaimDto>.Ok(claim, "Documents submitted; workflow resumed."));
    }

    [HttpPost("{id:int}/complete-review")]
    [Authorize(Roles = "Adjuster")]
    public async Task<IActionResult> CompleteReview(int id, ApprovalDecisionRequest request, CancellationToken ct)
    {
        var claim = await _claimService.CompleteAdjusterReviewAsync(id, CurrentUserId, request, ct);
        return Ok(ApiResponse<ClaimDto>.Ok(claim, "Adjuster review completed."));
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Approver")]
    public async Task<IActionResult> Approve(int id, ApprovalDecisionRequest request, CancellationToken ct)
    {
        var claim = await _claimService.ApproveAsync(id, CurrentUserId, request, ct);
        return Ok(ApiResponse<ClaimDto>.Ok(claim, "Claim approved."));
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "Approver")]
    public async Task<IActionResult> Reject(int id, ApprovalDecisionRequest request, CancellationToken ct)
    {
        var claim = await _claimService.RejectAsync(id, CurrentUserId, request, ct);
        return Ok(ApiResponse<ClaimDto>.Ok(claim, "Claim rejected."));
    }

}