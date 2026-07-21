using CarRental.Application.Customer;
using CarRental.Application.Customer.GetCurrentCustomer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.Controllers;

[ApiController]
[Authorize]
[Route("api/customers/me")]
public sealed class CurrentCustomerController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCurrentCustomerQuery(), cancellationToken);
        return result.Match<IActionResult>(Ok, error =>
        {
            var status = error == CustomerErrors.ProfileAlreadyLinked
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status422UnprocessableEntity;
            var problem = new ProblemDetails
            {
                Status = status,
                Title = "Member profile could not be resolved",
                Detail = error.Description,
                Instance = Request.Path
            };
            problem.Extensions["code"] = error.Code;
            problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            return StatusCode(status, problem);
        });
    }
}
