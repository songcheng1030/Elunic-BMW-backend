using System;
using System.Threading.Tasks;
using AIQXCommon.Middlewares;
using AIQXCommon.Models;
using AIQXFileService.Domain.Models;
using AIQXFileService.Implementation.Services;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Opw.HttpExceptions;

namespace AIQXFileService.Implementation.Controllers
{
    [ApiController]
    [Route("v1")]
    public class UploadFileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ImageConverter _imageConverter;
        private readonly IMapper _mapper;

        public UploadFileController(IFileService fileService, ImageConverter imageConverter, IMapper mapper)
        {
            _fileService = fileService;
            _imageConverter = imageConverter;
            _mapper = mapper;
        }

        // [MediaTokenValidatorGuard]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] CreateFileDto request)
        {
            var authInfo = HttpContext.GetAuthorizationOrFail();
            var result = await _fileService.CreateFileFromUploadAsync(request, authInfo.Id);

            if (result.ContentType.StartsWith("image/"))
            {
                try
                {
                    var stream = await _fileService.GetImageStreamByIdAsync(result.Id);
                    _imageConverter.GenerateThumbnailIfRequested(stream, result.ContentType, null, null, null, null);
                }
                catch
                {
                    await _fileService.DeleteByIdAsync(result.Id);
                    throw new BadRequestException($"Format of image \"{request.File.FileName}\" is not supported");
                }
            }

            return StatusCode(201, _mapper.Map<FileDto>(result));
        }
    }
}