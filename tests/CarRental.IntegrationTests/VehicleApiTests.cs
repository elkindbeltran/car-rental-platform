using CarRental.Application.Inventory;
using CarRental.Domain.Inventory;
using System.Net;
using System.Net.Http.Json;

namespace CarRental.IntegrationTests;

public sealed class VehicleApiTests(CarRentalApiFactory factory) : IClassFixture<CarRentalApiFactory>
{
    [Fact]
    public async Task GetVehicles_WithAuthenticatedUser_ReturnsPage()
    {
        using var client = factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/vehicles?page=1&pageSize=10&status=Available");
        var page = await response.Content.ReadFromJsonAsync<VehiclePage>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(page);
        Assert.Equal(1, page.Page);
        Assert.Equal(10, page.PageSize);
    }

    [Fact]
    public async Task CreateVehicle_WithoutAdministratorRole_ReturnsForbidden()
    {
        using var client = factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/vehicles", ValidVehicleRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateThenGetVehicle_AsAdministrator_PersistsNormalizedVehicle()
    {
        using var client = factory.CreateAuthenticatedClient(administrator: true);
        var createResponse = await client.PostAsJsonAsync("/api/vehicles", ValidVehicleRequest());

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<VehicleResponse>();
        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/api/vehicles/{created.Id}");
        var vehicle = await getResponse.Content.ReadFromJsonAsync<VehicleResponse>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(vehicle);
        Assert.Equal(created.Vin.ToUpperInvariant(), vehicle.Vin);
        Assert.Equal(created.LicensePlate.ToUpperInvariant(), vehicle.LicensePlate);
        Assert.Equal(VehicleStatus.Available, vehicle.Status);
    }

    [Fact]
    public async Task CreateVehicle_WithDuplicateVin_ReturnsConflict()
    {
        using var client = factory.CreateAuthenticatedClient(administrator: true);
        var request = ValidVehicleRequest();
        var firstResponse = await client.PostAsJsonAsync("/api/vehicles", request);

        var secondResponse = await client.PostAsJsonAsync("/api/vehicles", request with
        {
            LicensePlate = $"P-{Guid.NewGuid():N}"[..12]
        });

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    private static VehicleRequest ValidVehicleRequest()
    {
        var identifier = Guid.NewGuid().ToString("N").ToUpperInvariant();
        return new VehicleRequest(identifier[..17], "Honda", "Accord", 2025, $"P-{identifier[..8]}", 120m, "USD");
    }

    private sealed record VehicleRequest(string Vin, string Make, string Model, int Year, string LicensePlate, decimal DailyRate, string Currency);
}
