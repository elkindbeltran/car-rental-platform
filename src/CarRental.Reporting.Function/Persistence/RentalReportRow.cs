namespace CarRental.Reporting.Worker.Persistence;

public sealed class RentalReportRow
{
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public DateTimeOffset PickupAtUtc { get; set; }
    public DateTimeOffset ReturnAtUtc { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
