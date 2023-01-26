using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using AIQXCommon.Middlewares;
using AIQXCoreService.Implementation.Jobs;
using AIQXCoreService.Implementation.Persistence;
using AIQXCoreService.Implementation.Services;
using AutoMapper;
using FluentEmail.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Opw.HttpExceptions.AspNetCore;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AIQXCoreService
{
    public class Startup
    {

        public IConfiguration Configuration { get; }

        public ILogger<Startup> Logger;

        public readonly static long StartTime = DateTimeOffset.Now.ToUnixTimeSeconds();

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

            services.AddFluentEmail(ConfigService.GetInstance().SmtpSenderAddress())
                    .AddLiquidRenderer(options =>
                    {
                        options.FileProvider = new PhysicalFileProvider(Path.Combine($"{Directory.GetCurrentDirectory()}/", "Assets")); ;
                    })
                    .AddSmtpSender(new SmtpClient(ConfigService.GetInstance().SmtpConnHost())
                    {
                        UseDefaultCredentials = false,
                        Port = ConfigService.GetInstance().SmtpConnPort(),
                        Credentials = new NetworkCredential(ConfigService.GetInstance().SmtpConnUserName(), ConfigService.GetInstance().SmtpConnPassword()),
                        EnableSsl = !ConfigService.GetInstance().isDev(),
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
            services.AddScoped<PlantService>();
            services.AddScoped<UseCaseService>();
            services.AddScoped<AttachmentService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<UseCaseIndexingService>();
            services.AddScoped<MigrationJob>();
            services.AddScoped<KpiService>();

            services.AddCors(o => o.AddPolicy("DevelopmentPolicy", builder =>
                {
                    builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed(_ => true);
                }));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("docs", new OpenApiInfo { Title = "Core Service API", Version = "v1" });
                c.SwaggerGeneratorOptions = new SwaggerGeneratorOptions
                {
                    DescribeAllParametersInCamelCase = true
                };
                c.EnableAnnotations();
            });
            services.AddSwaggerGenNewtonsoftSupport();

            services.AddHostedService<MigrationJob>();

            services.AddScoped<CronJobService, KpiJobService>();
            services.AddCronJob<KpiJob>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = $"*/{ConfigService.GetInstance().JobIntervalMinutes()} * * * *";
                c.RunOnStartup = true;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
                        swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"https://{httpReq.Host.Value}/core" } };
                    });
                });
            }

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("../swagger/v1/swagger.json", "Core Service API");
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
