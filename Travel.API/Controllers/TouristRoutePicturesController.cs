using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Travel.API.Dtos;
using Travel.API.Models;
using Travel.API.Services;

namespace Travel.API.Controllers
{
    [Route("api/touristRoutes/{touristRouteId}/pictures")]
    [ApiController]
    public class TouristRoutePicturesController : ControllerBase
    {
        private readonly IToursitRouteRepository _toursitRouteRepository;
        private readonly IMapper _mapper;

        public TouristRoutePicturesController(
            IToursitRouteRepository toursitRouteRepository,
            IMapper mapper)
        {
            _toursitRouteRepository = toursitRouteRepository;
            _mapper = mapper;
        }

        [HttpGet(Name = "GetPictureListForTouristRoute")]
        public async Task<IActionResult> GetPictureListForTouristRoute(Guid touristRouteId)
        {

            if (! await _toursitRouteRepository.TouristRouteExistsAsync(touristRouteId))
            {
                return NotFound("旅游线路不存在");
            }

            var picturesFromRepo = await _toursitRouteRepository.GetPicturesByTouristRouteIdAsync(touristRouteId);
            if (picturesFromRepo == null || picturesFromRepo.Count() <= 0)
            {
                return NotFound("照片不存在");
            }

            return Ok(_mapper.Map<IEnumerable<TouristRoutePictureDto>>(picturesFromRepo));
        }

        [HttpGet("{pictureId}", Name = nameof(GetPicture))]
        public async Task<IActionResult> GetPicture(Guid touristRouteId, int pictureId)
        {
            if (! await _toursitRouteRepository.TouristRouteExistsAsync(touristRouteId))
            {
                return NotFound("旅游线路不存在");
            }

            var pictureFromRepo = await _toursitRouteRepository.GetPictureAsync(pictureId);
            if (pictureFromRepo == null)
            {
                return NotFound("图片不存在");
            }
            return Ok(_mapper.Map<TouristRoutePictureDto>(pictureFromRepo));
        }

        [HttpPost(Name = "CreateTouristRoutePicture")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> CreateTouristRoutePicture(
            [FromRoute] Guid touristRouteId,
            [FromBody] TouristRoutePictureForCreationDto touristRoutePictureForCreationDto)
        {
            if (! await _toursitRouteRepository.TouristRouteExistsAsync(touristRouteId))
            {
                return NotFound("旅游线路不存在");
            }

            var pictureModel = _mapper.Map<TouristRoutePicture>(touristRoutePictureForCreationDto);
            _toursitRouteRepository.AddTouristRoutePicture(touristRouteId, pictureModel);
            await _toursitRouteRepository.SaveAsync();

            var pictureToReturn = _mapper.Map<TouristRoutePictureDto>(pictureModel);

            return CreatedAtRoute(
                "GetPicture",
                new 
                { 
                    touristRouteId = pictureModel.TouristRouteId,
                    pictureId = pictureModel.Id
                },
                pictureToReturn
            );
        }

        [HttpDelete("{pictureId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> DeletePicture([FromRoute] Guid touristRouteId,
            [FromRoute] int pictureId)
        {
            if (! await _toursitRouteRepository.TouristRouteExistsAsync(touristRouteId))
            {
                return NotFound("旅游线路不存在");
            }

            var picture = await _toursitRouteRepository.GetPictureAsync(pictureId);
            _toursitRouteRepository.DeleteTouristRoutePicture(picture);
            await _toursitRouteRepository.SaveAsync();

            return NoContent();
        }
    }
}
