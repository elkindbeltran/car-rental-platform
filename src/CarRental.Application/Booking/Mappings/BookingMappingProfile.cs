using AutoMapper;
using CarRental.Application.Booking.CreateBooking;

namespace CarRental.Application.Booking.Mappings;

public sealed class BookingMappingProfile : Profile
{
    public BookingMappingProfile()
    {
        CreateMap<Domain.Booking.Booking, BookingResponse>()
            .ForCtorParam(nameof(BookingResponse.Status), options => options.MapFrom(source => source.Status.ToString()));
    }
}
