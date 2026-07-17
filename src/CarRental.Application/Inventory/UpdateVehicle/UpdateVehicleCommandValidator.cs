using FluentValidation;

namespace CarRental.Application.Inventory.UpdateVehicle;

internal sealed class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehicleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Make).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Year).InclusiveBetween(1886, 2200);
        RuleFor(x => x.LicensePlate).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DailyRate).GreaterThan(0).LessThanOrEqualTo(Domain.Inventory.Vehicle.MaximumDailyRate);
        RuleFor(x => x.Currency).NotEmpty().Length(3).Matches("^[A-Za-z]{3}$");
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.RowVersion).NotEmpty().Must(BeBase64).WithMessage("RowVersion must be valid Base64.");
    }

    private static bool BeBase64(string value)
    {
        try { _ = Convert.FromBase64String(value); return true; }
        catch (FormatException) { return false; }
    }
}
