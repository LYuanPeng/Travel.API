using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Travel.API.Dtos;
using Travel.API.ResourceParameters;
using Travel.API.Services;

namespace Travel.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IHttpContextAccessor _contextAccessor;// 从上下文获取当前用户
        private readonly IToursitRouteRepository _toursitRouteRepository;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;


        public OrdersController(IHttpContextAccessor contextAccessor,
            IToursitRouteRepository toursitRouteRepository,
            IMapper mapper,
            IHttpClientFactory httpClientFactory)
        {
            _contextAccessor = contextAccessor;
            _toursitRouteRepository = toursitRouteRepository;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet(Name = "GetOrders")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetOrders(
            [FromQuery] PaginationResourceParamaters parameters)
        {
            // 1 获得当前用户
            var userId = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            // 2 使用用户id来获取订单历史记录
            var orders = await _toursitRouteRepository.GetOrderByUserIdAsync(
                userId, parameters.PageSize, parameters.PageNumber);

            // 3返回响应
            return Ok(_mapper.Map<IEnumerable<OrderDto>>(orders));
        }

        [HttpGet("{orderId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetOrderById([FromRoute] Guid orderId)
        {
            // 1 获得当前用户
            var userId = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            // 2 使用用户id和订单id获取订单详情
            var order = await _toursitRouteRepository.GetOrderById(orderId);

            // 3返回响应
            return Ok(_mapper.Map<OrderDto>(order));
        }

        [HttpPost("{orderId}/placeOrder")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> PlaceOrder([FromRoute] Guid orderId)
        {
            // 1 获得当前用户
            var userId = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            // 2 开始处理支付
            var order = await _toursitRouteRepository.GetOrderById(orderId);
            order.PaymentProcessing();
            await _toursitRouteRepository.SaveAsync();

            // 3向第三方提交支付请求，等待第三方响应
            var httpClient = _httpClientFactory.CreateClient();
            string url = @"https://localhost:5001/api/FakeVanderPaymentProcess?orderNumber={0}&returnFault={1}";
            var response = await httpClient.PostAsync(
                string.Format(url, order.Id, false),
                null
            );

            // 4 提取支付结果，以及支付信息
            bool isApproved = false;
            string transcactionMetadata = "";
            if (response.IsSuccessStatusCode)
            {
                transcactionMetadata = await response.Content.ReadAsStringAsync();
                var jsonObject = (JObject)JsonConvert.DeserializeObject(transcactionMetadata);
                isApproved = jsonObject["approved"].Value<bool>();
            }

            // 5 如果第三方支付成功，完成订单
            if (isApproved)
            {
                order.PaymentApprove();
            }
            else
            {
                order.PaymentReject();
            }
            order.TransactionMetadata = transcactionMetadata;
            await _toursitRouteRepository.SaveAsync();

            return Ok(_mapper.Map<OrderDto>(order));
        }
    }
}
