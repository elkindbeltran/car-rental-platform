using CarRental.Application.Booking.CreateBooking;
using CarRental.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.Controllers;

[ApiController]
[Authorize]
[Route("api/bookings")]
public sealed class BookingsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateBookingCommand(
            request.CustomerId,
            request.VehicleId,
            request.PickupAtUtc,
            request.ReturnAtUtc,
            request.DailyRate,
            request.Currency);

        var result = await sender.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            booking => Created($"/api/bookings/{booking.Id}", booking),
            error => CreateProblemResult(error));
    }

    private ObjectResult CreateProblemResult(ResultError error)
    {
        var statusCode = error.Code switch
        {
            "Booking.CustomerNotFound" or "Booking.VehicleNotRentable" => StatusCodes.Status404NotFound,
            "Booking.VehicleUnavailable" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status422UnprocessableEntity
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = "Booking could not be created",
            Detail = error.Description,
            Instance = HttpContext.Request.Path
        };
        problem.Extensions["code"] = error.Code;
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;

        return StatusCode(statusCode, problem);
    }
}

public sealed record CreateBookingRequest(
    Guid CustomerId,
    Guid VehicleId,
    DateTimeOffset PickupAtUtc,
    DateTimeOffset ReturnAtUtc,
    decimal DailyRate,
    string Currency);
