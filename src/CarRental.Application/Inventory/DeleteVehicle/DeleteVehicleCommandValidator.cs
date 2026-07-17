using FluentValidation;

namespace CarRental.Application.Inventory.DeleteVehicle;

internal sealed class DeleteVehicleCommandValidator : AbstractValidator<DeleteVehicleCommand>
{
    public DeleteVehicleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RowVersion).NotEmpty().Must(value =>
        {
            try { _ = Convert.FromBase64String(value); return true; }
            catch (FormatException) { return false; }
        }).WithMessage("RowVersion must be valid Base64.");
    }
}
