using FluentValidation;

namespace CarRental.Application.Customer.DeleteCustomer;

internal sealed class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RowVersion).NotEmpty().Must(value =>
        {
            try { _ = Convert.FromBase64String(value); return true; }
            catch (FormatException) { return false; }
        }).WithMessage("RowVersion must be valid Base64.");
    }
}
