using AutoMapper;
using CarRental.Application.Abstractions.Messaging;
using CarRental.Domain.Inventory;
using CarRental.SharedKernel.Application;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Inventory.UpdateVehicle;

internal sealed class UpdateVehicleCommandHandler(IVehicleRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    : ICommandHandler<UpdateVehicleCommand, Result<VehicleResponse>>
{
    public async Task<Result<VehicleResponse>> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (vehicle is null) return Result.Failure<VehicleResponse>(VehicleErrors.NotFound);
        if (await repository.LicensePlateExistsAsync(request.LicensePlate.Trim(), request.Id, cancellationToken))
            return Result.Failure<VehicleResponse>(VehicleErrors.LicensePlateInUse);

        repository.SetOriginalRowVersion(vehicle, Convert.FromBase64String(request.RowVersion));
        vehicle.UpdateDetails(request.Make, request.Model, request.Year, request.LicensePlate, request.DailyRate, request.Currency);
        vehicle.ChangeStatus(request.Status);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(mapper.Map<VehicleResponse>(vehicle));
    }
}
