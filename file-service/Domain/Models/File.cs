using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using AIQXCommon.Models;
using AIQXFileService.Implementation.Services;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace AIQXFileService.Domain.Models
{
    [Table("files")]
    public class FileEntity : UpdatedAtModel
    {

        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public ICollection<RefIdEntity> RefIds { get; set; }

        [Required]
        public ICollection<TagEntity> Tags { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string CreatedBy { get; set; }

        [Required]
        public long Size { get; set; }

        [Required]
        public string ContentType { get; set; }
    }

    public class CreateFileDto
    {
        public IFormFile File { get; set; }

        public List<string> RefIds { get; set; }

        public List<string> Tags { get; set; }
    }

    public class UpdateFileDto
    {
        public List<string> RefIds { get; set; }

        public List<string> Tags { get; set; }
    }

    public class FileAutoMapperProfile : Profile
    {

        public FileAutoMapperProfile()
        {
            CreateMap<FileEntity, FileDto>()
                .ForMember(
                    dest => dest.Filename,
                    opt => opt.MapFrom(src => src.Name)
                )
                .ForMember(
                    dest => dest.RefIds,
                    opt => opt.MapFrom(src => src.RefIds.Select(r => r.Value).ToList())
                )
                .ForMember(
                    dest => dest.Tags,
                    opt => opt.MapFrom(src => src.Tags.Select(t => t.Value).ToList())
                )
                .ReverseMap();
        }

    }

}
