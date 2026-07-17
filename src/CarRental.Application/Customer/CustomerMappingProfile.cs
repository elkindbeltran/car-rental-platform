using AutoMapper;

namespace CarRental.Application.Customer;

internal sealed class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile() => CreateMap<Domain.Customer.Customer, CustomerResponse>()
        .ForCtorParam(nameof(CustomerResponse.RowVersion), options => options.MapFrom(source => Convert.ToBase64String(source.RowVersion)));
}
