using AutoMapper;
using CarRental.Application.Abstractions.Messaging;
using CarRental.Domain.Customer;
using CarRental.SharedKernel.Application;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Customer.CreateCustomer;

internal sealed class CreateCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    : ICommandHandler<CreateCustomerCommand, Result<CustomerResponse>>
{
    public async Task<Result<CustomerResponse>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        if (await repository.EmailExistsAsync(request.Email.Trim(), null, cancellationToken))
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.EmailInUse);
        }

        var customer = Domain.Customer.Customer.Create(Guid.NewGuid(), request.FirstName, request.LastName, request.Email, request.Phone);
        repository.Add(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(mapper.Map<CustomerResponse>(customer));
    }
}
