using System;
using System.Collections.Generic;


namespace Travel.API.Dtos
{
    public class OrderDto
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public ICollection<LineItemDto> OrderItems { get; set; }

        public string OrderState { get; set; }

        public DateTime CreateDateUTC { get; set; }

        public string TransactionMetadata { get; set; }//第三方回调数据
    }
}
