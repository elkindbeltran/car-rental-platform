using FluentValidation;

namespace CarRental.Application.Customer.CreateCustomer;

internal sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(Domain.Customer.Customer.MaximumNameLength);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(Domain.Customer.Customer.MaximumNameLength);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(Domain.Customer.Customer.MaximumEmailLength);
        RuleFor(x => x.Phone).MaximumLength(Domain.Customer.Customer.MaximumPhoneLength);
    }
}
