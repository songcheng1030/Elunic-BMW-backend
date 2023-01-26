using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIQXCommon.Models;
using AIQXCoreService.Implementation.Persistence;
using AIQXCoreService.Implementation.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Opw.HttpExceptions;

namespace AIQXCoreService.Implementation.Controllers
{
    [ApiController]
    [Route("v1/kpis")]
    public class KpiController : ControllerBase
    {
        private readonly ILogger<KpiController> _logger;
        private readonly IMapper _mapper;
        private readonly KpiCache _kpiCache = KpiCache.Instance;
        private readonly KpiService _kpiService;
        private readonly PlantService _plantService;

        public KpiController(ILogger<KpiController> logger, IMapper mapper, KpiService kpiService, PlantService plantService, UseCaseService useCaseService)
        {
            _logger = logger;
            _mapper = mapper;
            _kpiService = kpiService;
            _plantService = plantService;
        }

        [HttpGet("plants/{plantId}")]
        public async Task<ActionResult<DataResponseSchema<Dictionary<string, int>, DataResponseMeta>>> GetPlantsKpi(string plantId)
        {
            await _plantService.GetByIdAsync(plantId);
            var kpi = _kpiCache.Plants.FirstOrDefault(p => p.Key == plantId);
            if (default(KeyValuePair<string, Dictionary<string, int>>).Equals(kpi))
            {
                throw new NotFoundException("Kpis not found");
            }
            return Ok(kpi.Value);
        }

        [HttpGet("use-cases/{useCaseId}")]
        public async Task<ActionResult<DataResponseSchema<Dictionary<string, int>, DataResponseMeta>>> GetUseCasesKpi(Guid useCaseId)
        {
            var res = await _kpiService.CalculateForUseCaseAsync(useCaseId);
            return Ok(res);
        }
    }
}
