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
    [Route("v1/use-cases")]
    public class UseCaseController : ControllerBase
    {
        private readonly ILogger<UseCaseController> _logger;
        private readonly UseCaseService _useCaseService;
        private readonly PlantService _plantService;
        private readonly AttachmentService _attachmentService;
        private readonly IMapper _mapper;

        public UseCaseController(ILogger<UseCaseController> logger, UseCaseService useCaseService, PlantService plantService, AttachmentService attachmentService, IMapper mapper)
        {
            _logger = logger;
            _useCaseService = useCaseService;
            _plantService = plantService;
            _attachmentService = attachmentService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<DataResponseSchema<IList<UseCaseDto>, PagingResult>>> Get([FromQuery] UseCaseQueryOptions pagingOption)
        {
            var (useCaseEntities, pagingResult) = await _useCaseService.GetByPagingAsync(pagingOption);
            var useCases = _mapper.Map<IList<UseCaseDto>>(useCaseEntities);
            var result = new DataResponse(useCases, pagingResult);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DataResponseSchema<UseCaseDto, DataResponseMeta>>> Get(Guid id)
        {
            UseCaseEntity useCase = await _useCaseService.GetByIdAsync(id);
            return Ok(_mapper.Map<UseCaseDto>(useCase));
        }

        [HttpPost]
        public async Task<ActionResult<DataResponseSchema<UseCaseDto, DataResponseMeta>>> Post([FromBody] CreateUseCaseDto useCase)
        {
            var authInfo = HttpContext.GetAuthorizationOrFail();
            UseCaseEntity newUseCase = await _useCaseService.CreateAsync(_mapper.Map<UseCaseEntity>(useCase), useCase.PlantId, authInfo.Id);
            return StatusCode(201, _mapper.Map<UseCaseDto>(newUseCase));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DataResponseSchema<UseCaseDto, DataResponseMeta>>> Put(Guid id, [FromBody] UpdateUseCaseDto dto)
        {
            await AssertCanEdit(HttpContext, id);
            // TODO: After first step complete, user may only update certain props (e.g. building).

            UseCaseEntity useCase = await _useCaseService.UpdateAsync(id, dto);
            return Ok(_mapper.Map<UseCaseEntity, UseCaseDto>(useCase));
        }

        [HttpPut("{id}/step/{step}")]
        public async Task<ActionResult<DataResponseSchema<UseCaseDto, DataResponseMeta>>> PutFormStep(Guid id, string step, [FromBody] Object dto)
        {
            UseCaseStep? ste = UseCaseStepEntity.StepFromString(step);
            if (ste == null)
            {
                throw new BadRequestException($"Unknown step {step}");
            }
            await AssertCanEdit(HttpContext, id, (UseCaseStep)ste);

            var authInfo = HttpContext.GetAuthorizationOrFail();

            UseCaseEntity useCase = await _useCaseService.UpdateStepAsync(id, (UseCaseStep)ste, authInfo.Id, dto);
            return Ok(_mapper.Map<UseCaseDto>(useCase));
        }

        [HttpPost("{id}/step/{step}/complete")]
        public async Task<ActionResult<DataResponseSchema<UseCaseDto, DataResponseMeta>>> PostCompleteStep(Guid id, string step)
        {
            UseCaseStep? ste = UseCaseStepEntity.StepFromString(step);
            if (ste == null)
            {
                throw new BadRequestException($"Unknown step {step}");
            }
            await AssertCanEdit(HttpContext, id, (UseCaseStep)ste);

            var authInfo = HttpContext.GetAuthorizationOrFail();

            UseCaseEntity useCase = await _useCaseService.CompleteStepAsync(id, (UseCaseStep)ste, authInfo);
            return Ok(_mapper.Map<UseCaseDto>(useCase));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await AssertCanDelete(HttpContext, id);

            await _useCaseService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("{id}/attachments")]
        public async Task<ActionResult<DataResponseSchema<List<AttachmentDto>, DataResponseMeta>>> GetAttachments(Guid id)
        {
            IList<AttachmentEntity> attachments = await _attachmentService.GetByUseCaseAsync(id);
            return Ok(_mapper.Map<IList<AttachmentDto>>(attachments));
        }

        [HttpPost("{id}/attachments")]
        public async Task<ActionResult<DataResponseSchema<AttachmentDto, DataResponseMeta>>> PostAttachment(Guid id, [FromBody] CreateAttachmentBaseDto dto)
        {
            AttachmentType? type = AttachmentEntity.TypeFromString(dto.Type);
            if (type == null)
            {
                throw new BadRequestException($"Unknown attachment type {type}");
            }
            await AssertCanAddAttachment(HttpContext, id, (AttachmentType)type);

            var attachment = await _attachmentService.CreateAsync(id, _mapper.Map<AttachmentEntity>(dto));
            return StatusCode(201, _mapper.Map<AttachmentDto>(attachment));
        }

        [HttpPost("{id}/enable")]
        public async Task<ActionResult<DataResponseSchema<UseCaseDto, DataResponseMeta>>> PostEnabledStatus(Guid id)
        {
            await AssertChangeStatus(HttpContext, id, UseCaseStatus.Enabled);
            UseCaseEntity useCase = await _useCaseService.SetStatusAsync(id, UseCaseStatus.Enabled);
            return Ok(_mapper.Map<UseCaseDto>(useCase));
        }

        [HttpPost("{id}/decline")]
        public async Task<ActionResult<DataResponseSchema<UseCaseDto, DataResponseMeta>>> PostDeclinedStatus(Guid id)
        {
            await AssertChangeStatus(HttpContext, id, UseCaseStatus.Declined);
            UseCaseEntity useCase = await _useCaseService.SetStatusAsync(id, UseCaseStatus.Declined);
            return Ok(_mapper.Map<UseCaseDto>(useCase));
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
                UseCaseEntity current = await _useCaseService.GetByIdAsync(id);
                context.AssertUserIdOrFail(current.CreatedBy);

                UseCaseStep currentStep = current.CurrentStep;

                var role = UseCaseStepEntity.RolesDictionary[(UseCaseStep)currentStep];
                if (!authInfo.ContainsRole(role))
                {
                    throw new ForbiddenException($"Changing use case in step {currentStep} is permitted");
                }
            }
        }

        private async Task AssertCanEdit(HttpContext context, Guid id, UseCaseStep step)
        {
            await AssertCanEdit(context, id);

            var authInfo = context.GetAuthorizationOrFail();
            if (!authInfo.ContainsRole(UseCaseAppRole.AIQX_TEAM))
            {
                var role = UseCaseStepEntity.RolesDictionary[step];
                if (!authInfo.ContainsRole(role))
                {
                    throw new ForbiddenException($"Changing use case in step {step} is permitted");
                }

                UseCaseEntity current = await _useCaseService.GetByIdAsync(id);
                if (current.getCompletedSteps().Any(s => s.Type == step))
                {
                    throw new ForbiddenException($"Cannot change submitted step");
                }
            }
        }

        private async Task AssertCanAddAttachment(HttpContext context, Guid id, AttachmentType type)
        {
            if (context.IsInternalRequest())
            {
                return;
            }

            var authInfo = context.GetAuthorizationOrFail();
            if (!authInfo.ContainsRole(UseCaseAppRole.AIQX_TEAM))
            {
                UseCaseEntity current = await _useCaseService.GetByIdAsync(id);
                context.AssertUserIdOrFail(current.CreatedBy);

                UseCaseStep allowedStep = AttachmentEntity.StepsDictionary.GetValueOrDefault(type);
                if (allowedStep != current.CurrentStep)
                {
                    throw new ForbiddenException("Attachment locked");
                }
            }
        }

        private async Task AssertCanDelete(HttpContext context, Guid id)
        {
            if (context.IsInternalRequest())
            {
                return;
            }

            var authInfo = context.GetAuthorizationOrFail();
            if (!authInfo.ContainsRole(UseCaseAppRole.AIQX_TEAM))
            {
                UseCaseEntity current = await _useCaseService.GetByIdAsync(id);
                context.AssertUserIdOrFail(current.CreatedBy);
                UseCaseStep? step = current.GetLastStepOrNull();

                if (step != null)
                {
                    throw new ForbiddenException($"Cannot delete submitted use case");
                }
            }
        }

        private async Task AssertChangeStatus(HttpContext context, Guid id, UseCaseStatus status)
        {
            if (context.IsInternalRequest())
            {
                return;
            }

            var authInfo = context.GetAuthorizationOrFail();
            if (!authInfo.ContainsRole(UseCaseAppRole.AIQX_TEAM))
            {
                if (status == UseCaseStatus.Enabled)
                {
                    throw new ForbiddenException($"Cannot re-enable a use case");
                }

                UseCaseEntity current = await _useCaseService.GetByIdAsync(id);
                context.AssertUserIdOrFail(current.CreatedBy);

                if (current.CurrentStep != UseCaseStep.Order)
                {
                    throw new ForbiddenException($"Cannot decline use case in current step");
                }
            }
        }
    }
}
