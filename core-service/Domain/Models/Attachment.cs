using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using AIQXCommon.Models;
using AutoMapper;
using Newtonsoft.Json;

namespace AIQXCoreService.Domain.Models
{
    public enum AttachmentType
    {
        [EnumMember(Value = "image")]
        Image = 1,

        [EnumMember(Value = "variant")]
        Variant,

        [EnumMember(Value = "test_result")]
        TestResult,
    }

    [Table("attachments")]
    public class AttachmentEntity : UpdatedAtModel
    {
        public static ImmutableDictionary<AttachmentType, UseCaseStep> StepsDictionary = new Dictionary<AttachmentType, UseCaseStep>  {
            { AttachmentType.Image, UseCaseStep.InitialRequest },
            { AttachmentType.Variant, UseCaseStep.DetailedRequest },
            { AttachmentType.TestResult, UseCaseStep.Approval },
        }.ToImmutableDictionary();

        public static ImmutableDictionary<string, AttachmentType> TypesDictionary = Enum.GetValues(typeof(AttachmentType))
            .Cast<AttachmentType>()
            .ToImmutableDictionary(item => TypeToString(item), item => item);

        public static string TypeToString(AttachmentType value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            EnumMemberAttribute attribute
                = Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute))
                    as EnumMemberAttribute;

            return attribute == null ? value.ToString() : attribute.Value;
        }

        public static AttachmentType? TypeFromString(string step)
        {
            return TypesDictionary.GetValueOrDefault(step);
        }

        public Guid Id { get; set; }

        [Required]
        public string RefId { get; set; }

        // TODO: Change to AttachmentType and add migration
        [Required]
        public string Type { get; set; }

        [Required]
        public string Metadata { get; set; }

        [Required]
        public UseCaseEntity UseCase { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class AttachmentDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string RefId { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public object Metadata { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Guid UseCaseId { get; set; }
    }

    public class CreateAttachmentBaseDto
    {
        [Required]
        public string RefId { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public object Metadata { get; set; }
    }

    public class CreateAttachmentDto : CreateAttachmentBaseDto
    {
        [Required]
        public Guid UseCaseId { get; set; }
    }

    public class UpdateAttachmentDto
    {
#nullable enable
        public string? RefId { get; set; }

        public string? Type { get; set; }

        public object? Metadata { get; set; }
#nullable disable

        public void AssignNullFields(AttachmentEntity entity)
        {
            if (RefId == null)
                RefId = entity.RefId;
            if (Type == null)
                Type = entity.Type;
            if (Metadata == null)
                Metadata = entity.Metadata;
        }
    }

    public class AttachmentAutoMapperProfile : Profile
    {
        public AttachmentAutoMapperProfile()
        {
            CreateMap<AttachmentEntity, AttachmentDto>().ForMember(
                dest => dest.Metadata,
                opt => opt.MapFrom(src => JsonConvert.DeserializeObject(src.Metadata))
            ).ForMember(
                dest => dest.UseCaseId,
                opt => opt.MapFrom(src => src.UseCase.Id)
            ).ReverseMap();
            CreateMap<CreateAttachmentDto, AttachmentEntity>().ForMember(
                dest => dest.Metadata,
                opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Metadata))
            ).ReverseMap();
            CreateMap<CreateAttachmentBaseDto, AttachmentEntity>().ForMember(
                dest => dest.Metadata,
                opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Metadata))
            ).ReverseMap();
        }
    }
}
