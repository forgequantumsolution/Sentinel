using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Analytics_BE.Application.Interfaces;

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
            var path = context.Request.Path.ToString();

            if (IsPublicEndpoint(path))
            {
                await _next(context);
                return;
            }

            // Check if user is authenticated and claims are present
            if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                if (string.IsNullOrEmpty(authHeader))
                {
                    await context.Response.WriteAsync("{\"message\": \"Authentication required: Authorization header is missing\"}");
                }
                else if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    await context.Response.WriteAsync("{\"message\": \"Authentication required: Authorization header must use Bearer scheme\"}");
                }
                else
                {
                    await context.Response.WriteAsync("{\"message\": \"Authentication required: Token validation failed (Check if token is expired or secret key mismatch)\"}");
                }
                return;
            }

            // Fetch the user and put it in Items for easy access in controllers
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                try
                {
                    // Using context services to get dependencies
                    var userService = context.RequestServices.GetRequiredService<IUserService>();
                    var user = await userService.GetUserByIdAsync(userId);
                    
                    if (user != null)
                    {
                        context.Items["User"] = user;
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"message\": \"User not found or account disabled\"}");
                        return;
                    }
                }
                catch
                {
                    // Fallback if service resolution fails, but continue if we're authenticated
                }
            }

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
