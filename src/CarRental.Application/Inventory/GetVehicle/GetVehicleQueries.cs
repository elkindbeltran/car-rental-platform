using CarRental.Application.Abstractions.Messaging;
using CarRental.Domain.Inventory;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Inventory.GetVehicle;

public sealed record GetVehicleQuery(Guid Id) : IQuery<Result<VehicleResponse>>;
public sealed record GetVehiclesQuery(int Page, int PageSize, VehicleStatus? Status, string? Search) : IQuery<VehiclePage>;

internal sealed class GetVehicleQueryHandler(IVehicleReadService readService) : IQueryHandler<GetVehicleQuery, Result<VehicleResponse>>
{
    public async Task<Result<VehicleResponse>> Handle(GetVehicleQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await readService.GetByIdAsync(request.Id, cancellationToken);
        return vehicle is null ? Result.Failure<VehicleResponse>(VehicleErrors.NotFound) : Result.Success(vehicle);
    }
}

internal sealed class GetVehiclesQueryHandler(IVehicleReadService readService) : IQueryHandler<GetVehiclesQuery, VehiclePage>
{
    public Task<VehiclePage> Handle(GetVehiclesQuery request, CancellationToken cancellationToken) =>
        readService.GetPageAsync(request.Page, request.PageSize, request.Status, request.Search, cancellationToken);
}
