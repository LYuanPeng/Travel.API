using AutoMapper;
using Travel.API.Dtos;
using Travel.API.Models;

namespace Travel.API.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(
                    dest => dest.OrderState,
                    opt => 
                    {
                        opt.MapFrom(src => src.OrderState.ToString());
                    }
                );
        }
    }
}
