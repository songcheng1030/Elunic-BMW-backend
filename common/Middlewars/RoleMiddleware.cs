
using System.Linq;
using System.Threading.Tasks;
using AIQXCommon.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Opw.HttpExceptions;


namespace AIQXCommon.Middlewares
{
    public class RoleMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint == null)
            {
                // Pass, because no endpoint = no requirements placed
                await _next.Invoke(context);
                return;
            }

            var roles = endpoint.Metadata.GetMetadata<RequireRole>();
            var authInfo = context.GetAuthorizationOrNull();

            if (roles == null)
            {
                // Pass, because no requirements placed
                await _next.Invoke(context);
                return;
            }

            if (authInfo == null)
            {
                throw new ForbiddenException("Access denied");
            }

            if (authInfo.Roles.Count() < 1)
            {
                throw new ForbiddenException("User needs at least one role assigned");
            }

            // Ensure the the user has the required rights
            if (roles.allowedRoles.Count > 0)
            {
                var allowedRoles = roles.allowedRoles;
                if (!allowedRoles.Any(_ => authInfo.Roles.Contains(_)))
                {
                    throw new ForbiddenException("Insufficient privileges for operation");
                }
            }

            await _next.Invoke(context);
        }
    }

    public static class RoleMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleMiddleware>();
        }
    }
}

