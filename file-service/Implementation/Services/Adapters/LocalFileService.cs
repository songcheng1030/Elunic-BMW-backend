using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AIQXFileService.Domain.Models;
using AIQXFileService.Implementation.Persistence;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Opw.HttpExceptions;

namespace AIQXFileService.Implementation.Services
{
    public class LocalFileService : IFileService
    {
        private readonly ILogger<LocalFileService> _logger;
        private readonly AppDbContext _dbContext;
        private readonly ConfigService _config;
        private readonly TokenService _tokenService;
        private readonly FileExtensionContentTypeProvider _contentTypeProvider;

        public LocalFileService(ILogger<LocalFileService> logger, AppDbContext context, ConfigService config, TokenService tokenService, FileExtensionContentTypeProvider contentTypeProvider)
        {
            _logger = logger;
            _dbContext = context;
            _config = config;
            _tokenService = tokenService;
            _contentTypeProvider = contentTypeProvider;
        }

        public async Task<List<FileEntity>> GetAsync()
        {
            return await _dbContext.Files
                .Include(f => f.RefIds)
                .Include(f => f.Tags)
                .ToListAsync();
        }

        public async Task<FileEntity> GetByIdAsync(Guid id)
        {
            var result = await _dbContext.Files
                .Where(a => a.Id == id)
                .Include(f => f.RefIds)
                .Include(f => f.Tags)
                .FirstOrDefaultAsync();
            if (result == null)
            {
                throw new NotFoundException();
            }
            return result;
        }

        public async Task<FileEntity> CreateFileFromUploadAsync(CreateFileDto dto, string createdBy)
        {
            var name = $"{Guid.NewGuid().ToString()}";
            _contentTypeProvider.TryGetContentType(dto.File.FileName, out string contentType);
            if (String.IsNullOrEmpty(contentType))
            {
                contentType = Path.GetExtension(dto.File.FileName);
            };

            var entity = new FileEntity()
            {
                Name = dto.File.FileName,
                RefIds = dto.RefIds != null ? createRefIds(dto.RefIds) : new List<RefIdEntity>(),
                Tags = dto.Tags != null ? createTags(dto.Tags) : new List<TagEntity>(),
                Size = dto.File.Length,
                ContentType = contentType,
                CreatedBy = createdBy
            };
            var dbEntity = _dbContext.Add(entity);
            await _dbContext.SaveChangesAsync();

            try
            {
                var filePath = GetFilePathById(dbEntity.Entity.Id);
                var dir = Path.GetDirectoryName(filePath);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (var stream = System.IO.File.Create(filePath))
                {
                    await dto.File.CopyToAsync(stream);
                }
            }
            catch (Exception e)
            {
                // Remove entity in case file write failed.
                _dbContext.Files.Remove(dbEntity.Entity);
                await _dbContext.SaveChangesAsync();
                throw e;
            }


            return await GetByIdAsync(dbEntity.Entity.Id);
        }

        public async Task<FileEntity> UpdateByIdAsync(Guid id, UpdateFileDto dto)
        {
            FileEntity file = await GetByIdAsync(id);
            var entry = _dbContext.Entry(file);

            if (dto.Tags != null && dto.Tags.Count() > 0)
            {
                entry.Entity.Tags = createTags(dto.Tags);
            }

            if (dto.RefIds != null && dto.RefIds.Count() > 0)
            {
                entry.Entity.RefIds = createRefIds(dto.RefIds);
            }

            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(id);
        }


        public async Task DeleteByIdAsync(Guid id)
        {
            var file = await GetByIdAsync(id);
            var filePath = GetFilePathById(id);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            _dbContext.Files.Remove(file);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<MemoryStream> GetImageStreamByIdAsync(Guid id)
        {
            var file = await GetByIdAsync(id);

            //If file not starts with image/ we will get the thumbnail of the file
            if (!file.ContentType.StartsWith("image/"))
            {
                return GetThumbnailFileImageStream(file);
            }
            return GetFileStream(file);
        }

        public async Task<MemoryStream> GetRawStreamByIdRawAsync(Guid id)
        {
            var file = await GetByIdAsync(id);
            return GetFileStream(file);
        }


        public async Task<List<FileEntity>> GetFilesByRefIdAsync(string refId)
        {
            return await _dbContext.Files
                .Where(x => x.RefIds.Any(r => r.Value == refId.ToString()))
                .ToListAsync();
        }

        public MemoryStream GetFileStream(FileEntity file)
        {
            var mem = new MemoryStream();
            File.OpenRead(GetFilePathById(file.Id)).CopyTo(mem);
            mem.Position = 0;
            return mem;
        }

        private string GetFilePathById(Guid id)
        {
            var str = id.ToString();
            return Path.Join(
                _config.StoragePath(),
                str.Substring(0, 1),
                str.Substring(1, 1),
                str
            );
        }

        private ICollection<TagEntity> createTags(List<string> tags)
        {
            var results = new List<TagEntity>();
            results.AddRange(tags.Where(t => t != null && t.Length > 0).Select(t => new TagEntity { Value = t }));
            return results;
        }

        private ICollection<RefIdEntity> createRefIds(List<string> refIds)
        {
            var results = new List<RefIdEntity>();
            results.AddRange(refIds.Where(r => r != null && r.Length > 0).Select(r => new RefIdEntity { Value = r }));
            return results;
        }

        private MemoryStream GetThumbnailFileImageStream(FileEntity file)
        {
            var ext = ExtractFileType(file);
            var mem = new MemoryStream();
            //if our file has no extension we will set a blank thumbnail
            if (ext == null)
            {
                throw new BadRequestException("Cannot find thumbnail for given file type");
            }

            var iconName = $"{ext}.png".ToLower();

            var iconDirPath = _config.IconsPath();
            var iconFileNames = new List<string>(Directory.GetFiles(iconDirPath));
            if (!iconFileNames.Select(n => Path.GetFileName(n)).Contains(iconName))
            {
                iconName = "_blank.png";
            }

            File.OpenRead(Path.Join(iconDirPath, iconName)).CopyTo(mem);
            mem.Position = 0;

            return mem;
        }

        private string ExtractFileType(FileEntity file)
        {
            // First: inspect the original file name since the browser
            // might send application/octet-stream for any other file
            // than images or other "really" common document formats

            var ext = Path.GetExtension(file.Name);
            if (!string.IsNullOrEmpty(ext))
            {
                return ext.Replace(".", string.Empty);
            }

            // Second: check via mime type library
            var contentType = file.ContentType;
            if (!string.IsNullOrEmpty(contentType))
            {
                return contentType;
            }

            // Can't identify
            return null;
        }


        public string GetMediaToken(Guid id)
        {
            var idData = new
            {
                fileId = id
            };
            string token = _tokenService.generateToken_Ed25519(_config.mediaTokenPrivateKey(), idData);
            return token;
        }
    }
}
