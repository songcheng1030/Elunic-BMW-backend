using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIQXCoreService.Domain.Models;
using AIQXCoreService.Implementation.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Opw.HttpExceptions;

namespace AIQXCoreService.Implementation.Services
{
    public class UseCaseIndexingService
    {
        private readonly ILogger<UseCaseIndexingService> _logger;
        private readonly AppDbContext _dbContext;

        public UseCaseIndexingService(ILogger<UseCaseIndexingService> logger, AppDbContext context)
        {
            _logger = logger;
            _dbContext = context;
        }

        public async Task IndexAsync(UseCaseEntity useCase)
        {
            // UseCaseStepContentEntity content = new UseCaseStepContentEntity()
            // {
            //     Path = "building",
            //     Content = item.Value.ToString(),
            //     UseCaseStep = step
            // };

            // await _dbContext.AddAsync(content);

            // await _dbContext.SaveChangesAsync();
        }

        public async Task IndexAsync(UseCaseStepEntity step)
        {
            var entries = DeserializeAndFlatten(step.Form);
            foreach (var item in entries)
            {
                if (item.Value is string && !String.IsNullOrEmpty(item.Value.ToString()))
                {
                    var existing = _dbContext.UseCaseStepContent.Where(c => c.Path == item.Key && c.UseCaseStep == step).FirstOrDefault();
                    if (existing == null)
                    {
                        UseCaseStepContentEntity content = new UseCaseStepContentEntity()
                        {
                            Path = item.Key,
                            Content_EN = item.Value.ToString(),
                            Content_DE = item.Value.ToString(),
                            UseCaseStep = step
                        };
                        await _dbContext.AddAsync(content);
                    }
                    else
                    {
                        var entry = _dbContext.Entry(existing);
                        entry.Entity.Content_EN = item.Value.ToString();
                        entry.Entity.Content_DE = item.Value.ToString();
                        await _dbContext.SaveChangesAsync();
                    }

                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<IList<SearchResult>> SearchAsync(string q)
        {
            var contentResults = await _dbContext.UseCaseStepContent
                // Search both in english and german.
                .Where(c => EF.Functions.FreeText(c.Content_EN, q, 1033) || EF.Functions.FreeText(c.Content_DE, q, 1031) || EF.Functions.Like(c.Content_EN, $"%{q}%"))
                .Select(c => c.UseCaseStep)
                .Distinct()
                .Include(c => c.UseCase)
                .Include(c => c.UseCase.Plant)
                .ToListAsync();

            IList<UseCaseEntity> useCaseResults = await _dbContext.UseCases
                .Where(c => EF.Functions.Like(c.Name, $"%{q}%"))
                .Include(c => c.Plant)
                .ToListAsync();

            Dictionary<UseCaseEntity, HashSet<UseCaseStepEntity>> results = new Dictionary<UseCaseEntity, HashSet<UseCaseStepEntity>>();
            foreach (var item in contentResults)
            {
                var exiting = results.Where(pair => pair.Key.Id == item.UseCase.Id).FirstOrDefault();
                if (!exiting.Equals(default(KeyValuePair<UseCaseEntity, HashSet<UseCaseStepEntity>>)))
                {
                    var step = exiting.Value.Where(s => item.Id == s.Id).FirstOrDefault();
                    if (step == null)
                    {
                        exiting.Value.Add(item);
                    }
                }
                else
                {
                    var set = new HashSet<UseCaseStepEntity>();
                    set.Add(item);
                    results.Add(item.UseCase, set);
                }
            }

            foreach (var item in useCaseResults)
            {
                var exiting = results.Where(pair => pair.Key.Id == item.Id).FirstOrDefault();
                if (exiting.Equals(default(KeyValuePair<UseCaseEntity, HashSet<UseCaseStepEntity>>)))
                {
                    results.Add(item, new HashSet<UseCaseStepEntity>());
                }
            }

            return results.Select(pair => new SearchResult
            {
                UseCase = pair.Key,
                Steps = pair.Value.ToList()
            }).ToList();
        }

        private Dictionary<string, object> DeserializeAndFlatten(string json)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            JToken token = JToken.Parse(json);
            FillDictionaryFromJToken(dict, token, "");
            return dict;
        }

        private void FillDictionaryFromJToken(Dictionary<string, object> dict, JToken token, string prefix)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (JProperty prop in token.Children<JProperty>())
                    {
                        FillDictionaryFromJToken(dict, prop.Value, Join(prefix, prop.Name));
                    }
                    break;

                case JTokenType.Array:
                    int index = 0;
                    foreach (JToken value in token.Children())
                    {
                        FillDictionaryFromJToken(dict, value, Join(prefix, index.ToString()));
                        index++;
                    }
                    break;

                default:
                    dict.Add(prefix, ((JValue)token).Value);
                    break;
            }
        }

        private string Join(string prefix, string name)
        {
            return (string.IsNullOrEmpty(prefix) ? name : prefix + "." + name);
        }
    }
}
