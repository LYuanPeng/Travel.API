using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Travel.API.Models;
using Travel.API.MyValidationAttributes;

namespace Travel.API.Dtos
{
    [TouristRouteTitleMustBeDifferentFromDescription]
    public abstract class TouristRouteForManipulationDto
    {
        [Required(ErrorMessage = "title 不可为空")]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1500)]
        public virtual string Description { get; set; }

        //计算方式：原价×折扣
        public decimal Price { get; set; }

        //public decimal OriginalPrice { get; set; }

        //public double? DiscountPresent { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 发团时间
        /// </summary>
        public DateTime? DepratureTime { get; set; }

        /// <summary>
        /// 卖点介绍
        /// </summary>
        public string Features { get; set; }

        /// <summary>
        /// 费用说明
        /// </summary>
        public string Fees { get; set; }

        public string Notes { get; set; }


        public double? Rating { get; set; }

        public string TravelDays { get; set; }

        public string TripType { get; set; }

        public string DepartureCity { get; set; }

        public ICollection<TouristRoutePicture> TouristRoutePictures { get; set; }
            = new List<TouristRoutePicture>();
    }
}
