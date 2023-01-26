using System;
using System.Collections.Generic;
using System.IO;

namespace AIQXCommon.Models
{
    public class FileDto
    {
        public Guid Id { get; set; }

        public string Filename { get; set; }

        public List<string> RefIds { get; set; }

        public List<string> Tags { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string CreatedBy { get; set; }

        public long Size { get; set; }

        public string ContentType { get; set; }

        // public string PublicUrl { get; set; }

        // public string PublicThumbnailUrl { get; set; }

        public string PrivateUrl { get; set; }

        // public string PrivateThumbnailUrl { get; set; }

        // public string RelativePublicUrl { get; set; }

        // public string RelativePrivateUrl { get; set; }
    }

    public class FileMetadata
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<string> RefIds { get; set; }
        public List<string> Tags { get; set; }
        public long Size { get; set; }
        public string Mime { get; set; }
        public DateTime CreatedAt { get; set; }
        public object exif { get; set; }
        public object extended { get; set; }
    }

    public static class FileTag
    {
        public const string LOCKED = "locked";
    }
}