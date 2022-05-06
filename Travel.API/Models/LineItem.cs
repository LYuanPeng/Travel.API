using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Travel.API.Models
{
    public class LineItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } 

        public Guid TouristRouteId { get; set; }

        [ForeignKey("TouristRouteId")]
        public TouristRoute TouristRoute { get; set; }

        public Guid? ShoppingCartId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal OriginalPrice { get; set; }

        [Range(0.0, 1.0)]
        public double? DiscountPresent { get; set; }


    }
}
