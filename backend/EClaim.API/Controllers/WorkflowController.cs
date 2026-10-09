using EClaim.Application.Common;
using EClaim.Application.DTOs.Workflow;
using EClaim.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EClaim.API.Controllers;

[Authorize]
public class WorkflowController : BaseApiController
{
    private IWorkflowService _workflowService; 

    public WorkflowController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet("claims/{claimId:int}/status")]
    public async Task<IActionResult> GetClaimWorkflowStatus(int claimId, CancellationToken ct)
    {
        var status = await _workflowService.GetWorkflowStatusAsync(claimId, ct);
        return Ok(ApiResponse<ClaimWorkflowStatusDto>.Ok(status));
    }

}