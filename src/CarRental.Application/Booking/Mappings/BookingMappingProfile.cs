using AutoMapper;
using CarRental.Application.Booking.CreateBooking;

namespace CarRental.Application.Booking.Mappings;

public sealed class BookingMappingProfile : Profile
{
    public BookingMappingProfile()
    {
        CreateMap<Domain.Booking.Booking, BookingResponse>()
            .ForMember(destination => destination.Status, options => options.MapFrom(source => source.Status.ToString()));
    }
}
