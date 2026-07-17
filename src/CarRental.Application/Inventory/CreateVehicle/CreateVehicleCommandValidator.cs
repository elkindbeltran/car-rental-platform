using FluentValidation;

namespace CarRental.Application.Inventory.CreateVehicle;

internal sealed class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.Vin).NotEmpty().Matches("^[A-HJ-NPR-Za-hj-npr-z0-9]{17}$");
        RuleFor(x => x.Make).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Year).InclusiveBetween(1886, 2200);
        RuleFor(x => x.LicensePlate).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DailyRate).GreaterThan(0).LessThanOrEqualTo(Domain.Inventory.Vehicle.MaximumDailyRate);
        RuleFor(x => x.Currency).NotEmpty().Length(3).Matches("^[A-Za-z]{3}$");
    }
}
