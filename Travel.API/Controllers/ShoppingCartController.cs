using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Travel.API.Dtos;
using Travel.API.Helper;
using Travel.API.Models;
using Travel.API.Services;

namespace Travel.API.Controllers
{
    [ApiController]
    [Route("api/shoppingCart")]

    public class ShoppingCartController : ControllerBase
    {
        private readonly IHttpContextAccessor _contextAccessor;// 从上下文获取当前用户
        private readonly IToursitRouteRepository _toursitRouteRepository;
        private readonly IMapper _mapper;


        public ShoppingCartController(IHttpContextAccessor contextAccessor,
            IToursitRouteRepository toursitRouteRepository,
            IMapper mapper)
        {
            _contextAccessor = contextAccessor;
            _toursitRouteRepository = toursitRouteRepository;
            _mapper = mapper;
        }

        [HttpGet(Name = "GetShoppingCart")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetShoppingCart()
        {
            // 1 获得当前用户
            var userId = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            // 2 使用userid获得购物车
            var shoppingCart = await _toursitRouteRepository.GetShoppingCartByUserIdAsync(userId);

            return Ok(_mapper.Map<ShoppingCartDto>(shoppingCart));
        }

        [HttpPost("items")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> AddShoppingCartItem(
            [FromBody] AddShoppingCartItemDto addShoppingCartItemDto)
        {
            // 1 获得当前用户
            var userId = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            // 2 使用userid获得购物车
            var shoppingCart = await _toursitRouteRepository.GetShoppingCartByUserIdAsync(userId);

            // 3 创建lineItem
            var touristRoute = await _toursitRouteRepository.GetTouristRouteAsync(addShoppingCartItemDto.TouristRouteId);
            if (touristRoute == null)
            {
                return NotFound("旅游路线不存在");
            }

            var lineItem = new LineItem()
            {
                TouristRouteId = addShoppingCartItemDto.TouristRouteId,
                ShoppingCartId = shoppingCart.Id,
                OriginalPrice = touristRoute.OriginalPrice,
                DiscountPresent = touristRoute.DiscountPresent
            };

            // 4 添加lineitem，并保存数据库
            await _toursitRouteRepository.AddShoppingCartItem(lineItem);
            await _toursitRouteRepository.SaveAsync();

            return Ok(_mapper.Map<ShoppingCartDto>(shoppingCart));
        }

        [HttpDelete("items/{itemId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> DeleteShoppingCartItem([FromRoute] int itemId)
        {
            // 1 获取lineitem数据
            var lineItem = await _toursitRouteRepository.GetShoppingCartItemByItemId(itemId);
            if (lineItem == null)
            {
                return NotFound("购物车商品找不到");
            }

            _toursitRouteRepository.DeleteShoppingCartItem(lineItem);
            await _toursitRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpDelete("items/({itemIDs})")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> RemoveShoppingCartItems(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))] [FromRoute] IEnumerable<int> itemIDs)
        {
            var lineItems = await _toursitRouteRepository.GetShoppingCartsByIdListAsync(itemIDs);
            _toursitRouteRepository.DeleteShoppingCartItems(lineItems);
            await _toursitRouteRepository.SaveAsync();

            return NoContent();
        }

        /// <summary>
        /// 购物车结算下单
        /// </summary>
        /// <returns></returns>
        [HttpPost("checkout")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Checkout()
        {
            // 1 获得当前用户
            var userId = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            // 2 使用userid获得购物车
            var shoppingCart = await _toursitRouteRepository.GetShoppingCartByUserIdAsync(userId);

            // 3 创建订单
            var order = new Order()
            { 
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderState = OrderStateEnum.Pending,
                OrderItems = shoppingCart.ShoppingCartItems,
                CreateDateUTC = DateTime.UtcNow
            };
            shoppingCart.ShoppingCartItems = null;// 清空购物车，EF Core会自动跟踪这个对象

            // 4 保存数据
            await _toursitRouteRepository.AddOrderAsync(order);
            await _toursitRouteRepository.SaveAsync();

            // 5 返回响应
            return Ok(_mapper.Map<OrderDto>(order));
        }

    }
}
