using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Travel.API.Controllers
{
    [ApiController]
    [Route("api/FakeVanderPaymentProcess")]
    public class FakeVanderPaymentProcess : ControllerBase
    {
        /// <summary>
        /// 第三方模拟支付
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <param name="returnFault"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> ProcessPayment(
            [FromQuery] Guid orderNumber,
            [FromQuery] bool returnFault = false)
        {
            // 假装在处理
            await Task.Delay(3000);

            if (returnFault)
            {
                return Ok(new
                {
                    id = Guid.NewGuid(),
                    Created = DateTime.UtcNow,
                    approved = false,
                    message = "Reject",
                    payment_method = "信用卡支付",
                    order_number = orderNumber,
                    card = new
                    {
                        card_type = "信用卡",
                        last_four = "1234"
                    }
                });
            }

            return Ok(new 
            { 
                id = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                approved = true,
                message = "Approve",
                payment_method = "信用卡支付",
                order_number = orderNumber,
                card = new
                {
                    card_type = "信用卡",
                    last_four = "1234"
                }
            });
        }
    }
}
