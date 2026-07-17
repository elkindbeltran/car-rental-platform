using CarRental.Domain.Inventory;

namespace CarRental.Application.Inventory;

public sealed record VehicleResponse(Guid Id, string Vin, string Make, string Model, int Year, string LicensePlate, decimal DailyRate, string Currency, VehicleStatus Status, string RowVersion);
public sealed record VehicleSummary(Guid Id, string Make, string Model, int Year, string LicensePlate, decimal DailyRate, string Currency, VehicleStatus Status);
public sealed record VehiclePage(IReadOnlyCollection<VehicleSummary> Items, int Page, int PageSize, int TotalCount);

public interface IVehicleReadService
{
    Task<VehicleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<VehiclePage> GetPageAsync(int page, int pageSize, VehicleStatus? status, string? search, CancellationToken cancellationToken);
}
