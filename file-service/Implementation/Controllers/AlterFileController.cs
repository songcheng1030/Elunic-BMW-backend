using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIQXCommon.Auth;
using AIQXCommon.Middlewares;
using AIQXCommon.Models;
using AIQXFileService.Domain.Models;
using AIQXFileService.Implementation.Services;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Opw.HttpExceptions;
using SixLabors.ImageSharp;

namespace AIQXFileService.Implementation.Controllers
{
    [ApiController]
    [Route("v1/files")]
    public class AlterFileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;
        private readonly ConfigService _configService;

        public AlterFileController(IFileService fileService, IMapper mapper, ConfigService configService)
        {
            _fileService = fileService;
            _mapper = mapper;
            _configService = configService;
        }

        // [MediaTokenValidatorGuard(GuardRule.AllowIfId)]
        [HttpGet("{id}/metadata")]
        public async Task<ActionResult<DataResponseSchema<FileMetadata, DataResponseMeta>>> GetFileInfo(Guid id)
        {
            FileEntity file = await _fileService.GetByIdAsync(id);
            var result = this.GetFileMetadata(file);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateFileDto dto)
        {
            await AssertCanEdit(HttpContext, id);

            FileEntity file = await _fileService.UpdateByIdAsync(id, dto);
            return Ok(_mapper.Map<FileDto>(file));
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await AssertCanEdit(HttpContext, id);

            await _fileService.DeleteByIdAsync(id);
            return NoContent();
        }

        private string GetExternalUrl()
        {
            if (!string.IsNullOrEmpty(_configService.ServiceUrl()))
            {
                return _configService.ServiceUrl();
            }

            // Hard wired for dev
            if (_configService.isDev())
            {
                return $"http://localhost:{_configService.HttpPort()}";
            }

            var host = this.Request.Headers.FirstOrDefault(_ => _.Key == "host").Value.ToString();
            var xHost = this.Request.Headers.FirstOrDefault(_ => _.Key == "x-host").Value.ToString();

            if (!string.IsNullOrEmpty(host))
            {
                return $"http://{host}";
            }

            if (!string.IsNullOrEmpty(xHost))
            {
                return $"http://{xHost}";
            }

            return _configService.ServiceUrl();
        }

        private object ReadExifData(FileEntity file)
        {
            if (!file.ContentType.StartsWith("image") ||
                !file.ContentType.StartsWith("jpg") ||
                !file.ContentType.StartsWith("jpeg")
            )
            {
                throw new Exception("Unsupported media format to get exif for!");
            }

            var stream = _fileService.GetFileStream(file);
            if (stream.Length <= 0)
            {
                throw new NotFoundException("No such file!");
            }

            IEnumerable exifData = null;
            try
            {
                using (var loadedImage = Image.Load(stream))
                {
                    var exifProfile = loadedImage.Metadata?.ExifProfile;
                    exifData = exifProfile.Values.Select(x => KeyValuePair.Create(x.Tag.ToString(), x.GetValue()));
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error when reading EXIF data: {0}", e.Message);
            }

            return exifData;
        }

        private object GetFileMetadata(FileEntity file)
        {
            object exifInfo = null;
            try
            {
                exifInfo = this.ReadExifData(file);
            }
            catch { }

            var mediaToken = Request.Query["mediaToken"];
            var shouldApplyQuery = Request.Query["mediaToken.shouldApply"].ToString();
            bool.TryParse(shouldApplyQuery, out var shouldApply);
            var extendedInfo = new
            {
                url = "",
                urlTokenized = "",
                urlThumbnailTokenized = "",
                iat = "",
            };
            if (string.IsNullOrEmpty(mediaToken) || !shouldApply)
            {
                var baseUrl = GetExternalUrl();
                var extUrl = $"{baseUrl}/v1/file/{file.Id}";

                var extUrlToken = $"{baseUrl}/v1/file/{file.Id}";
                extUrlToken += "?disposition=inline";
                extUrlToken += $"&token={_fileService.GetMediaToken(file.Id)}";

                var extUrlImageToken = $"{baseUrl}/v1/thumbnail/{file.Id}";
                extUrlImageToken += "?disposition=inline";
                extUrlImageToken += $"&token={_fileService.GetMediaToken(file.Id)}";

                extendedInfo = new
                {
                    url = extUrl,
                    urlTokenized = extUrlToken,
                    urlThumbnailTokenized = extUrlImageToken,
                    iat = DateTime.UtcNow.ToString("s"),
                };
            }

            return new FileMetadata
            {
                Id = file.Id,
                Name = file.Name,
                RefIds = file.RefIds.Select(r => r.Value).ToList(),
                Tags = file.Tags.Select(t => t.Value).ToList(),
                Size = file.Size,
                Mime = file.ContentType,
                CreatedAt = file.CreatedAt,
                exif = exifInfo,
                extended = extendedInfo
            };
        }

        private async Task AssertCanEdit(HttpContext context, Guid id)
        {
            if (context.IsInternalRequest())
            {
                return;
            }

            var authInfo = context.GetAuthorizationOrFail();
            if (!authInfo.ContainsRole(UseCaseAppRole.AIQX_TEAM))
            {
                FileEntity current = await _fileService.GetByIdAsync(id);

                context.AssertUserIdOrFail(current.CreatedBy);

                // Locked files cannot be changed anymore.
                if (current.Tags.Any(t => t.Value == FileTag.LOCKED))
                {
                    throw new ForbiddenException("File locked");
                }
            }
        }
    }
}