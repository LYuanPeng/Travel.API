using AutoMapper;
using Travel.API.Dtos;
using Travel.API.Models;

namespace Travel.API.Profiles
{
    public class ShoppingCartProfile : Profile
    {
        public ShoppingCartProfile()
        {

            CreateMap<ShoppingCart, ShoppingCartDto>();
            CreateMap<LineItem, LineItemDto>();

        }
    }
}
