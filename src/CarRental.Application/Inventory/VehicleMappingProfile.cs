using AutoMapper;

namespace CarRental.Application.Inventory;

internal sealed class VehicleMappingProfile : Profile
{
    public VehicleMappingProfile() => CreateMap<Domain.Inventory.Vehicle, VehicleResponse>()
        .ForCtorParam(nameof(VehicleResponse.RowVersion), options => options.MapFrom(source => Convert.ToBase64String(source.RowVersion)));
}
