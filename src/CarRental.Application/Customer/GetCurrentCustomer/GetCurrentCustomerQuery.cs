using AutoMapper;
using CarRental.Application.Abstractions.Messaging;
using CarRental.Domain.Customer;
using CarRental.SharedKernel.Application;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Customer.GetCurrentCustomer;

public sealed record GetCurrentCustomerQuery : IQuery<Result<CustomerResponse>>;

internal sealed class GetCurrentCustomerQueryHandler(
    ICurrentUserService currentUser,
    ICurrentUserProfileService profileService,
    ICustomerRepository repository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IQueryHandler<GetCurrentCustomerQuery, Result<CustomerResponse>>
{
    public async Task<Result<CustomerResponse>> Handle(GetCurrentCustomerQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentUser.UserId))
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.ProfileClaimsMissing);
        }

        var customer = await repository.GetByExternalUserIdAsync(currentUser.UserId, cancellationToken);
        if (customer is not null)
        {
            return Result.Success(mapper.Map<CustomerResponse>(customer));
        }

        var profile = string.IsNullOrWhiteSpace(currentUser.Email)
            ? await profileService.GetAsync(cancellationToken)
            : new CurrentUserProfile(currentUser.Email, currentUser.GivenName, currentUser.FamilyName);
        if (profile is null)
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.ProfileClaimsMissing);
        }

        customer = await repository.GetByEmailAsync(profile.Email, cancellationToken);
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
                profile.GivenName ?? "Member",
                profile.FamilyName ?? "Driver",
                profile.Email,
                null);
            customer.LinkToUser(currentUser.UserId);
            repository.Add(customer);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(mapper.Map<CustomerResponse>(customer));
    }
}
