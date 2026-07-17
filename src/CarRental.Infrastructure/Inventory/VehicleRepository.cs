using CarRental.Application.Booking.Abstractions;
using CarRental.Application.Inventory;
using CarRental.Domain.Inventory;
using CarRental.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Inventory;

internal sealed class VehicleRepository(ApplicationDbContext dbContext)
    : IVehicleRepository, IVehicleBookingReader, IVehicleReadService
{
    public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Vehicles.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Add(Vehicle aggregate) => dbContext.Vehicles.Add(aggregate);
    public void Remove(Vehicle aggregate) => dbContext.Vehicles.Remove(aggregate);

    public Task<bool> VinExistsAsync(string vin, CancellationToken cancellationToken = default)
    {
        var normalized = vin.Trim().ToUpperInvariant();
        return dbContext.Vehicles.IgnoreQueryFilters().AnyAsync(x => x.Vin == normalized, cancellationToken);
    }

    public Task<bool> LicensePlateExistsAsync(string licensePlate, Guid? excludingVehicleId, CancellationToken cancellationToken = default)
    {
        var normalized = licensePlate.Trim().ToUpperInvariant();
        return dbContext.Vehicles.AnyAsync(x => x.LicensePlate == normalized && (!excludingVehicleId.HasValue || x.Id != excludingVehicleId.Value), cancellationToken);
    }

    public Task<bool> IsRentableAsync(Guid vehicleId, CancellationToken cancellationToken = default) =>
        dbContext.Vehicles.AnyAsync(x => x.Id == vehicleId && x.Status == VehicleStatus.Available, cancellationToken);

    public void SetOriginalRowVersion(Vehicle vehicle, byte[] rowVersion) =>
        dbContext.Entry(vehicle).Property(x => x.RowVersion).OriginalValue = rowVersion;

    async Task<VehicleResponse?> IVehicleReadService.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var vehicle = await dbContext.Vehicles.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.Id, x.Vin, x.Make, x.Model, x.Year, x.LicensePlate, x.DailyRate, x.Currency, x.Status, x.RowVersion })
            .SingleOrDefaultAsync(cancellationToken);

        return vehicle is null
            ? null
            : new VehicleResponse(vehicle.Id, vehicle.Vin, vehicle.Make, vehicle.Model, vehicle.Year, vehicle.LicensePlate, vehicle.DailyRate, vehicle.Currency, vehicle.Status, Convert.ToBase64String(vehicle.RowVersion));
    }

    public async Task<VehiclePage> GetPageAsync(int page, int pageSize, VehicleStatus? status, string? search, CancellationToken cancellationToken)
    {
        var query = dbContext.Vehicles.AsNoTracking();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Make.Contains(term) || x.Model.Contains(term) || x.LicensePlate.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Make).ThenBy(x => x.Model).ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new VehicleSummary(x.Id, x.Make, x.Model, x.Year, x.LicensePlate, x.DailyRate, x.Currency, x.Status))
            .ToListAsync(cancellationToken);
        return new VehiclePage(items, page, pageSize, totalCount);
    }
}
