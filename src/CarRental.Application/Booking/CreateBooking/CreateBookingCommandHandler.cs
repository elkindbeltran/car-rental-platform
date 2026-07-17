using AutoMapper;
using CarRental.Application.Abstractions.Messaging;
using CarRental.Application.Booking.Abstractions;
using CarRental.Domain.Booking;
using CarRental.SharedKernel.Application;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Booking.CreateBooking;

internal sealed class CreateBookingCommandHandler(
    IBookingRepository bookingRepository,
    ICustomerBookingReader customerReader,
    IVehicleBookingReader vehicleReader,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IMapper mapper) : ICommandHandler<CreateBookingCommand, Result<BookingResponse>>
{
    public async Task<Result<BookingResponse>> Handle(
        CreateBookingCommand request,
        CancellationToken cancellationToken)
    {
        if (!await customerReader.ExistsAsync(request.CustomerId, cancellationToken))
        {
            return Result.Failure<BookingResponse>(BookingErrors.CustomerNotFound);
        }

        if (!await vehicleReader.IsRentableAsync(request.VehicleId, cancellationToken))
        {
            return Result.Failure<BookingResponse>(BookingErrors.VehicleNotRentable);
        }

        var overlaps = await bookingRepository.HasOverlappingBookingAsync(
            request.VehicleId,
            request.PickupAtUtc,
            request.ReturnAtUtc,
            cancellationToken);

        if (overlaps)
        {
            return Result.Failure<BookingResponse>(BookingErrors.VehicleUnavailable);
        }

        var booking = Domain.Booking.Booking.Create(
            Guid.NewGuid(),
            request.CustomerId,
            request.VehicleId,
            request.PickupAtUtc,
            request.ReturnAtUtc,
            request.DailyRate,
            request.Currency,
            dateTimeProvider.UtcNow);

        bookingRepository.Add(booking);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.Map<BookingResponse>(booking));
    }
}
