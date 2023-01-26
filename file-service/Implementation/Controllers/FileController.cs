using System;
using System.Threading.Tasks;
using AIQXFileService.Implementation.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace AIQXCoreService.Implementation.Controllers
{
    [ApiController]
    [Route("v1/files")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly IFileService _fileService;

        private readonly IMapper _mapper;

        public FileController(ILogger<FileController> logger, IFileService fileService, IMapper mapper)
        {
            _logger = logger;
            _fileService = fileService;
            _mapper = mapper;
        }

        [HttpGet("{id}/download")]
        [SwaggerResponse(200, type: typeof(byte[]))]
        public async Task<ActionResult> Download(Guid id)
        {
            var file = await _fileService.GetByIdAsync(id);
            var fileStream = _fileService.GetFileStream(file);

            return new FileStreamResult(fileStream, file.ContentType)
            {
                EnableRangeProcessing = true,
                FileDownloadName = file.Name,
                LastModified = file.UpdatedAt
            };
        }
    }
}
