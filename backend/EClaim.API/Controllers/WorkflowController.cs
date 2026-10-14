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

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var workflows = await _workflowService.GetAllWorkflowsAsync(ct);
        return Ok(ApiResponse<List<WorkflowDto>>.Ok(workflows));
    }

    [HttpGet("claims/{claimId:int}/status")]
    public async Task<IActionResult> GetClaimWorkflowStatus(int claimId, CancellationToken ct)
    {
        var status = await _workflowService.GetWorkflowStatusAsync(claimId, ct);
        return Ok(ApiResponse<ClaimWorkflowStatusDto>.Ok(status));
    }

}