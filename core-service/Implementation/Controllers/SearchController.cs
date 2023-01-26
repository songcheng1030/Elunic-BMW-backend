using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIQXCommon.Auth;
using AIQXCommon.Middlewares;
using AIQXCommon.Models;
using AIQXCoreService.Domain.Models;
using AIQXCoreService.Implementation.Services;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Opw.HttpExceptions;

namespace AIQXCoreService.Implementation.Controllers
{
    [ApiController]
    [Route("v1/search")]
    public class SearchController : ControllerBase
    {
        private readonly ILogger<SearchController> _logger;
        private readonly UseCaseIndexingService _indexingService;
        private readonly IMapper _mapper;

        public SearchController(ILogger<SearchController> logger, UseCaseIndexingService indexingService, IMapper mapper)
        {
            _logger = logger;
            _indexingService = indexingService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<DataResponseSchema<IList<SearchResultDto>, DataResponseMeta>>> Get([FromQuery] UseCaseContentQueryOptions query)
        {
            IList<SearchResult> steps = await _indexingService.SearchAsync(query.q);
            return Ok(_mapper.Map<IList<SearchResultDto>>(steps));
        }
    }
}
