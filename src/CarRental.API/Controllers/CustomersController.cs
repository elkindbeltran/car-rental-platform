using CarRental.API.Authentication;
using CarRental.Application.Abstractions.Messaging;
using CarRental.Application.Customer;
using CarRental.Application.Customer.CreateCustomer;
using CarRental.Application.Customer.DeleteCustomer;
using CarRental.Application.Customer.GetCustomer;
using CarRental.Application.Customer.UpdateCustomer;
using CarRental.SharedKernel.Results;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.Controllers;

public static class CustomersEndpoints
{
    public static IEndpointRouteBuilder MapCustomersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var customers = endpoints.MapGroup("api/customers")
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithTags("Customers");

        customers.MapGet("", GetPageAsync)
            .Produces<CustomerPage>();

        customers.MapGet("{id:guid}", GetByIdAsync)
            .Produces<CustomerResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        customers.MapPost("", CreateAsync)
            .Produces<CustomerResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict);

        customers.MapPut("{id:guid}", UpdateAsync)
            .Produces<CustomerResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        customers.MapDelete("{id:guid}", DeleteAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> GetPageAsync(
        IApplicationMediator mediator,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 25,
        string? search = null) =>
        Results.Ok(await mediator.SendAsync(
            new GetCustomersQuery(page, pageSize, search),
            cancellationToken));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IApplicationMediator mediator,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new GetCustomerQuery(id), cancellationToken);
        return result.Match<IResult>(
            Results.Ok,
            error => CustomerProblem(error, StatusCodes.Status404NotFound, context));
    }

    private static async Task<IResult> CreateAsync(
        CreateCustomerRequest request,
        IApplicationMediator mediator,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new CreateCustomerCommand(request.FirstName, request.LastName, request.Email, request.Phone),
            cancellationToken);

        return result.Match<IResult>(
            customer => Results.Created($"/api/customers/{customer.Id}", customer),
            error => CustomerProblem(error, StatusCodes.Status409Conflict, context));
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateCustomerRequest request,
        IApplicationMediator mediator,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new UpdateCustomerCommand(
                id,
                request.FirstName,
                request.LastName,
                request.Email,
                request.Phone,
                request.RowVersion),
            cancellationToken);

        return result.Match<IResult>(
            Results.Ok,
            error => CustomerProblem(
                error,
                error == CustomerErrors.NotFound
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status409Conflict,
                context));
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        [FromHeader(Name = "If-Match")] string rowVersion,
        IApplicationMediator mediator,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new DeleteCustomerCommand(id, NormalizeEtag(rowVersion)),
            cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : CustomerProblem(result.Error, StatusCodes.Status404NotFound, context);
    }

    private static IResult CustomerProblem(ResultError error, int statusCode, HttpContext context) =>
        Results.Problem(
            statusCode: statusCode,
            title: "Customer request failed",
            detail: error.Description,
            instance: context.Request.Path,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = error.Code,
                ["traceId"] = context.TraceIdentifier
            });

    private static string NormalizeEtag(string value) => value.Trim().Trim('"');
}

public sealed record CreateCustomerRequest(string FirstName, string LastName, string Email, string? Phone);
public sealed record UpdateCustomerRequest(string FirstName, string LastName, string Email, string? Phone, string RowVersion);
