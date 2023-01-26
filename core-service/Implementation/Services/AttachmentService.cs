using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIQXCoreService.Domain.Models;
using AIQXCoreService.Implementation.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Opw.HttpExceptions;

namespace AIQXCoreService.Implementation.Services
{
    public class AttachmentService
    {
        private readonly ILogger<AttachmentService> _logger;
        private readonly AppDbContext _dbContext;

        public AttachmentService(ILogger<AttachmentService> logger, AppDbContext context)
        {
            _logger = logger;
            _dbContext = context;
        }

        /// <summery>
        /// Returns a list of all attachments.
        /// </summery>
        public async Task<IList<AttachmentEntity>> GetAsync()
        {
            return await _dbContext.Attachments
                .Include(a => a.UseCase)
                .ToListAsync();
        }

        /// <summery>
        /// Returns an attachment.
        /// <exception cref="NotFoundExpection"> Throws Exception if attachment was not found.
        /// </summery>
        public async Task<AttachmentEntity> GetByIdAsync(Guid id)
        {
            AttachmentEntity attachment = await _dbContext.Attachments
                .Where(a => a.Id == id)
                .Include(a => a.UseCase)
                .Include(a => a.UseCase.Steps)
                .FirstOrDefaultAsync();
            if (attachment == null)
            {
                throw new NotFoundException("Attachment not found");
            }

            return attachment;
        }

        /// <summery>
        /// Returns all attachments of a usecase.
        /// </summery>
        public async Task<IList<AttachmentEntity>> GetByUseCaseAsync(Guid useCaseId)
        {
            return await _dbContext.Attachments
                .Where(a => a.UseCase.Id == useCaseId)
                .Include(a => a.UseCase)
                .ToListAsync();
        }

        /// <summery>
        /// Creates a new attachment.
        /// <exception cref="NotFoundExpection"> Thrown if no corresponding usecase was found.
        /// </summery>
        public async Task<AttachmentEntity> CreateAsync(Guid useCaseId, AttachmentEntity attachment)
        {
            UseCaseEntity useCase = await _dbContext.UseCases
                .Where(uc => uc.Id == useCaseId)
                .FirstOrDefaultAsync();
            if (useCase == null)
            {
                throw new NotFoundException("UseCase not found");
            }

            attachment.UseCase = useCase;
            var dbEntity = await _dbContext.AddAsync(attachment);
            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(dbEntity.Entity.Id);
        }

        /// <summery>
        /// Deletes an attachement.
        /// </summery>
        public async Task<bool> DeleteAsync(Guid id)
        {
            AttachmentEntity attachment = await GetByIdAsync(id);
            _dbContext.Remove(attachment);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        /// <summery>
        /// Updates an attachment.
        /// </summery>
        public async Task<AttachmentEntity> UpdateAsync(Guid id, UpdateAttachmentDto dto)
        {
            AttachmentEntity attachment = await GetByIdAsync(id);

            dto.AssignNullFields(attachment);
            var entry = _dbContext.Entry(attachment);
            dto.Metadata = JsonConvert.SerializeObject(dto.Metadata);
            entry.CurrentValues.SetValues(dto);

            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(entry.Entity.Id);
        }
    }
}
