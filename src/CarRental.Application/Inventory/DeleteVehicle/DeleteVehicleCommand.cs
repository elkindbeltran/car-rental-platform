using CarRental.Application.Abstractions.Messaging;
using CarRental.Domain.Inventory;
using CarRental.SharedKernel.Application;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Inventory.DeleteVehicle;

public sealed record DeleteVehicleCommand(Guid Id, string RowVersion) : ICommand<Result>;

internal sealed class DeleteVehicleCommandHandler(IVehicleRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteVehicleCommand, Result>
{
    public async Task<Result> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (vehicle is null) return Result.Failure(VehicleErrors.NotFound);
        repository.SetOriginalRowVersion(vehicle, Convert.FromBase64String(request.RowVersion));
        vehicle.Retire();
        repository.Remove(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
