using CarRental.SharedKernel.Domain;
using CarRental.SharedKernel.Exceptions;

namespace CarRental.Domain.Booking;

public sealed class Booking : AggregateRoot<Guid>, IAuditableEntity, IConcurrencyTracked
{
    public const int MaximumRentalDays = 365;
    public const decimal MaximumDailyRate = 100_000m;

    private Booking(
        Guid id,
        Guid customerId,
        Guid vehicleId,
        DateTimeOffset pickupAtUtc,
        DateTimeOffset returnAtUtc,
        decimal dailyRate,
        string currency,
        DateTimeOffset occurredOnUtc) : base(id)
    {
        CustomerId = customerId;
        VehicleId = vehicleId;
        PickupAtUtc = pickupAtUtc;
        ReturnAtUtc = returnAtUtc;
        DailyRate = dailyRate;
        Currency = currency;
        TotalAmount = CalculateTotalAmount(pickupAtUtc, returnAtUtc, dailyRate);
        Status = BookingStatus.Confirmed;

        RaiseDomainEvent(new BookingCreatedDomainEvent(
            id,
            customerId,
            vehicleId,
            pickupAtUtc,
            returnAtUtc,
            occurredOnUtc));
    }

    private Booking() { }

    public Guid CustomerId { get; private init; }
    public Guid VehicleId { get; private init; }
    public DateTimeOffset PickupAtUtc { get; private set; }
    public DateTimeOffset ReturnAtUtc { get; private set; }
    public decimal DailyRate { get; private init; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private init; } = string.Empty;
    public BookingStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public static Booking Create(
        Guid id,
        Guid customerId,
        Guid vehicleId,
        DateTimeOffset pickupAtUtc,
        DateTimeOffset returnAtUtc,
        decimal dailyRate,
        string currency,
        DateTimeOffset occurredOnUtc)
    {
        if (id == Guid.Empty)
        {
            throw new BusinessException("Booking.InvalidId", "A booking identifier is required.");
        }

        if (customerId == Guid.Empty || vehicleId == Guid.Empty)
        {
            throw new BusinessException("Booking.InvalidReference", "Customer and vehicle identifiers are required.");
        }

        if (pickupAtUtc >= returnAtUtc)
        {
            throw new BusinessException("Booking.InvalidPeriod", "The return time must be later than the pickup time.");
        }

        if (returnAtUtc - pickupAtUtc > TimeSpan.FromDays(MaximumRentalDays))
        {
            throw new BusinessException(
                "Booking.PeriodTooLong",
                $"A booking cannot exceed {MaximumRentalDays} days.");
        }

        if (dailyRate is <= 0 or > MaximumDailyRate)
        {
            throw new BusinessException(
                "Booking.InvalidDailyRate",
                $"The daily rate must be greater than zero and no more than {MaximumDailyRate}.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (currency.Length != 3 || !currency.All(char.IsAsciiLetter))
        {
            throw new BusinessException("Booking.InvalidCurrency", "Currency must be a three-letter ISO currency code.");
        }

        return new Booking(
            id,
            customerId,
            vehicleId,
            pickupAtUtc,
            returnAtUtc,
            dailyRate,
            currency.ToUpperInvariant(),
            occurredOnUtc);
    }

    private static decimal CalculateTotalAmount(
        DateTimeOffset pickupAtUtc,
        DateTimeOffset returnAtUtc,
        decimal dailyRate)
    {
        var rentalDays = Math.Max(1, (int)Math.Ceiling((returnAtUtc - pickupAtUtc).TotalDays));
        return checked(rentalDays * dailyRate);
    }
}
