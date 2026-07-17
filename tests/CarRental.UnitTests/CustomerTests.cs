using CarRental.Domain.Customer;
using CarRental.SharedKernel.Exceptions;

namespace CarRental.UnitTests;

public sealed class CustomerTests
{
    [Fact]
    public void Create_WithValidDetails_NormalizesValuesAndActivatesCustomer()
    {
        var id = Guid.NewGuid();

        var customer = Customer.Create(id, "  Ada ", " Lovelace  ", " ADA@Example.COM ", " +57 300 000 0000 ");

        Assert.Equal(id, customer.Id);
        Assert.Equal("Ada", customer.FirstName);
        Assert.Equal("Lovelace", customer.LastName);
        Assert.Equal("ada@example.com", customer.Email);
        Assert.Equal("+57 300 000 0000", customer.Phone);
        Assert.True(customer.IsActive);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("name@example.com trailing")]
    public void Create_WithInvalidEmail_ThrowsBusinessException(string email)
    {
        var exception = Assert.Throws<BusinessException>(() =>
            Customer.Create(Guid.NewGuid(), "Ada", "Lovelace", email, null));

        Assert.Equal("Customer.InvalidEmail", exception.Code);
    }

    [Fact]
    public void UpdateDetails_ReplacesMutableDetails()
    {
        var customer = Customer.Create(Guid.NewGuid(), "Ada", "Lovelace", "ada@example.com", null);

        customer.UpdateDetails("Grace", "Hopper", "grace@example.com", "123");

        Assert.Equal("Grace", customer.FirstName);
        Assert.Equal("Hopper", customer.LastName);
        Assert.Equal("grace@example.com", customer.Email);
        Assert.Equal("123", customer.Phone);
    }

    [Fact]
    public void Deactivate_MarksCustomerInactive()
    {
        var customer = Customer.Create(Guid.NewGuid(), "Ada", "Lovelace", "ada@example.com", null);

        customer.Deactivate();

        Assert.False(customer.IsActive);
    }

    [Fact]
    public void Create_WithEmptyId_ThrowsExpectedBusinessError()
    {
        var exception = Assert.Throws<BusinessException>(() =>
            Customer.Create(Guid.Empty, "Ada", "Lovelace", "ada@example.com", null));

        Assert.Equal("Customer.InvalidId", exception.Code);
    }
}
