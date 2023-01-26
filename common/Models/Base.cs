using System;
using System.ComponentModel.DataAnnotations;

namespace AIQXCommon.Models
{
    public class UpdatedAtModel
    {
        [Required]
        public DateTime UpdatedAt { get; set; }
    }
}
