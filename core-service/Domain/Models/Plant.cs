using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper;

namespace AIQXCoreService.Domain.Models
{
    [Table("plants")]
    public class PlantEntity
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Country { get; set; }

        [Range(0, Int32.MaxValue)]
        public int Position { get; set; }

        public ICollection<UseCaseEntity> UseCases { get; set; }
    }

    public class PlantDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Country { get; set; }

        public int Position { get; set; }
    }

    public class CreatePlantDto
    {
        [Required]
        [RegularExpression(@"^[^.]*$")]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Country { get; set; }

        [Range(0, Int32.MaxValue)]
        public int Position { get; set; }
    }

    public class UpdatePlantDto
    {
#nullable enable
        public string? Name { get; set; }

        public string? Country { get; set; }

        [Range(0, Int32.MaxValue)]
        public int? Position { get; set; }
#nullable disable

        public void AssignNullFields(PlantEntity entity)
        {
            if (Name == null)
                Name = entity.Name;
            if (Country == null)
                Country = entity.Country;
            if (Position == null)
                Position = entity.Position;
        }
    }
    public class PlantAutoMapperProfile : Profile
    {

        public PlantAutoMapperProfile()
        {
            CreateMap<PlantEntity, PlantDto>().ReverseMap();
            CreateMap<CreatePlantDto, PlantEntity>().ReverseMap();
            CreateMap<UpdatePlantDto, PlantEntity>().ReverseMap();
        }

    }

}
