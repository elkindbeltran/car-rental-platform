using CarRental.API.Authentication;
using CarRental.Application.Customer;
using CarRental.Application.Customer.CreateCustomer;
using CarRental.Application.Customer.DeleteCustomer;
using CarRental.Application.Customer.GetCustomer;
using CarRental.Application.Customer.UpdateCustomer;
using CarRental.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.Administrator)]
[Route("api/customers")]
public sealed class CustomersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<CustomerPage>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CustomerPage>> GetPage([FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? search = null, CancellationToken cancellationToken = default) =>
        Ok(await sender.Send(new GetCustomersQuery(page, pageSize, search), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCustomerQuery(id), cancellationToken);
        return result.Match<IActionResult>(Ok, error => ProblemResult(error, StatusCodes.Status404NotFound));
    }

    [HttpPost]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateCustomerCommand(request.FirstName, request.LastName, request.Email, request.Phone), cancellationToken);
        return result.Match<IActionResult>(customer => CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer), error => ProblemResult(error, StatusCodes.Status409Conflict));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateCustomerCommand(id, request.FirstName, request.LastName, request.Email, request.Phone, request.RowVersion), cancellationToken);
        return result.Match<IActionResult>(Ok, error => ProblemResult(error, error == CustomerErrors.NotFound ? StatusCodes.Status404NotFound : StatusCodes.Status409Conflict));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, [FromHeader(Name = "If-Match")] string rowVersion, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteCustomerCommand(id, NormalizeEtag(rowVersion)), cancellationToken);
        return result.IsSuccess ? NoContent() : ProblemResult(result.Error, StatusCodes.Status404NotFound);
    }

    private ObjectResult ProblemResult(ResultError error, int statusCode)
    {
        var problem = new ProblemDetails { Status = statusCode, Title = "Customer request failed", Detail = error.Description, Instance = Request.Path };
        problem.Extensions["code"] = error.Code;
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
        return StatusCode(statusCode, problem);
    }

    private static string NormalizeEtag(string value) => value.Trim().Trim('"');
}

public sealed record CreateCustomerRequest(string FirstName, string LastName, string Email, string? Phone);
public sealed record UpdateCustomerRequest(string FirstName, string LastName, string Email, string? Phone, string RowVersion);
