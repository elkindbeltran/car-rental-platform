using FluentValidation;

namespace CarRental.Application.Customer.UpdateCustomer;

internal sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(Domain.Customer.Customer.MaximumNameLength);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(Domain.Customer.Customer.MaximumNameLength);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(Domain.Customer.Customer.MaximumEmailLength);
        RuleFor(x => x.Phone).MaximumLength(Domain.Customer.Customer.MaximumPhoneLength);
        RuleFor(x => x.RowVersion).NotEmpty().Must(BeBase64).WithMessage("RowVersion must be valid Base64.");
    }

    private static bool BeBase64(string value)
    {
        try { _ = Convert.FromBase64String(value); return true; }
        catch (FormatException) { return false; }
    }
}
