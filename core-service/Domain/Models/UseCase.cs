using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AIQXCommon.Models;
using AutoMapper;
using Newtonsoft.Json;

namespace AIQXCoreService.Domain.Models
{
    public enum UseCaseStatus
    {
        [EnumMember(Value = "enabled")]
        Enabled = 1,

        [EnumMember(Value = "declined")]
        Declined = 2,
    }

    [Table("use_cases")]
    public class UseCaseEntity : UpdatedAtModel
    {
        public static ImmutableDictionary<string, UseCaseStatus> StatusDictionary = Enum.GetValues(typeof(UseCaseStatus))
            .Cast<UseCaseStatus>()
            .ToImmutableDictionary(item => StatusToString(item), item => item);

        public static List<UseCaseStatus> StatusOrder = Enum.GetValues(typeof(UseCaseStatus))
            .Cast<UseCaseStatus>()
            .ToList();

        public static string StatusToString(UseCaseStatus status)
        {
            FieldInfo field = status.GetType().GetField(status.ToString());

            EnumMemberAttribute attribute
                = Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute))
                    as EnumMemberAttribute;

            return attribute == null ? status.ToString() : attribute.Value;
        }

        public static UseCaseStatus? StatusFromString(string status)
        {
            return StatusDictionary.GetValueOrDefault(status);
        }

        public static string GetUseCaseName(Guid caseId, string plantId, string building, string band, string line, string name)
        {
            string uniqueId;
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(new UTF8Encoding().GetBytes(caseId.ToString()));
                var encoded = Convert.ToBase64String(hash);
                encoded = Regex.Replace(encoded, "[^0-9a-zA-Z]", "");
                uniqueId = encoded.Substring(0, 4);
            }

            if (band == null || band.Length == 0) band = "xx";
            if (line == null || line.Length == 0) line = "xxx";

            string finalName = plantId + ".H" + building + ".B" + band + ".T" + line + "." + uniqueId + "-" + name;

            if (finalName.Count(s => s == '.') != 4)
            {
                throw new Exception("Invalid use case name");
            }

            return finalName;
        }

        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Image { get; set; }

        [Required]
        public string Building { get; set; }

        public string Line { get; set; }

        public string Position { get; set; }

        [Required]
        public UseCaseStep CurrentStep { get; set; }

        public UseCaseStatus Status { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string CreatedBy { get; set; }

        [Required]
        public PlantEntity Plant { get; set; }

        public ICollection<AttachmentEntity> Attachments { get; set; }

        public ICollection<UseCaseStepEntity> Steps { get; set; }

        public UseCaseStep? GetLastStepOrNull()
        {
            var step = getCompletedSteps().LastOrDefault();
            if (step == null)
            {
                return null;
            }
            return step.Type;
        }

        public UseCaseStep? GetNextStepOrNull()
        {
            int index = getCompletedSteps().Count + 1;
            if (index >= UseCaseStepEntity.StepsOrder.Count)
            {
                return null;
            }
            return UseCaseStepEntity.StepsOrder[index];
        }

        public IList<UseCaseStepEntity> getCompletedSteps()
        {
            return Steps
                .Where(s => s.CompletedAt != null)
                .OrderBy(x => UseCaseStepEntity.StepsOrder.IndexOf(x.Type))
                .ToList();
        }

        public DateTime getLastCompletedStepDateOrCreatedAt()
        {
            var step = getCompletedSteps().LastOrDefault();
            return step == null ? CreatedAt : (DateTime)step.CompletedAt;
        }
    }

    public class UseCaseDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Image { get; set; }

        public string Building { get; set; }

        public string Line { get; set; }

        public string Position { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string CreatedBy { get; set; }

        public string PlantId { get; set; }

        public List<AttachmentDto> Attachments { get; set; }

        public List<UseCaseStepDto> Steps { get; set; }
    }

    public class CreateUseCaseDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [RegularExpression(@"^[^.]*$")]
        public string PlantId { get; set; }

        [Required]
        [RegularExpression(@"^[^.]*$")]
        public string Building { get; set; }

        public string Image { get; set; }

        [RegularExpression(@"^[^.]*$")]
        public string Line { get; set; }

        [RegularExpression(@"^[^.]*$")]
        public string Position { get; set; }

    }

    public class UpdateUseCaseDto
    {
#nullable enable
        public string? Name { get; set; }

        public string? Image { get; set; }

        [RegularExpression(@"^[^.]*$")]
        public string? Building { get; set; }

        [RegularExpression(@"^[^.]*$")]
        public string? Line { get; set; }

        [RegularExpression(@"^[^.]*$")]
        public string? Position { get; set; }

        [RegularExpression(@"^[^.]*$")]
        public string? PlantId { get; set; }
#nullable disable

        public void AssignNullFields(UseCaseEntity entity)
        {
            if (Image == null)
                Image = entity.Image;
            if (Building == null)
                Building = entity.Building;
            if (Line == null)
                Line = entity.Line;
            if (Position == null)
                Position = entity.Position;
            if (PlantId == null)
                PlantId = entity.Plant.Id;

            // Special handling name
            if (Name == null)
            {
                Name = entity.Name;
            }
            else
            {
                Name = UseCaseEntity.GetUseCaseName(entity.Id, PlantId, Building, Line, Position, Name);
            }
        }
    }

    public class UseCaseAutoMapperProfile : Profile
    {

        public UseCaseAutoMapperProfile()
        {
            CreateMap<UseCaseEntity, UseCaseDto>()
                .ForMember(
                    dest => dest.PlantId,
                    opt => opt.MapFrom(src => src.Plant.Id)
                ).ForMember(
                    dest => dest.Status,
                    opt => opt.MapFrom(src => UseCaseEntity.StatusToString(src.Status))
                ).ForMember(
                    dest => dest.Steps,
                    opt => opt.MapFrom((src, dest, i, context) => src.Steps
                        .Select(s => context.Mapper.Map<UseCaseStepEntity, UseCaseStepDto>(s)).ToList())
                ).ForMember(
                    dest => dest.Attachments,
                    opt => opt.MapFrom((src, dest, i, context) => src.Attachments
                        .Select(s => context.Mapper.Map<AttachmentEntity, AttachmentDto>(s)).ToList())
                ).ReverseMap();

            CreateMap<CreateUseCaseDto, UseCaseEntity>()
                .ReverseMap();

            CreateMap<UpdateUseCaseDto, UseCaseEntity>()
                .ReverseMap();
        }
    }

    public class UseCaseQueryOptions : PagingOption
    {
#nullable enable
        public string? q { get; set; }
        public string? plantId { get; set; }
        public string[]? status { get; set; }
        public string[]? steps { get; set; }
#nullable disable
    }
}
