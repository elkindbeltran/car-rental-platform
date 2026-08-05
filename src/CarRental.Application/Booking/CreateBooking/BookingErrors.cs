using CarRental.SharedKernel.Results;

namespace CarRental.Application.Booking.CreateBooking;

internal static class BookingErrors
{
    public static ResultError CustomerForbidden { get; } = ResultError.Create(
        "Booking.CustomerForbidden", "Members can only create bookings for their own customer profile.");
    public static readonly ResultError CustomerNotFound = ResultError.Create(
        "Booking.CustomerNotFound",
        "The selected customer does not exist.");

    public static readonly ResultError VehicleNotRentable = ResultError.Create(
        "Booking.VehicleNotRentable",
        "The selected vehicle does not exist or is not rentable.");

    public static readonly ResultError VehicleUnavailable = ResultError.Create(
        "Booking.VehicleUnavailable",
        "The selected vehicle is unavailable for the requested period.");
}
