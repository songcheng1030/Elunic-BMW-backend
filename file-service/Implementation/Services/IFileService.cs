using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AIQXFileService.Domain.Models;
using AIQXFileService.Implementation.Persistence;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Opw.HttpExceptions;

namespace AIQXFileService.Implementation.Services
{
    public interface IFileService
    {
        Task<List<FileEntity>> GetAsync();

        Task<FileEntity> GetByIdAsync(Guid id);

        Task<FileEntity> CreateFileFromUploadAsync(CreateFileDto dto, string createdBy);

        Task<FileEntity> UpdateByIdAsync(Guid id, UpdateFileDto dto);

        Task DeleteByIdAsync(Guid id);

        Task<MemoryStream> GetImageStreamByIdAsync(Guid id);

        Task<MemoryStream> GetRawStreamByIdRawAsync(Guid id);

        Task<List<FileEntity>> GetFilesByRefIdAsync(string refId);

        MemoryStream GetFileStream(FileEntity file);

        string GetMediaToken(Guid id);
    }
}
