using CarRental.Application.Abstractions.Messaging;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Inventory.CreateVehicle;

public sealed record CreateVehicleCommand(string Vin, string Make, string Model, int Year, string LicensePlate, decimal DailyRate, string Currency)
    : ICommand<Result<VehicleResponse>>;
