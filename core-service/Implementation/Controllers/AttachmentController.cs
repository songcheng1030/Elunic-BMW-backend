using System;
using System.Collections.Generic;
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
    [Route("v1/attachments")]
    public class AttachmentController : ControllerBase
    {
        private readonly ILogger<AttachmentController> _logger;
        private readonly AttachmentService _attachmentService;
        private readonly IMapper _mapper;

        public AttachmentController(ILogger<AttachmentController> logger, AttachmentService attachmentService, IMapper mapper)
        {
            _logger = logger;
            _attachmentService = attachmentService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<DataResponseSchema<IList<AttachmentDto>, DataResponseMeta>>> Get()
        {
            IList<AttachmentEntity> attachments = await _attachmentService.GetAsync();
            return Ok(_mapper.Map<IList<AttachmentDto>>(attachments));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DataResponseSchema<AttachmentDto, DataResponseMeta>>> Get(Guid id)
        {
            AttachmentEntity attachment = await _attachmentService.GetByIdAsync(id);
            return Ok(_mapper.Map<AttachmentDto>(attachment));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await AssertCanEdit(HttpContext, id);

            await _attachmentService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<DataResponseSchema<AttachmentDto, DataResponseMeta>>> Post([FromBody] CreateAttachmentDto dto)
        {
            AttachmentType? type = AttachmentEntity.TypeFromString(dto.Type);
            if (type == null)
            {
                throw new BadRequestException($"Unknown attachment type {dto.Type}");
            }
            await AssertCanCreate(HttpContext, dto.UseCaseId, (AttachmentType)type);

            AttachmentEntity attachment = await _attachmentService.CreateAsync(dto.UseCaseId, _mapper.Map<AttachmentEntity>(dto));
            return StatusCode(201, _mapper.Map<AttachmentDto>(attachment));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DataResponseSchema<AttachmentDto, DataResponseMeta>>> Put(Guid id, [FromBody] UpdateAttachmentDto dto)
        {
            await AssertCanEdit(HttpContext, id);

            AttachmentEntity attachment = await _attachmentService.UpdateAsync(id, dto);
            return Ok(_mapper.Map<AttachmentDto>(attachment));
        }

        private async Task AssertCanCreate(HttpContext context, Guid id, AttachmentType type)
        {
            if (context.IsInternalRequest())
            {
                return;
            }

            var authInfo = context.GetAuthorizationOrFail();
            if (!authInfo.ContainsRole(UseCaseAppRole.AIQX_TEAM))
            {
                AttachmentEntity current = await _attachmentService.GetByIdAsync(id);

                context.AssertUserIdOrFail(current.UseCase.CreatedBy);

                // Does comes from the database and therefore we can assume its save to cast.
                UseCaseStep allowedStep = AttachmentEntity.StepsDictionary[type];
                UseCaseStep currentStep = current.UseCase.CurrentStep;
                if (allowedStep != currentStep)
                {
                    throw new ForbiddenException($"Cannot create attachment in current use case step {currentStep}");
                }
            }
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
                AttachmentEntity current = await _attachmentService.GetByIdAsync(id);
                AttachmentType? type = AttachmentEntity.TypeFromString(current.Type);
                // Does comes from the database and therefore we can assume its save to cast.
                UseCaseStep allowedStep = AttachmentEntity.StepsDictionary[(AttachmentType)type];
                UseCaseStep currentStep = current.UseCase.CurrentStep;
                if (allowedStep != currentStep)
                {
                    throw new ForbiddenException("Attachment locked");
                }
            }
        }
    }
}
