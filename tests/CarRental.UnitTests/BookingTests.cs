using CarRental.Domain.Booking;
using CarRental.SharedKernel.Exceptions;

namespace CarRental.UnitTests;

public sealed class BookingTests
{
    private static readonly DateTimeOffset PickupAtUtc = new(2026, 7, 17, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ForPartialDays_RoundsTotalUpAndRaisesDomainEvent()
    {
        var booking = Booking.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            PickupAtUtc,
            PickupAtUtc.AddHours(25),
            80m,
            "usd",
            PickupAtUtc.AddMinutes(-5));

        Assert.Equal(160m, booking.TotalAmount);
        Assert.Equal("USD", booking.Currency);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        var domainEvent = Assert.Single(booking.DomainEvents);
        Assert.IsType<BookingCreatedDomainEvent>(domainEvent);
    }

    [Fact]
    public void Create_ForLessThanOneDay_ChargesMinimumOneDay()
    {
        var booking = CreateBooking(PickupAtUtc, PickupAtUtc.AddHours(2), 75m);

        Assert.Equal(75m, booking.TotalAmount);
    }

    [Fact]
    public void Create_WhenReturnIsNotAfterPickup_ThrowsExpectedBusinessError()
    {
        var exception = Assert.Throws<BusinessException>(() => CreateBooking(PickupAtUtc, PickupAtUtc, 75m));

        Assert.Equal("Booking.InvalidPeriod", exception.Code);
    }

    [Fact]
    public void Create_WhenPeriodExceedsMaximum_ThrowsExpectedBusinessError()
    {
        var exception = Assert.Throws<BusinessException>(() =>
            CreateBooking(PickupAtUtc, PickupAtUtc.AddDays(Booking.MaximumRentalDays + 1), 75m));

        Assert.Equal("Booking.PeriodTooLong", exception.Code);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("123")]
    [InlineData("USDD")]
    public void Create_WithInvalidCurrency_ThrowsExpectedBusinessError(string currency)
    {
        var exception = Assert.Throws<BusinessException>(() =>
            Booking.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), PickupAtUtc, PickupAtUtc.AddDays(1), 75m, currency, PickupAtUtc));

        Assert.Equal("Booking.InvalidCurrency", exception.Code);
    }

    private static Booking CreateBooking(DateTimeOffset pickup, DateTimeOffset @return, decimal dailyRate) =>
        Booking.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), pickup, @return, dailyRate, "USD", pickup.AddMinutes(-1));
}
