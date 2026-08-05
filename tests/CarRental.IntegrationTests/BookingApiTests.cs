using CarRental.Application.Booking.CreateBooking;
using CarRental.Application.Customer;
using CarRental.Application.Inventory;
using System.Net;
using System.Net.Http.Json;

namespace CarRental.IntegrationTests;

public sealed class BookingApiTests(CarRentalApiFactory factory) : IClassFixture<CarRentalApiFactory>
{
    [Fact]
    public async Task CreateBooking_WithExistingCustomerAndAvailableVehicle_ReturnsCreated()
    {
        using var administrator = factory.CreateAuthenticatedClient(administrator: true);
        var customer = await CreateCustomerAsync(administrator);
        var vehicle = await CreateVehicleAsync(administrator);
        using var user = factory.CreateAuthenticatedClient(administrator: true);

        var response = await user.PostAsJsonAsync("/api/bookings", BookingRequest(customer.Id, vehicle.Id));
        var booking = await response.Content.ReadFromJsonAsync<BookingResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(booking);
        Assert.Equal(customer.Id, booking.CustomerId);
        Assert.Equal(vehicle.Id, booking.VehicleId);
        Assert.Equal(240m, booking.TotalAmount);
    }

    [Fact]
    public async Task CreateOverlappingBooking_ForSameVehicle_ReturnsConflict()
    {
        using var administrator = factory.CreateAuthenticatedClient(administrator: true);
        var customer = await CreateCustomerAsync(administrator);
        var vehicle = await CreateVehicleAsync(administrator);
        using var user = factory.CreateAuthenticatedClient(administrator: true);
        var request = BookingRequest(customer.Id, vehicle.Id);

        var firstResponse = await user.PostAsJsonAsync("/api/bookings", request);
        var secondResponse = await user.PostAsJsonAsync("/api/bookings", request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task CreateBooking_WithUnknownCustomer_ReturnsNotFound()
    {
        using var administrator = factory.CreateAuthenticatedClient(administrator: true);
        var vehicle = await CreateVehicleAsync(administrator);
        using var user = factory.CreateAuthenticatedClient(administrator: true);

        var response = await user.PostAsJsonAsync("/api/bookings", BookingRequest(Guid.NewGuid(), vehicle.Id));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Member_CanCreateBooking_ForOwnProvisionedCustomer()
    {
        using var administrator = factory.CreateAuthenticatedClient(administrator: true);
        var vehicle = await CreateVehicleAsync(administrator);
        using var member = factory.CreateAuthenticatedClient(email: $"member-{Guid.NewGuid():N}@example.com");
        var customer = await member.GetFromJsonAsync<CustomerResponse>("/api/customers/me");

        var response = await member.PostAsJsonAsync("/api/bookings", BookingRequest(customer!.Id, vehicle.Id));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Member_CannotCreateBooking_ForAnotherCustomer()
    {
        using var administrator = factory.CreateAuthenticatedClient(administrator: true);
        var customer = await CreateCustomerAsync(administrator);
        var vehicle = await CreateVehicleAsync(administrator);
        using var member = factory.CreateAuthenticatedClient(email: $"member-{Guid.NewGuid():N}@example.com");
        await member.GetAsync("/api/customers/me");

        var response = await member.PostAsJsonAsync("/api/bookings", BookingRequest(customer.Id, vehicle.Id));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Member_WithoutEmailClaim_CanResolveProfileFromUserInfo()
    {
        using var member = factory.CreateAuthenticatedClient(userId: "userinfo-member");

        var response = await member.GetAsync("/api/customers/me");
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("userinfo-member@example.com", customer!.Email);
    }

    private static object BookingRequest(Guid customerId, Guid vehicleId) => new
    {
        CustomerId = customerId,
        VehicleId = vehicleId,
        PickupAtUtc = new DateTimeOffset(2026, 8, 1, 10, 0, 0, TimeSpan.Zero),
        ReturnAtUtc = new DateTimeOffset(2026, 8, 3, 10, 0, 0, TimeSpan.Zero),
        DailyRate = 120m,
        Currency = "USD"
    };

    private static async Task<CustomerResponse> CreateCustomerAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/customers", new
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = $"customer-{Guid.NewGuid():N}@example.com",
            Phone = (string?)null
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CustomerResponse>())!;
    }

    private static async Task<VehicleResponse> CreateVehicleAsync(HttpClient client)
    {
        var identifier = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var response = await client.PostAsJsonAsync("/api/vehicles", new
        {
            Vin = identifier[..17],
            Make = "Honda",
            Model = "Accord",
            Year = 2025,
            LicensePlate = $"P-{identifier[..8]}",
            DailyRate = 120m,
            Currency = "USD"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<VehicleResponse>())!;
    }
}
