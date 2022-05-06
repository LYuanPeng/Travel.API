using System.ComponentModel.DataAnnotations;
using Travel.API.Dtos;

namespace Travel.API.MyValidationAttributes
{
    public class TouristRouteTitleMustBeDifferentFromDescriptionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var touristRouteDto = (TouristRouteForManipulationDto)validationContext.ObjectInstance;
            if (touristRouteDto.Title == touristRouteDto.Description)
            {
                return new ValidationResult(
                    "路线名称必须与路线描述不同",
                    new[] { nameof(TouristRouteForCreationDto) }
                );
            }
            return ValidationResult.Success;
        }
    }
}
