using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Travel.API.Models;

namespace Travel.API.Dtos
{
    public class TouristRouteForUpdateDto : TouristRouteForManipulationDto
    {

        [Required(ErrorMessage = "更新必备")]
        [MaxLength(1500)]
        public override string Description { get; set; }
    }
}
