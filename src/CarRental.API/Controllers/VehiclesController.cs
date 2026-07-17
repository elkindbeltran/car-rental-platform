using CarRental.API.Authentication;
using CarRental.Application.Inventory;
using CarRental.Application.Inventory.CreateVehicle;
using CarRental.Application.Inventory.DeleteVehicle;
using CarRental.Application.Inventory.GetVehicle;
using CarRental.Application.Inventory.UpdateVehicle;
using CarRental.Domain.Inventory;
using CarRental.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.Controllers;

[ApiController]
[Authorize]
[Route("api/vehicles")]
public sealed class VehiclesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<VehiclePage>(StatusCodes.Status200OK)]
    public async Task<ActionResult<VehiclePage>> GetPage([FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] VehicleStatus? status = null, [FromQuery] string? search = null, CancellationToken cancellationToken = default) =>
        Ok(await sender.Send(new GetVehiclesQuery(page, pageSize, status, search), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetVehicleQuery(id), cancellationToken);
        return result.Match<IActionResult>(Ok, error => ProblemResult(error, StatusCodes.Status404NotFound));
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> Create(CreateVehicleRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateVehicleCommand(request.Vin, request.Make, request.Model, request.Year, request.LicensePlate, request.DailyRate, request.Currency), cancellationToken);
        return result.Match<IActionResult>(vehicle => CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle), error => ProblemResult(error, StatusCodes.Status409Conflict));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> Update(Guid id, UpdateVehicleRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateVehicleCommand(id, request.Make, request.Model, request.Year, request.LicensePlate, request.DailyRate, request.Currency, request.Status, request.RowVersion), cancellationToken);
        return result.Match<IActionResult>(Ok, error => ProblemResult(error, error == VehicleErrors.NotFound ? StatusCodes.Status404NotFound : StatusCodes.Status409Conflict));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> Delete(Guid id, [FromHeader(Name = "If-Match")] string rowVersion, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteVehicleCommand(id, rowVersion.Trim().Trim('"')), cancellationToken);
        return result.IsSuccess ? NoContent() : ProblemResult(result.Error, StatusCodes.Status404NotFound);
    }

    private ObjectResult ProblemResult(ResultError error, int statusCode)
    {
        var problem = new ProblemDetails { Status = statusCode, Title = "Vehicle request failed", Detail = error.Description, Instance = Request.Path };
        problem.Extensions["code"] = error.Code;
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
        return StatusCode(statusCode, problem);
    }
}

public sealed record CreateVehicleRequest(string Vin, string Make, string Model, int Year, string LicensePlate, decimal DailyRate, string Currency);
public sealed record UpdateVehicleRequest(string Make, string Model, int Year, string LicensePlate, decimal DailyRate, string Currency, VehicleStatus Status, string RowVersion);
