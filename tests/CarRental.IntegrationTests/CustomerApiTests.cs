using CarRental.Application.Customer;
using System.Net;
using System.Net.Http.Json;

namespace CarRental.IntegrationTests;

public sealed class CustomerApiTests(CarRentalApiFactory factory) : IClassFixture<CarRentalApiFactory>
{
    [Fact]
    public async Task GetCustomers_WithoutJwt_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/customers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomers_WithoutAdministratorRole_ReturnsForbidden()
    {
        using var client = factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/customers");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateThenGetCustomer_AsAdministrator_PersistsNormalizedCustomer()
    {
        using var client = factory.CreateAuthenticatedClient(administrator: true);
        var createResponse = await client.PostAsJsonAsync("/api/customers", new
        {
            FirstName = "  Ada ",
            LastName = " Lovelace ",
            Email = $"ADA-{Guid.NewGuid():N}@Example.COM",
            Phone = " 12345 "
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/api/customers/{created.Id}");
        var customer = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(customer);
        Assert.Equal("Ada", customer.FirstName);
        Assert.Equal("Lovelace", customer.LastName);
        Assert.Equal(created.Email.ToLowerInvariant(), customer.Email);
        Assert.Equal("12345", customer.Phone);
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidEmail_ReturnsValidationProblem()
    {
        using var client = factory.CreateAuthenticatedClient(administrator: true);

        var response = await client.PostAsJsonAsync("/api/customers", new
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "invalid",
            Phone = (string?)null
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
