using AutoMapper;
using CarRental.Application.Abstractions.Messaging;
using CarRental.Domain.Inventory;
using CarRental.SharedKernel.Application;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Inventory.CreateVehicle;

internal sealed class CreateVehicleCommandHandler(IVehicleRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    : ICommandHandler<CreateVehicleCommand, Result<VehicleResponse>>
{
    public async Task<Result<VehicleResponse>> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        if (await repository.VinExistsAsync(request.Vin.Trim(), cancellationToken))
            return Result.Failure<VehicleResponse>(VehicleErrors.VinInUse);
        if (await repository.LicensePlateExistsAsync(request.LicensePlate.Trim(), null, cancellationToken))
            return Result.Failure<VehicleResponse>(VehicleErrors.LicensePlateInUse);

        var vehicle = Vehicle.Create(Guid.NewGuid(), request.Vin, request.Make, request.Model, request.Year, request.LicensePlate, request.DailyRate, request.Currency);
        repository.Add(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(mapper.Map<VehicleResponse>(vehicle));
    }
}
