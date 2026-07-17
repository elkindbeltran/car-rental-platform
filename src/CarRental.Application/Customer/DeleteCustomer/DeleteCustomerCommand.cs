using CarRental.Application.Abstractions.Messaging;
using CarRental.Domain.Customer;
using CarRental.SharedKernel.Application;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Customer.DeleteCustomer;

public sealed record DeleteCustomerCommand(Guid Id, string RowVersion) : ICommand<Result>;

internal sealed class DeleteCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCustomerCommand, Result>
{
    public async Task<Result> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (customer is null) return Result.Failure(CustomerErrors.NotFound);
        repository.SetOriginalRowVersion(customer, Convert.FromBase64String(request.RowVersion));
        customer.Deactivate();
        repository.Remove(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
