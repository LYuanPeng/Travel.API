using System;
using System.Collections.Generic;
using Travel.API.Models;
using System.Threading.Tasks;
using Travel.API.Helper;

namespace Travel.API.Services
{
    public interface IToursitRouteRepository
    {
        Task<PaginationList<TouristRoute>> GetTouristRoutesAsync(
            string keyword, string ratingOperator, int? ratingValue,
            int pageSize, int pageNumber, string orderBy);
        Task<TouristRoute> GetTouristRouteAsync(Guid touristRouteId);
        Task<bool> TouristRouteExistsAsync(Guid touristRouteId);
        Task<IEnumerable<TouristRoutePicture>> GetPicturesByTouristRouteIdAsync(Guid touristRouteId);
        Task<TouristRoutePicture> GetPictureAsync(int pictureId);
        void AddTouristRoute(TouristRoute touristRoute);
        void AddTouristRoutePicture(Guid touristRouteId, TouristRoutePicture picture);
        void DeleteTouristRoute(TouristRoute touristRoute);
        void DeleteTouristRoutePicture(TouristRoutePicture picture);
        Task<IEnumerable<TouristRoute>> GetTouristRoutesByIDListAsync(IEnumerable<Guid> ids);
        void DeleteTouristRoutes(IEnumerable<TouristRoute> touristRoutes);
        Task<ShoppingCart> GetShoppingCartByUserIdAsync(string userId);
        Task CreateShoppingCart(ShoppingCart shoppingCart);
        Task AddShoppingCartItem(LineItem lineItem);
        Task<LineItem> GetShoppingCartItemByItemId(int lineItemId);
        void DeleteShoppingCartItem(LineItem lineItem);
        Task<IEnumerable<LineItem>> GetShoppingCartsByIdListAsync(IEnumerable<int> ids);
        void DeleteShoppingCartItems(IEnumerable<LineItem> lineItems);
        Task AddOrderAsync(Order order);
        Task<PaginationList<Order>> GetOrderByUserIdAsync(string userId, int pageSize, int pageNumber);
        Task<Order> GetOrderById(Guid orderId);
        Task<bool> SaveAsync();
    }
}
