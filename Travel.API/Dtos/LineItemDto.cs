using System;

namespace Travel.API.Dtos
{
    public class LineItemDto
    {
        public int Id { get; set; }

        public Guid TouristRouteId { get; set; }

        public TouristRouteDto TouristRoute { get; set; }

        public Guid? ShoppingCartId { get; set; }

        public decimal OriginalPrice { get; set; }

        public double? DiscountPresent { get; set; }
    }
}
