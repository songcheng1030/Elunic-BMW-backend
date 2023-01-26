using System;
using System.Collections.Generic;
using AIQXCommon.Middlewares;
using AIQXFileService.Implementation.Persistence;
using AIQXFileService.Implementation.Services;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Opw.HttpExceptions.AspNetCore;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AIQXFileService
{
    public class Startup
    {

        public IConfiguration Configuration { get; }
        public ILogger<Startup> Logger;

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            // Initialize the config service as Singelton
            // to access it from withing the DbContext of a connection pool
            new ConfigService(configuration);

            Configuration = configuration;
            Logger = LoggerFactory.Create(builder => { builder.AddSerilog(); }).CreateLogger<Startup>();
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ConfigService>();


            services.AddMvc().AddHttpExceptions(options =>
            {
                options.IncludeExceptionDetails = _ => false;
                options.ShouldLogException = exception =>
                {
                    Logger.LogError(exception, "Exception while processing request");
                    return false;
                };
            });

            services.AddAutoMapper(System.Reflection.Assembly.GetExecutingAssembly());
            services.AddControllers(options => options.Filters.Add(typeof(ResponseWrapperResultFilter)))
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                });

            // SETUP
            services.AddDbContextPool<AppDbContext>(cfg =>
            {
                cfg.UseSqlServer(
                    ConfigService.GetInstance().SqlConnStr(),
                    x => x.MigrationsHistoryTable(ConfigService.GetInstance().TablePrefix() + "__EFMigrationsHistory").EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    )
                );
            });

            services.AddScoped<FileExtensionContentTypeProvider>();

            services.AddScoped<LocalFileService>();
            services.AddScoped<AzureFileService>();
            services.AddScoped<IFileService>(sp =>
            {
                var adapterType = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING") ?? Configuration["AZURE_STORAGE_CONNECTION_STRING"];
                if (adapterType != null)
                {
                    return sp.GetService<AzureFileService>();
                }
                else
                {
                    return sp.GetService<LocalFileService>();
                }
            });

            services.AddScoped<TokenService>();
            services.AddScoped<ImageConverter>();

            services.AddCors(o => o.AddPolicy("DevelopmentPolicy", builder =>
                {
                    builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed(_ => true);
                }));


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("docs", new OpenApiInfo { Title = "File Service API", Version = "v1" });
                c.SwaggerGeneratorOptions = new SwaggerGeneratorOptions
                {
                    DescribeAllParametersInCamelCase = true,
                    Servers = new List<OpenApiServer>()
                };
                c.EnableAnnotations();
            });
            services.AddSwaggerGenNewtonsoftSupport();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext context)
        {
            context.Database.Migrate();

            app.UseHttpExceptions();

            if (env.IsDevelopment())
            {
                app.UseCors("DevelopmentPolicy");
                app.UseSwagger();
            }
            else
            {
                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                    {
                        swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"https://{httpReq.Host.Value}/file" } };
                    });
                });
            }

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("../swagger/v1/swagger.json", "File Service API");
                c.RoutePrefix = "docs";
            });

            app.UseRouting();

            app.UseAuthMiddleware();
            app.UseRoleMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
