using CarRental.Application.Abstractions.Messaging;
using CarRental.Domain.Inventory;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Inventory.UpdateVehicle;

public sealed record UpdateVehicleCommand(Guid Id, string Make, string Model, int Year, string LicensePlate, decimal DailyRate, string Currency, VehicleStatus Status, string RowVersion)
    : ICommand<Result<VehicleResponse>>;
