using AutoMapper;
using CarRental.Application.Abstractions.Messaging;
using CarRental.Domain.Customer;
using CarRental.SharedKernel.Application;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Customer.UpdateCustomer;

internal sealed class UpdateCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    : ICommandHandler<UpdateCustomerCommand, Result<CustomerResponse>>
{
    public async Task<Result<CustomerResponse>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (customer is null) return Result.Failure<CustomerResponse>(CustomerErrors.NotFound);
        if (await repository.EmailExistsAsync(request.Email.Trim(), request.Id, cancellationToken))
            return Result.Failure<CustomerResponse>(CustomerErrors.EmailInUse);

        repository.SetOriginalRowVersion(customer, Convert.FromBase64String(request.RowVersion));
        customer.UpdateDetails(request.FirstName, request.LastName, request.Email, request.Phone);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(mapper.Map<CustomerResponse>(customer));
    }
}
