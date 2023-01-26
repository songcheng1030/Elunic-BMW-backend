
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AIQXCommon.Auth;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Opw.HttpExceptions;

namespace AIQXCommon.Middlewares
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthMiddleware> _logger;
        public readonly static string AUTHORIZATION_INFO_NAME = "auth";
        public readonly static string INTERNAL_REQUEST_INFO_NAME = "internal";

        public AuthMiddleware(RequestDelegate next, ILogger<AuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Try to extract the token from the headers
            var header = context.Request.Headers["X-Auth-Request-Access-Token"];
            var tokenData = "";

            if (header.Count() < 1)
            {
                // If not available, check if it is an internal request.
                var internalHeader = context.Request.Headers["X-Internal-Request-Token"];
                if (internalHeader.Count() > 0)
                {
                    tokenData = internalHeader.First<string>();
                    if (!string.IsNullOrEmpty(tokenData))
                    {
                        var reqInfo = DecodeXInternalRequestToken(tokenData);
                        context.Items[INTERNAL_REQUEST_INFO_NAME] = reqInfo;
                        await _next.Invoke(context);
                        return;
                    }
                }
            }
            else
            {
                tokenData = header.First<string>();
            }

            // Decode the token and store it in the context
            if (!string.IsNullOrEmpty(tokenData))
            {
                var authInfo = DecodeXRequestAccessToken(tokenData);
                context.Items[AUTHORIZATION_INFO_NAME] = authInfo;
            }

            await _next.Invoke(context);
        }

