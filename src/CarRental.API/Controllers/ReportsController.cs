using CarRental.API.Authentication;
using CarRental.Application.Reporting.DownloadReport;
using CarRental.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.ReportDownload)]
[Route("api/reports")]
public sealed class ReportsController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DownloadReportQuery(id), cancellationToken);

        return result.Match<IActionResult>(
            link =>
            {
                Response.Headers.CacheControl = "no-store";
                return Redirect(link.Uri.AbsoluteUri);
            },
            CreateProblemResult);
    }

    private ObjectResult CreateProblemResult(ResultError error)
    {
        var statusCode = error.Code == "Report.UserIdentityMissing"
            ? StatusCodes.Status401Unauthorized
            : StatusCodes.Status404NotFound;
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode == StatusCodes.Status401Unauthorized
                ? "Authentication required"
                : "Report not found",
            Detail = error.Description,
            Instance = HttpContext.Request.Path
        };
        problem.Extensions["code"] = error.Code;
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;

        return StatusCode(statusCode, problem);
    }
}
