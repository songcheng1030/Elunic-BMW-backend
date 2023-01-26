using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using AIQXCoreService.Domain.Models;
using FluentEmail.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIQXCoreService.Implementation.Services
{
    public class NotificationService
    {
        public static ImmutableDictionary<UseCaseStep, string> emailTemplateDictionary = new Dictionary<UseCaseStep, string> {
            { UseCaseStep.InitialRequest, "initial-feasibility-check__" },
            { UseCaseStep.InitialFeasibilityCheck,  "initial-feasibility-check-completed__" },
            { UseCaseStep.DetailedRequest,  "final-feasibility-check__" },
            { UseCaseStep.Offer, "offer-available__" },
            { UseCaseStep.Order, "offer-accepted__" },
        }.ToImmutableDictionary();

        private readonly ILogger<NotificationService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public NotificationService(ILogger<NotificationService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public bool sendPlainTextEmail(string[] recipients, string subject, string body)
        {
            try
            {
                _logger.LogDebug($"Trying SMTP plain text mail sending");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var mailer = scope.ServiceProvider.GetRequiredService<IFluentEmail>();
                    var addresses = "";
                    foreach (var address in recipients)
                    {
                        addresses += address;
                        addresses += ";";
                    }
                    mailer.To(addresses.Remove(addresses.Length - 1, 1));
                    mailer.Subject(subject);
                    mailer.Body(body);
                    mailer.SendAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"SMTP mail sending error: {e.ToString()}");
                return false;
            }
            return true;
        }

        public bool sendHtmlEmail(string[] recipients, string name, string language, string extUrl, UseCaseStep step)
        {
            try
            {
                _logger.LogDebug($"Trying SMTP html text mail sending");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var templateUrl = emailTemplateDictionary[step];

                    var mailer = scope.ServiceProvider.GetRequiredService<IFluentEmail>();
                    mailer.To(String.Join(";", recipients));
                    // TODO: Translate.
                    mailer.Subject($"{name} - Action required");
                    mailer.UsingTemplateFromFile($"{Directory.GetCurrentDirectory()}/Assets/{templateUrl}{language}.html",
                    new
                    {
                        useCaseName = name,
                        logoUrl = "",
                        formUrl = extUrl
                    });
                    mailer.SendAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"SMTP mail sending error: {e.ToString()}");
                return false;
            }
            return true;
        }
    }
}