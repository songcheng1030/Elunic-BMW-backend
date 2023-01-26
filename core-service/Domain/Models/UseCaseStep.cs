using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using AIQXCommon.Auth;
using AutoMapper;
using Newtonsoft.Json;

namespace AIQXCoreService.Domain.Models
{

    public enum UseCaseStep
    {
        [EnumMember(Value = "initial-request")]
        InitialRequest = 1,

        [EnumMember(Value = "initial-feasibility-check")]
        InitialFeasibilityCheck = 2,

        [EnumMember(Value = "detailed-request")]
        DetailedRequest = 3,

        [EnumMember(Value = "offer")]
        Offer = 4,

        [EnumMember(Value = "order")]
        Order = 5,

        [EnumMember(Value = "setup-details")]
        SetupDetails = 6,

        [EnumMember(Value = "implementation")]
        Implementation = 7,

        [EnumMember(Value = "testing")]
        Testing = 8,

        [EnumMember(Value = "approval")]
        Approval = 9,

        [EnumMember(Value = "live")]
        Live = 10
    }

    [Table("use_case_steps")]
    public class UseCaseStepEntity
    {
        public static ImmutableDictionary<string, UseCaseStep> StepsDictionary = Enum.GetValues(typeof(UseCaseStep))
            .Cast<UseCaseStep>()
            .ToImmutableDictionary(item => StepToString(item), item => item);

        public static List<UseCaseStep> StepsOrder = Enum.GetValues(typeof(UseCaseStep))
            .Cast<UseCaseStep>()
            .ToList();

        public static ImmutableDictionary<UseCaseStep, UseCaseAppRole> RolesDictionary = new Dictionary<UseCaseStep, UseCaseAppRole>  {
            { UseCaseStep.InitialRequest, UseCaseAppRole.REQUESTOR },
            { UseCaseStep.InitialFeasibilityCheck, UseCaseAppRole.AIQX_TEAM },
            { UseCaseStep.DetailedRequest, UseCaseAppRole.REQUESTOR },
            { UseCaseStep.Offer, UseCaseAppRole.AIQX_TEAM },
            { UseCaseStep.Order, UseCaseAppRole.REQUESTOR },
            { UseCaseStep.SetupDetails, UseCaseAppRole.AIQX_TEAM },
            { UseCaseStep.Implementation, UseCaseAppRole.AIQX_TEAM },
            { UseCaseStep.Testing, UseCaseAppRole.AIQX_TEAM },
            { UseCaseStep.Approval, UseCaseAppRole.REQUESTOR },
            { UseCaseStep.Live, UseCaseAppRole.AIQX_TEAM }
        }.ToImmutableDictionary();

        public static string StepToString(UseCaseStep step)
        {
            FieldInfo field = step.GetType().GetField(step.ToString());

            EnumMemberAttribute attribute
                = Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute))
                    as EnumMemberAttribute;

            return attribute == null ? step.ToString() : attribute.Value;
        }

        public static UseCaseStep? StepFromString(string step)
        {
            return StepsDictionary.GetValueOrDefault(step);
        }

        public static bool IsAiqxStep(UseCaseStep step)
        {
            return RolesDictionary[step] == UseCaseAppRole.AIQX_TEAM;
        }

        public Guid Id { get; set; }

        [Required]
        public UseCaseStep Type { get; set; }

        [Required]
        public string Form { get; set; }

#nullable enable
        public DateTime? CompletedAt { get; set; }
#nullable disable

        [Required]
        public string CreatedBy { get; set; }

        [Required]
        public UseCaseEntity UseCase { get; set; }

        [Required]
        public ICollection<UseCaseStepContentEntity> Content { get; set; }

#nullable enable
        /// <summary>Duration in minutes</summary>
        public int Duration { get; set; }
#nullable disable
    }

    public class UseCaseStepDto
    {
        public string Type { get; set; }

        public Object Form { get; set; }

#nullable enable
        public DateTime? CompletedAt { get; set; }
#nullable disable

        public string CreatedBy { get; set; }
    }

    public class UpdateUseCaseStepDto
    {
        public Object Form { get; set; }
    }

    public class UseCaseStepAutoMapperProfile : Profile
    {
        public UseCaseStepAutoMapperProfile()
        {
            CreateMap<UseCaseStepEntity, UseCaseStepDto>()
                .ForMember(
                    dest => dest.Form,
                    opt => opt.MapFrom(src => JsonConvert.DeserializeObject(src.Form))
                ).ForMember(
                    dest => dest.Type,
                    opt => opt.MapFrom(src => UseCaseStepEntity.StepToString(src.Type))
                ).ReverseMap();
        }
    }
}