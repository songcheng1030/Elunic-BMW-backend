using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper;

namespace AIQXFileService.Domain.Models
{
    [Table("tags")]
    public class TagEntity
    {
        public int Id { get; set; }

        [Required]
        public FileEntity File { get; set; }

        [Required]
        public string Value { get; set; }
    }

    public class TagDto
    {
        public int Id { get; set; }

        [Required]
        public string FileId { get; set; }

        [Required]
        public string Value { get; set; }

    }


    public class TagAutoMapperProfile : Profile
    {

        public TagAutoMapperProfile()
        {
            CreateMap<TagEntity, TagDto>()
                .ForMember(dest => dest.FileId, opts => opts.MapFrom(src => src.File.Id))
                .ReverseMap();
        }

    }

}
