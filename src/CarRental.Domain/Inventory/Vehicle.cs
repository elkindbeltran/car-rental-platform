using CarRental.SharedKernel.Domain;
using CarRental.SharedKernel.Exceptions;

namespace CarRental.Domain.Inventory;

public sealed class Vehicle : AggregateRoot<Guid>, IAuditableEntity, IConcurrencyTracked, ISoftDeletable
{
    public const decimal MaximumDailyRate = 100_000m;

    private Vehicle(
        Guid id,
        string vin,
        string make,
        string model,
        int year,
        string licensePlate,
        decimal dailyRate,
        string currency) : base(id)
    {
        Vin = Normalize(vin, 17, "VIN").ToUpperInvariant();
        UpdateDetails(make, model, year, licensePlate, dailyRate, currency);
        Status = VehicleStatus.Available;
    }

    private Vehicle() { }

    public string Vin { get; private init; } = string.Empty;
    public string Make { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public string LicensePlate { get; private set; } = string.Empty;
    public decimal DailyRate { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public VehicleStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    public static Vehicle Create(Guid id, string vin, string make, string model, int year, string licensePlate, decimal dailyRate, string currency)
    {
        if (id == Guid.Empty)
        {
            throw new BusinessException("Vehicle.InvalidId", "A vehicle identifier is required.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(vin);
        var normalizedVin = vin.Trim();
        if (normalizedVin.Length != 17 ||
            normalizedVin.Any(character => !char.IsAsciiLetterOrDigit(character) || character is 'I' or 'i' or 'O' or 'o' or 'Q' or 'q'))
        {
            throw new BusinessException("Vehicle.InvalidVin", "VIN must contain 17 alphanumeric characters and cannot contain I, O, or Q.");
        }

        return new Vehicle(id, vin, make, model, year, licensePlate, dailyRate, currency);
    }

    public void UpdateDetails(string make, string model, int year, string licensePlate, decimal dailyRate, string currency)
    {
        Make = Normalize(make, 100, "make");
        Model = Normalize(model, 100, "model");
        LicensePlate = Normalize(licensePlate, 20, "license plate").ToUpperInvariant();
        if (year is < 1886 or > 2200)
        {
            throw new BusinessException("Vehicle.InvalidYear", "Vehicle year is outside the supported range.");
        }

        if (dailyRate is <= 0 or > MaximumDailyRate)
        {
            throw new BusinessException("Vehicle.InvalidDailyRate", $"Daily rate must be greater than zero and no more than {MaximumDailyRate}.");
        }

        var normalizedCurrency = Normalize(currency, 3, "currency");
        if (normalizedCurrency.Length != 3 || !normalizedCurrency.All(char.IsAsciiLetter))
        {
            throw new BusinessException("Vehicle.InvalidCurrency", "Currency must be a three-letter ISO currency code.");
        }

        Year = year;
        DailyRate = dailyRate;
        Currency = normalizedCurrency.ToUpperInvariant();
    }

    public void ChangeStatus(VehicleStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new BusinessException("Vehicle.InvalidStatus", "Vehicle status is invalid.");
        }

        Status = status;
    }

    public void Retire() => Status = VehicleStatus.Retired;

    private static string Normalize(string value, int maximumLength, string fieldName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new BusinessException("Vehicle.InvalidValue", $"Vehicle {fieldName} cannot exceed {maximumLength} characters.");
        }

        return normalized;
    }
}
