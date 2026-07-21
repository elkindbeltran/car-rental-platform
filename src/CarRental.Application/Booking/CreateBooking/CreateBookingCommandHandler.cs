using AutoMapper;
using CarRental.Application.Abstractions.Messaging;
using CarRental.Application.Booking.Abstractions;
using CarRental.Application.Booking.IntegrationEvents;
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
    IIntegrationEventPublisher eventPublisher,
    ICurrentUserService currentUser,
    IMapper mapper) : ICommandHandler<CreateBookingCommand, Result<BookingResponse>>
{
    public async Task<Result<BookingResponse>> Handle(
        CreateBookingCommand request,
        CancellationToken cancellationToken)
    {
        var customerId = request.CustomerId;
        if (!currentUser.IsInRole("Administrator"))
        {
            var ownedCustomerId = string.IsNullOrWhiteSpace(currentUser.UserId)
                ? null
                : await customerReader.GetIdByExternalUserIdAsync(currentUser.UserId, cancellationToken);
            if (!ownedCustomerId.HasValue || ownedCustomerId.Value != customerId)
            {
                return Result.Failure<BookingResponse>(BookingErrors.CustomerForbidden);
            }
        }

        if (!await customerReader.ExistsAsync(customerId, cancellationToken))
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

        var occurredOnUtc = dateTimeProvider.UtcNow;
        var booking = Domain.Booking.Booking.Create(
            Guid.NewGuid(),
            customerId,
            request.VehicleId,
            request.PickupAtUtc,
            request.ReturnAtUtc,
            request.DailyRate,
            request.Currency,
            occurredOnUtc);

        bookingRepository.Add(booking);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new BookingCreatedIntegrationEvent(
                Guid.NewGuid(),
                occurredOnUtc,
                booking.Id,
                booking.CustomerId,
                booking.VehicleId,
                booking.PickupAtUtc,
                booking.ReturnAtUtc,
                booking.TotalAmount,
                booking.Currency),
            cancellationToken);

        return Result.Success(mapper.Map<BookingResponse>(booking));
    }
}
