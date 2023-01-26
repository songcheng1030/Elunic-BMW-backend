using System.Collections.Generic;
using System.Threading.Tasks;
using AIQXCommon.Auth;
using AIQXCommon.Models;
using AIQXCoreService.Domain.Models;
using AIQXCoreService.Implementation.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AIQXCoreService.Implementation.Controllers
{
    [ApiController]
    [Route("v1/plants")]
    public class PlantController : ControllerBase
    {
        private readonly ILogger<PlantController> _logger;
        private readonly PlantService _plantService;
        private readonly IMapper _mapper;

        public PlantController(ILogger<PlantController> logger, PlantService plantService, IMapper mapper)
        {
            _logger = logger;
            _plantService = plantService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<DataResponseSchema<IList<PlantDto>, DataResponseMeta>>> Get()
        {
            IList<PlantEntity> plants = await _plantService.GetAsync();
            return Ok(_mapper.Map<IList<PlantDto>>(plants));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DataResponseSchema<PlantDto, DataResponseMeta>>> Get(string id)
        {
            PlantEntity plant = await _plantService.GetByIdAsync(id);
            return Ok(_mapper.Map<PlantDto>(plant));
        }

        [HttpPost]
        [RequireRole(UseCaseAppRole.AIQX_TEAM)]
        public async Task<ActionResult<DataResponseSchema<PlantDto, DataResponseMeta>>> Post([FromBody] CreatePlantDto plant)
        {
            _plantService.ValidatePlantID(plant.Id);
            PlantEntity newPlant = await _plantService.CreateAsync(_mapper.Map<PlantEntity>(plant));
            return StatusCode(201, _mapper.Map<PlantDto>(newPlant));
        }

        [HttpPut("{id}")]
        [RequireRole(UseCaseAppRole.AIQX_TEAM)]
        public async Task<ActionResult<DataResponseSchema<PlantDto, DataResponseMeta>>> Put(string id, [FromBody] UpdatePlantDto dto)
        {
            PlantEntity plant = await _plantService.UpdateAsync(id, dto);
            return Ok(_mapper.Map<PlantDto>(plant));
        }

        [HttpDelete("{id}")]
        [RequireRole(UseCaseAppRole.AIQX_TEAM)]
        public async Task<ActionResult> Delete(string id)
        {
            await _plantService.DeleteAsync(id);
            return NoContent();
        }
    }
}