#nullable enable
        private IDictionary<string, object> DecodeToken(string token, string? secret)
        {
            IDictionary<string, object> json = new Dictionary<string, object>();

            // First decode the token to get a json string, if this fails
            // we directly respond with an error
            try
            {
                if (secret == null)
                {
                    // Token does not need to be validated
                    IJsonSerializer serializer = new JsonNetSerializer();
                    IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                    IJwtDecoder decoder = new JwtDecoder(serializer, urlEncoder);
                    json = decoder.DecodeToObject<IDictionary<string, object>>(token);
                }
                else
                {
                    // Token does not need to be validated
                    IJsonSerializer serializer = new JsonNetSerializer();
                    var provider = new UtcDateTimeProvider();
                    IJwtValidator validator = new JwtValidator(serializer, provider);
                    IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                    IJwtAlgorithm algorithm = new HMACSHA256Algorithm(); // symmetric
                    IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
                    json = decoder.DecodeToObject<IDictionary<string, object>>(token, secret, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Invalid or malformed JWT token provided!");
                throw new ForbiddenException("Malformed token");
            }

            return json;
        }
#nullable disable

        private InternalRequestInformation DecodeXInternalRequestToken(string token)
        {
            IDictionary<string, object> json = DecodeToken(token, Environment.GetEnvironmentVariable("APP_INTERNAL_TOKEN_SECRET"));

            try
            {
                var originalClaims = (Newtonsoft.Json.Linq.JObject)json["original_claims"];
                var authInfo = new AuthInformation
                {
                    Id = (string)originalClaims["sub"],
                    Mail = (string)originalClaims["email"],
                    Roles = new List<UseCaseAppRole>(),
                };
                var reqInfo = new InternalRequestInformation
                {
                    ServerId = (string)json["id"],
                    AuthInfo = authInfo,
                };

                return reqInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to parse internal token: {json}");
                throw new ForbiddenException("Provided token cannot be parsed");
            }
        }

        private AuthInformation DecodeXRequestAccessToken(string token)
        {
            IDictionary<string, object> json = DecodeToken(token, null);

            // Assemble the auth info object which is then used subseqently
            try
            {
                var authInfo = new AuthInformation
                {
                    Id = (string)json["sub"],
                    Mail = (string)json["email"],
                    Roles = new List<UseCaseAppRole>(),
                };

                // Extract further data from the token, if available
                // and let null if not available
                if (json.ContainsKey("given_name") && !string.IsNullOrEmpty((string)json["given_name"]))
                {
                    authInfo.GivenName = (string)json["given_name"];
                }

                if (json.ContainsKey("family_name") && !string.IsNullOrEmpty((string)json["family_name"]))
                {
                    authInfo.FamilyName = (string)json["family_name"];
                }

                if (json.ContainsKey("name") && !string.IsNullOrEmpty((string)json["name"]))
                {
                    authInfo.DisplayName = (string)json["name"];
                }
                else if (authInfo.GivenName != null || authInfo.FamilyName != null)
                {
                    authInfo.DisplayName = ($"{authInfo.GivenName} {authInfo.FamilyName}").Trim();
                }

                if (json.ContainsKey("preferred_username") && !string.IsNullOrEmpty((string)json["preferred_username"]))
                {
                    authInfo.UserName = (string)json["preferred_username"];
                }
                else if (authInfo.DisplayName != null)
                {
                    authInfo.UserName = (string)json["preferred_username"];
                }
                else
                {
                    authInfo.UserName = (string)json["email"];
                }

                // Extract the groups from the IdP and transform it into 
                // roles to be used inside the app
                if (json.ContainsKey("groups"))
                {
                    var groups = (Newtonsoft.Json.Linq.JArray)json["groups"];
                    foreach (var group in groups)
                    {
                        var groupNameProvided = (string)group;
                        foreach (var roleMapping in GroupRolesMapping.Mappings)
                        {
                            foreach (var groupName in roleMapping.Value)
                            {
                                if (groupNameProvided.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                                {
                                    authInfo.Roles.Add(roleMapping.Key);
                                }
                            }

                        }
                    }

                    // Make everything unique
                    authInfo.Roles = authInfo.Roles.Distinct<UseCaseAppRole>().ToList<UseCaseAppRole>();
                }

                return authInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to parse token: {json}");
                throw new ForbiddenException("Provided token cannot be parsed");
            }
        }

    }

    public class InternalRequestInformation
    {
        public string ServerId { get; set; }
        public AuthInformation AuthInfo { get; set; }
    }

    public class AuthInformation
    {
        public string Id { get; set; }

#nullable enable
        public string? DisplayName { get; set; }

        public string? UserName { get; set; }

        public string? GivenName { get; set; }

        public string? FamilyName { get; set; }

#nullable disable
        public string Mail { get; set; }

        public IList<UseCaseAppRole> Roles { get; set; }

        public bool ContainsRole(UseCaseAppRole role)
        {
            return Roles.Contains(role);
        }

        public override string ToString()
        {
            return $"AuthInformation{JsonConvert.SerializeObject(this)}";
        }

        public object ToExternal()
        {
            return new
            {
                Id = this.Id,
                DisplayName = this.DisplayName,
                Username = this.UserName,
                GivenName = this.GivenName,
                FamilyName = this.FamilyName,
                Mail = this.Mail,
                Roles = this.Roles.Select(r => r.ToString()).ToArray<string>(),
            };
        }

    }

    public static class AuthContextExtension
    {
        public static bool IsInternalRequest(this HttpContext context)
        {
            return context.Items[AuthMiddleware.INTERNAL_REQUEST_INFO_NAME] != null;
        }
        public static AuthInformation GetAuthorizationOrNull(this HttpContext context)
        {
            if (context.Items[AuthMiddleware.AUTHORIZATION_INFO_NAME] == null)
            {
                return null;
            }
            return (AuthInformation)context.Items[AuthMiddleware.AUTHORIZATION_INFO_NAME];
        }

        public static AuthInformation GetAuthorizationOrFail(this HttpContext context)
        {
            if (context.Items[AuthMiddleware.AUTHORIZATION_INFO_NAME] == null)
            {
                throw new ForbiddenException("No permission for this ressource");
            }
            return (AuthInformation)context.Items[AuthMiddleware.AUTHORIZATION_INFO_NAME];
        }

        public static string GetAuthUserIdOrFail(this HttpContext context)
        {
            var auth = context.GetAuthorizationOrNull();

            if (auth == null || auth.Id == null)
            {
                throw new InvalidOperationException("No auth info available at this point");
            }

            return auth.Id;
        }

        public static bool AssertUserIdOrFail(this HttpContext context, string userId)
        {
            var auth = context.GetAuthorizationOrNull();

            if (auth == null || auth.Id == null)
            {
                throw new InvalidOperationException("No auth info available at this point");
            }

            return auth.Id.Equals(userId);
        }

    }

    public static class AuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthMiddleware>();
        }
    }
}
