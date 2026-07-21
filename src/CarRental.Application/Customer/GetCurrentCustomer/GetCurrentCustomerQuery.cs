using AutoMapper;
using CarRental.Application.Abstractions.Messaging;
using CarRental.Domain.Customer;
using CarRental.SharedKernel.Application;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Customer.GetCurrentCustomer;

public sealed record GetCurrentCustomerQuery : IQuery<Result<CustomerResponse>>;

internal sealed class GetCurrentCustomerQueryHandler(
    ICurrentUserService currentUser,
    ICustomerRepository repository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IQueryHandler<GetCurrentCustomerQuery, Result<CustomerResponse>>
{
    public async Task<Result<CustomerResponse>> Handle(GetCurrentCustomerQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentUser.UserId) || string.IsNullOrWhiteSpace(currentUser.Email))
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.ProfileClaimsMissing);
        }

        var customer = await repository.GetByExternalUserIdAsync(currentUser.UserId, cancellationToken);
        if (customer is not null)
        {
            return Result.Success(mapper.Map<CustomerResponse>(customer));
        }

        customer = await repository.GetByEmailAsync(currentUser.Email, cancellationToken);
        if (customer is not null)
        {
            if (customer.ExternalUserId is not null)
            {
                return Result.Failure<CustomerResponse>(CustomerErrors.ProfileAlreadyLinked);
            }

            customer.LinkToUser(currentUser.UserId);
        }
        else
        {
            customer = Domain.Customer.Customer.Create(
                Guid.NewGuid(),
                currentUser.GivenName ?? "Member",
                currentUser.FamilyName ?? "Driver",
                currentUser.Email,
                null);
            customer.LinkToUser(currentUser.UserId);
            repository.Add(customer);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(mapper.Map<CustomerResponse>(customer));
    }
}
