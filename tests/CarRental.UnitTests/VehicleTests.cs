using CarRental.Domain.Inventory;
using CarRental.SharedKernel.Exceptions;

namespace CarRental.UnitTests;

public sealed class VehicleTests
{
    [Fact]
    public void Create_WithValidDetails_NormalizesIdentityAndStartsAvailable()
    {
        var vehicle = Vehicle.Create(Guid.NewGuid(), "1hgcm82633a004352", " Honda ", " Accord ", 2024, " abc-123 ", 125.50m, "usd");

        Assert.Equal("1HGCM82633A004352", vehicle.Vin);
        Assert.Equal("ABC-123", vehicle.LicensePlate);
        Assert.Equal("USD", vehicle.Currency);
        Assert.Equal(VehicleStatus.Available, vehicle.Status);
    }

    [Theory]
    [InlineData("SHORT")]
    [InlineData("1HGCM82633I004352")]
    [InlineData("1HGCM82633O004352")]
    [InlineData("1HGCM82633Q004352")]
    public void Create_WithInvalidVin_ThrowsExpectedBusinessError(string vin)
    {
        var exception = Assert.Throws<BusinessException>(() =>
            Vehicle.Create(Guid.NewGuid(), vin, "Honda", "Accord", 2024, "ABC-123", 100m, "USD"));

        Assert.Equal("Vehicle.InvalidVin", exception.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100001)]
    public void Create_WithInvalidDailyRate_ThrowsExpectedBusinessError(int dailyRate)
    {
        var exception = Assert.Throws<BusinessException>(() =>
            Vehicle.Create(Guid.NewGuid(), "1HGCM82633A004352", "Honda", "Accord", 2024, "ABC-123", dailyRate, "USD"));

        Assert.Equal("Vehicle.InvalidDailyRate", exception.Code);
    }

    [Fact]
    public void ChangeStatus_AndRetire_ApplyLifecycleTransitions()
    {
        var vehicle = Vehicle.Create(Guid.NewGuid(), "1HGCM82633A004352", "Honda", "Accord", 2024, "ABC-123", 100m, "USD");

        vehicle.ChangeStatus(VehicleStatus.Maintenance);
        Assert.Equal(VehicleStatus.Maintenance, vehicle.Status);

        vehicle.Retire();
        Assert.Equal(VehicleStatus.Retired, vehicle.Status);
    }
}
