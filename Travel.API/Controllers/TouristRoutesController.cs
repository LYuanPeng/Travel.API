using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Travel.API.Dtos;
using Travel.API.Services;
using AutoMapper;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Travel.API.ResourceParameters;
using Travel.API.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Travel.API.Helper;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Net.Http.Headers;
using System.Dynamic;

namespace Travel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TouristRoutesController : ControllerBase
    {
        private readonly IToursitRouteRepository _toursitRouteRepository;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;
        private readonly IPropertyMappingService _propertyMappingService;

        public TouristRoutesController(
            IToursitRouteRepository toursitRouteRepository,
            IMapper mapper,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IPropertyMappingService propertyMappingService)
        {
            _toursitRouteRepository = toursitRouteRepository;
            _mapper = mapper;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _propertyMappingService = propertyMappingService;
        }

        /// <summary>
        /// 生成旅游路线资源url
        /// </summary>
        private string GenerateTouristRouteResourceURL(
            TouristRouteResourceParameters paramaters,
            PaginationResourceParamaters paramaters2,
            ResourceUriType type)
        {
            return type switch
            {
                ResourceUriType.PreviousPage => _urlHelper.Link("GetTouristRoutes",
                    new
                    {
                        fileds = paramaters.Fields,
                        orderBy = paramaters.OrderBy,
                        keyword = paramaters.Keyword,
                        rating = paramaters.Rating,
                        pageNumber = paramaters2.PageNumber - 1,
                        pageSize = paramaters2.PageSize
                    }),
                ResourceUriType.NextPage => _urlHelper.Link("GetTouristRoutes",
                    new
                    {
                        fileds = paramaters.Fields,
                        orderBy = paramaters.OrderBy,
                        keyword = paramaters.Keyword,
                        rating = paramaters.Rating,
                        pageNumber = paramaters2.PageNumber + 1,
                        pageSize = paramaters2.PageSize
                    }),
                _ => _urlHelper.Link("GetTouristRoutes",
                    new
                    {
                        fileds = paramaters.Fields,
                        orderBy = paramaters.OrderBy,
                        keyword = paramaters.Keyword,
                        rating = paramaters.Rating,
                        pageNumber = paramaters2.PageNumber,
                        pageSize = paramaters2.PageSize
                    })
            };
        }

        private IEnumerable<LinkDto> CreateLinksForTouristRouteList(
            TouristRouteResourceParameters paramaters,
            PaginationResourceParamaters paramaters2)
        {
            var links = new List<LinkDto>();
            // 添加self自我链接
            links.Add(new LinkDto(
                GenerateTouristRouteResourceURL(paramaters, paramaters2, ResourceUriType.CurrentPage),
                "self",
                "GET"
            ));

            // "api/touristRoutes"
            // 添加创建旅游路线
            links.Add(new LinkDto(
                Url.Link("CreateTouristRoute", null),
                "create_tourist_route)",
                "POST"
            ));

            return links;
        }

        // api/touristRoutes?keyword=传入的参数
        // 1 application/json -> 旅游路线资源
        // 2 application/vnd.f4n.hateoas+json
        // 3 application/vnd.f4n.touristRoutes.simplify+json -> 输出简化版资源数据
        // 4 application/vnd.f4n.touristRoutes.simplify.hateoas+json-> 输出简化版hateoas超媒体资源
        [Produces("application/json",
            "application/vnd.f4n.hateoas+json",
            "application/vnd.f4n.touristRoutes.simplify+json",
            "application/vnd.f4n.touristRoutes.simplify.hateoas+json")]
        [HttpGet(Name = "GetTouristRoutes")]
        [HttpHead]
        public async Task<IActionResult> GetTouristRoutes(
            [FromQuery] TouristRouteResourceParameters paramaters,
            [FromQuery] PaginationResourceParamaters paramaters2,
            [FromHeader(Name = "Accept")] string mediaType 
        //[FromQuery] string keyword,
        //string rating //小于lessThanX，大于largerThanX，等于equalToX
        )
        {
            if (!MediaTypeHeaderValue
                .TryParse(mediaType, out MediaTypeHeaderValue parseMediaType))
            {
                return BadRequest();
            }

            if (!_propertyMappingService.IsMappingExists<TouristRouteDto, TouristRoute>(paramaters.OrderBy))
            {
                return BadRequest("请输入正确的排序参数");
            }

            if (!_propertyMappingService.IsPropertiesExists<TouristRouteDto>(paramaters.Fields))
            {
                return BadRequest("请输入正确的塑性参数");
            }

            var touristRoutesFromRepo = await _toursitRouteRepository.GetTouristRoutesAsync(
                paramaters.Keyword,
                paramaters.RatingOperator,
                paramaters.RatingValue,
                paramaters2.PageSize,
                paramaters2.PageNumber,
                paramaters.OrderBy
            );
            if (touristRoutesFromRepo == null || touristRoutesFromRepo.Count() <= 0)
            {
                return NotFound("没有旅游路线");
            }

            var previousPageLink = touristRoutesFromRepo.HasPrevious
                ? GenerateTouristRouteResourceURL(paramaters, paramaters2, ResourceUriType.PreviousPage)
                : null;

            var nextPageLink = touristRoutesFromRepo.HasNext
                ? GenerateTouristRouteResourceURL(paramaters, paramaters2, ResourceUriType.NextPage)
                : null;

            // x-pagination
            var paginationMetadata = new
            {
                previousPageLink,
                nextPageLink,
                totalCount = touristRoutesFromRepo.Count,
                pageSize = touristRoutesFromRepo.PageSize,
                currentPage = touristRoutesFromRepo.CurrentPage,
                totalPages = touristRoutesFromRepo.TotalPages
            };
            Response.Headers.Add("x-pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            bool isHateoas = parseMediaType.SubTypeWithoutSuffix
                .EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);

            var primaryMediaType = isHateoas
                ? parseMediaType.SubTypeWithoutSuffix
                    .Substring(0, parseMediaType.SubTypeWithoutSuffix.Length - 8)
                : parseMediaType.SubTypeWithoutSuffix;

            //var touristRoutesDto = _mapper.Map<IEnumerable<TouristRouteDto>>(touristRoutesFromRepo);
            //var shapedDtoList = touristRoutesDto.ShapeData(paramaters.Fields);

            IEnumerable<object> touristRoutesDto;
            IEnumerable<ExpandoObject> shapedDtoList;

            if (primaryMediaType == "vnd.f4n.touristRoutes.simplify")
            {
                touristRoutesDto = _mapper.Map<IEnumerable<TouristRouteSimplifyDto>>(touristRoutesFromRepo);
                shapedDtoList = ((IEnumerable<TouristRouteSimplifyDto>)touristRoutesDto).ShapeData(paramaters.Fields);
            }
            else
            {
                touristRoutesDto = _mapper.Map<IEnumerable<TouristRouteDto>>(touristRoutesFromRepo);
                shapedDtoList = touristRoutesDto.ShapeData(paramaters.Fields);
            }


            if (isHateoas)
            {
                var linkDto = CreateLinksForTouristRouteList(paramaters, paramaters2);
                var shapedDtoWithLinklist = shapedDtoList.Select(t =>
                {
                    var touristRouteDictionary = t as IDictionary<string, object>;
                    var links = CreateLinkForTouristRoute((Guid)touristRouteDictionary["Id"], null);
                    touristRouteDictionary.Add("links", links);
                    return touristRouteDictionary;
                });

                var result = new
                {
                    value = shapedDtoWithLinklist,
                    links = linkDto
                };
                return Ok(result);
            }

            return Ok(shapedDtoList);
        }

        private IEnumerable<LinkDto> CreateLinkForTouristRoute(
            Guid touristRouteId,
            string fields)
        {
            var links = new List<LinkDto>();

            // 查询
            links.Add(
                new LinkDto(
                    Url.Link("GetTouristRouteById", new { touristRouteId, fields }),
                     "self",
                     "GET"
                    )
                );
            // 更新
            links.Add(
                new LinkDto(
                    Url.Link("UpdateTouristRoute", new { touristRouteId, fields }),
                     "update",
                     "PUT"
                    )
                );
            // 局部更新
            links.Add(
                new LinkDto(
                    Url.Link("PartiallyUpdateTouristRoute", new { touristRouteId, fields }),
                     "partially_update",
                     "PATCH"
                    )
                );
            // 删除
            links.Add(
                new LinkDto(
                    Url.Link("DeleteTouristRoute", new { touristRouteId, fields }),
                     "delete",
                     "DELETE"
                    )
                );

            // 获取路线图片
            links.Add(
                new LinkDto(
                    Url.Link("GetPictureListForTouristRoute", new { touristRouteId, fields }),
                     "get_pictures",
                     "GET"
                    )
                );
            // 添加新图片
            links.Add(
                new LinkDto(
                    Url.Link("CreateTouristRoutePicture", new { touristRouteId, fields }),
                     "create_pictures",
                     "POST"
                    )
                );

            return links;
        }

        [HttpGet("{touristRouteId:Guid}", Name = nameof(GetTouristRouteById))] //api/touristroutes/{touristRouteId}
        [HttpHead]
        public async Task<IActionResult> GetTouristRouteById(
            Guid touristRouteId,
            string fields)
        {
            var touristRouteFromRepo = await _toursitRouteRepository.GetTouristRouteAsync(touristRouteId);
            if (touristRouteFromRepo == null)
            {
                return NotFound($"旅游路线{touristRouteId}找不到");
            }
            //var touristRouteDto = new TouristRouteDto()
            //{
            //    Id = touristRouteFromRepo.Id,
            //    Title = touristRouteFromRepo.Title,
            //    Description = touristRouteFromRepo.Description,
            //    Price = touristRouteFromRepo.OriginalPrice * (decimal)(touristRouteFromRepo.DiscountPresent ?? 1),
            //    CreateTime = touristRouteFromRepo.CreateTime,
            //    UpdateTime = touristRouteFromRepo.UpdateTime,
            //    Features = touristRouteFromRepo.Features,
            //    Fees = touristRouteFromRepo.Fees,
            //    Notes = touristRouteFromRepo.Notes,
            //    Rating = touristRouteFromRepo.Rating,
            //    TravelDays = touristRouteFromRepo.TravelDays.ToString(),
            //    TripType = touristRouteFromRepo.TripType.ToString(),
            //    DepartureCity = touristRouteFromRepo.DepartureCity.ToString()
            //};
            var touristRouteDto = _mapper.Map<TouristRouteDto>(touristRouteFromRepo);

            //return Ok(touristRouteDto.ShapeData(fields));
            var linkDtos = CreateLinkForTouristRoute(touristRouteId, fields);
            var result = touristRouteDto.ShapeData(fields)
                as IDictionary<string, object>;
            result.Add("links", linkDtos);
            return Ok(result);
        }

        [HttpPost(Name = "CreateTouristRoute")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> CreateTouristRoute([FromBody] TouristRouteForCreationDto touristRouteForCreationDto)
        {
            var touristRouteModel = _mapper.Map<TouristRoute>(touristRouteForCreationDto);
            _toursitRouteRepository.AddTouristRoute(touristRouteModel);
            await _toursitRouteRepository.SaveAsync();

            var touristRouteToReturn = _mapper.Map<TouristRouteDto>(touristRouteModel);

            var links = CreateLinkForTouristRoute(touristRouteModel.Id, null);
            var result = touristRouteToReturn.ShapeData(null)
                as IDictionary<string, object>;
            result.Add("links", links);

            return CreatedAtRoute(
                "GetTouristRouteById", //这里返回不对，不知道为什么
                new {
                    touristRouteId = result["Id"],
                    result
                });
        }

        [HttpPut("{touristRouteId}", Name = "UpdateTouristRoute")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> UpdateTouristRoute(
            [FromRoute] Guid touristRouteId,
            [FromBody] TouristRouteForUpdateDto touristRouteForUpdateDto)
        {
            if (! await _toursitRouteRepository.TouristRouteExistsAsync(touristRouteId))
            {
                return NotFound("旅游路线找不到");
            }

            var touristFromRepo = await _toursitRouteRepository.GetTouristRouteAsync(touristRouteId);
            //1.映射dto
            //2.更新dto
            //3.映射model
            //以上三步使用automapper可以一行写完，但是记得写profile
            _mapper.Map(touristRouteForUpdateDto, touristFromRepo);

            //efcore会跟踪touristFromRepo这个实体，有修改了，数据模型的追踪会更改
            await _toursitRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{touristRouteId}", Name = "PartiallyUpdateTouristRoute")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> PartiallyUpdateTouristRoute(
            [FromRoute] Guid touristRouteId,
            [FromBody] JsonPatchDocument<TouristRouteForUpdateDto> patchDocument)
        {
            if (! await _toursitRouteRepository.TouristRouteExistsAsync(touristRouteId))
            {
                return NotFound("旅游路线找不到");
            }

            var touristFromRepo =  await _toursitRouteRepository.GetTouristRouteAsync(touristRouteId);
            var touristRouteToPatch = _mapper.Map<TouristRouteForUpdateDto>(touristFromRepo);
            patchDocument.ApplyTo(touristRouteToPatch, ModelState);

            if(!TryValidateModel(touristRouteToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(touristRouteToPatch, touristFromRepo);
            await _toursitRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpDelete("{touristRouteId}", Name = "DeleteTouristRoute")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> DeleteTouristRoute([FromRoute] Guid touristRouteId)
        {
            if (! await _toursitRouteRepository.TouristRouteExistsAsync(touristRouteId))
            {
                return NotFound("旅游路线找不到");
            }

            var touristRoute = await _toursitRouteRepository.GetTouristRouteAsync(touristRouteId);
            _toursitRouteRepository.DeleteTouristRoute(touristRoute);
            await _toursitRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpDelete("({touristIDs})")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> DeleleByIDs(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))] [FromRoute] IEnumerable<Guid> touristIDs)
        {
            if (touristIDs == null)
            {
                return BadRequest();
            }

            var touristRouteFromRepo = await _toursitRouteRepository.GetTouristRoutesByIDListAsync(touristIDs);
            _toursitRouteRepository.DeleteTouristRoutes(touristRouteFromRepo);
            await _toursitRouteRepository.SaveAsync();

            return NoContent();
        }
    }
}
