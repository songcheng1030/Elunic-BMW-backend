using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper;

namespace AIQXFileService.Domain.Models
{
    [Table("ref_ids")]
    public class RefIdEntity
    {
        public int Id { get; set; }

        [Required]
        public FileEntity File { get; set; }

        [Required]
        public string Value { get; set; }
    }

    public class RefIdDto
    {
        public int Id { get; set; }

        [Required]
        public string FileId { get; set; }

        [Required]
        public string Value { get; set; }

    }


    public class RefIdAutoMapperProfile : Profile
    {

        public RefIdAutoMapperProfile()
        {
            CreateMap<RefIdEntity, RefIdDto>()
                .ForMember(dest => dest.FileId, opts => opts.MapFrom(src => src.File.Id))
                .ReverseMap();
        }

    }

}
