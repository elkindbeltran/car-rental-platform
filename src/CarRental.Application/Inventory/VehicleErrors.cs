using CarRental.SharedKernel.Results;

namespace CarRental.Application.Inventory;

public static class VehicleErrors
{
    public static ResultError NotFound { get; } = ResultError.Create("Vehicle.NotFound", "The vehicle was not found.");
    public static ResultError VinInUse { get; } = ResultError.Create("Vehicle.VinInUse", "The VIN is already assigned to a vehicle.");
    public static ResultError LicensePlateInUse { get; } = ResultError.Create("Vehicle.LicensePlateInUse", "The license plate is already assigned to a vehicle.");
}
