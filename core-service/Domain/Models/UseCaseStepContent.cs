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

    [Table("use_case_step_content")]
    public class UseCaseStepContentEntity : UpdatedAtModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        public UseCaseStepEntity UseCaseStep { get; set; }

        [Required]
        public string Content_EN { get; set; }

        [Required]
        public string Content_DE { get; set; }
    }

    public class SearchResult
    {
        public UseCaseEntity UseCase;

        public IList<UseCaseStepEntity> Steps;
    }

    public class SearchResultDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string PlantId { get; set; }

        public string PlantName { get; set; }

        public string Status { get; set; }

        public string CurrentStep { get; set; }

        public IList<string> MatchingStepTypes { get; set; }
    }

    public class UseCaseContentQueryOptions
    {
        [Required]
        public string q { get; set; }
    }

    public class SearchResultAutoMapperProfile : Profile
    {
        public SearchResultAutoMapperProfile()
        {
            CreateMap<SearchResult, SearchResultDto>()
                .ForMember(
                    dest => dest.Id,
                    opt => opt.MapFrom(src => src.UseCase.Id)
                )
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => src.UseCase.Name)
                )
                .ForMember(
                    dest => dest.PlantId,
                    opt => opt.MapFrom(src => src.UseCase.Plant.Id)
                )
                .ForMember(
                    dest => dest.PlantName,
                    opt => opt.MapFrom(src => src.UseCase.Plant.Name)
                )
                .ForMember(
                    dest => dest.Status,
                    opt => opt.MapFrom(src => UseCaseEntity.StatusToString(src.UseCase.Status))
                )
                .ForMember(
                    dest => dest.CurrentStep,
                    opt => opt.MapFrom(src => UseCaseStepEntity.StepToString(src.UseCase.CurrentStep))
                )
                .ForMember(
                    dest => dest.MatchingStepTypes,
                    opt => opt.MapFrom(src => src.Steps.Select(s => UseCaseStepEntity.StepToString(s.Type)))
                ).ReverseMap();
        }
    }
}
