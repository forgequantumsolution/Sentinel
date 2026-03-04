using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Analytics_BE.Infrastructure.Middleware
{
    public class RequireAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HashSet<string> _publicEndpoints;

        public RequireAuthMiddleware(RequestDelegate next)
        {
            _next = next;

            // Define public endpoints that don't require authentication
            _publicEndpoints = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/forgot-password",
                "/api/auth/reset-password",
                "/api/auth/verify-email"
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the current endpoint is public
            var path = context.Request.Path.ToString();

            if (IsPublicEndpoint(path))
            {
                // Public endpoint, continue without authentication check
                await _next(context);
                return;
            }

            // Check if user is authenticated
            if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Authentication required");
                return;
            }

            // User is authenticated, continue to next middleware
            await _next(context);
        }

        private bool IsPublicEndpoint(string path)
        {
            // Check exact matches for auth endpoints
            if (_publicEndpoints.Contains(path))
                return true;

            // Check if it's a Swagger request
            if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
