using CarRental.SharedKernel.Application;

namespace CarRental.Domain.Inventory;

public interface IVehicleRepository : IRepository<Vehicle, Guid>
{
    Task<bool> VinExistsAsync(string vin, CancellationToken cancellationToken = default);
    Task<bool> LicensePlateExistsAsync(string licensePlate, Guid? excludingVehicleId, CancellationToken cancellationToken = default);
    void SetOriginalRowVersion(Vehicle vehicle, byte[] rowVersion);
}
