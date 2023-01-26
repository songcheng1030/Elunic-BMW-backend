using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AIQXCoreService.Domain.Models;
using AIQXCoreService.Implementation.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Opw.HttpExceptions;

namespace AIQXCoreService.Implementation.Services
{
    public class PlantService
    {
        private readonly ILogger<PlantService> _logger;
        private readonly AppDbContext _dbContext;

        public PlantService(ILogger<PlantService> logger, AppDbContext context)
        {
            _logger = logger;
            _dbContext = context;
        }

        public void ValidatePlantID(string PlantID)
        {
            Regex rgx = new Regex(@"^[P]\d{2}$");
            if (!rgx.IsMatch(PlantID))
            {
                throw new BadRequestException("Plant ID is not valid");
            }
        }

        public async Task<IList<PlantEntity>> GetAsync()
        {
            return await _dbContext.Plants
                .Include(p => p.UseCases)
                .ToListAsync();
        }

        public async Task<PlantEntity> GetByIdAsync(string id)
        {
            PlantEntity plant = await _dbContext.Plants
                .Where(p => p.Id == id)
                .Include(p => p.UseCases)
                .FirstOrDefaultAsync();

            if (plant == null)
            {
                throw new NotFoundException("Plant not found");
            }

            return plant;
        }

        public async Task<PlantEntity> CreateAsync(PlantEntity plant)
        {
            var dbEntity = await _dbContext.Plants.AddAsync(plant);
            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(dbEntity.Entity.Id);
        }

        public async Task<PlantEntity> UpdateAsync(string id, UpdatePlantDto dto)
        {
            PlantEntity plant = await _dbContext.Plants
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();

            dto.AssignNullFields(plant);
            var entry = _dbContext.Entry(plant);
            entry.CurrentValues.SetValues(dto);

            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(entry.Entity.Id);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            PlantEntity plant = await GetByIdAsync(id);
            _dbContext.Plants.Remove(plant);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
