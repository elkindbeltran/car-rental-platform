using CarRental.SharedKernel.Application;
using FluentValidation;

namespace CarRental.Application.Booking.CreateBooking;

internal sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(command => command.CustomerId).NotEmpty();
        RuleFor(command => command.VehicleId).NotEmpty();
        RuleFor(command => command.PickupAtUtc)
            .GreaterThan(dateTimeProvider.UtcNow)
            .WithMessage("Pickup time must be in the future.");
        RuleFor(command => command.ReturnAtUtc)
            .GreaterThan(command => command.PickupAtUtc)
            .WithMessage("Return time must be later than pickup time.");
        RuleFor(command => command)
            .Must(command => command.ReturnAtUtc - command.PickupAtUtc <= TimeSpan.FromDays(Domain.Booking.Booking.MaximumRentalDays))
            .WithMessage($"A booking cannot exceed {Domain.Booking.Booking.MaximumRentalDays} days.");
        RuleFor(command => command.DailyRate)
            .GreaterThan(0)
            .LessThanOrEqualTo(Domain.Booking.Booking.MaximumDailyRate);
        RuleFor(command => command.Currency)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Za-z]{3}$")
            .WithMessage("Currency must be a three-letter ISO currency code.");
    }
}
