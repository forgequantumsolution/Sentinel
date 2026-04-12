using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Core.Entities;
using Core.Enums;
using Infrastructure.Persistence;
using System.Reflection;

namespace WebAPI.Services
{
    /// <summary>
    /// On startup, scans all controllers for endpoints and inserts any missing
    /// ActionObjects (ObjectType=Url) into the database.
    /// </summary>
    public class EndpointSyncService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EndpointSyncService> _logger;

        public EndpointSyncService(IServiceProvider serviceProvider, ILogger<EndpointSyncService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var endpoints = DiscoverEndpoints();
            if (endpoints.Count == 0) return;

            var existingCodes = await context.ActionObjects
                .IgnoreQueryFilters()
                .Where(a => a.ObjectType == ObjectType.Url)
                .Select(a => a.Code)
                .ToListAsync(cancellationToken);

            var existingSet = new HashSet<string>(existingCodes.Where(c => c != null)!, StringComparer.OrdinalIgnoreCase);

            // Group endpoints by controller to create parent + children
            var grouped = endpoints.GroupBy(e => e.ControllerCode);
            int added = 0;

            foreach (var group in grouped)
            {
                var first = group.First();

                // Ensure parent exists
                if (!existingSet.Contains(first.ControllerCode))
                {
                    context.ActionObjects.Add(new ActionObject
                    {
                        Name = first.ControllerName + " API",
                        Code = first.ControllerCode,
                        Description = first.ControllerName + " endpoints",
                        ObjectType = ObjectType.Url,
                        Route = null,
                        SortOrder = 0
                    });
                    existingSet.Add(first.ControllerCode);
                    added++;
                }

                var parentId = await context.ActionObjects
                    .IgnoreQueryFilters()
                    .Where(a => a.Code == first.ControllerCode)
                    .Select(a => a.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                int sort = 0;
                foreach (var ep in group)
                {
                    if (!existingSet.Contains(ep.Code))
                    {
                        context.ActionObjects.Add(new ActionObject
                        {
                            Name = ep.Name,
                            Code = ep.Code,
                            Description = $"{ep.HttpMethod} {ep.Route}",
                            ObjectType = ObjectType.Url,
                            Route = null,
                            SortOrder = ++sort,
                            ParentObjectId = parentId != Guid.Empty ? parentId : null
                        });
                        existingSet.Add(ep.Code);
                        added++;
                    }
                }
            }

            if (added > 0)
            {
                await context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("EndpointSync: registered {Count} new API endpoint ActionObjects", added);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static List<EndpointInfo> DiscoverEndpoints()
        {
            var results = new List<EndpointInfo>();
            var controllerTypes = Assembly.GetEntryAssembly()?
                .GetTypes()
                .Where(t => !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t))
                ?? [];

            foreach (var controllerType in controllerTypes)
            {
                var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
                var controllerName = controllerType.Name.Replace("Controller", "");
                var baseRoute = routeAttr?.Template?.Replace("[controller]", controllerName.ToLower()) ?? $"/api/{controllerName.ToLower()}";
                var controllerCode = $"API_{controllerName.ToUpper()}";

                var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var method in methods)
                {
                    var httpAttr = method.GetCustomAttributes()
                        .OfType<HttpMethodAttribute>()
                        .FirstOrDefault();

                    if (httpAttr == null) continue;

                    var httpMethod = httpAttr switch
                    {
                        HttpGetAttribute => "GET",
                        HttpPostAttribute => "POST",
                        HttpPutAttribute => "PUT",
                        HttpDeleteAttribute => "DELETE",
                        HttpPatchAttribute => "PATCH",
                        _ => "GET"
                    };

                    var template = httpAttr.Template;
                    var route = string.IsNullOrEmpty(template)
                        ? baseRoute
                        : $"{baseRoute}/{template}";

                    var actionName = method.Name;
                    var code = $"{controllerCode}_{actionName.ToUpper()}";

                    results.Add(new EndpointInfo
                    {
                        ControllerName = controllerName,
                        ControllerCode = controllerCode,
                        ControllerRoute = baseRoute,
                        Name = FormatName(actionName),
                        Code = code,
                        Route = route,
                        HttpMethod = httpMethod
                    });
                }
            }

            return results;
        }

        private static string FormatName(string methodName)
        {
            // Insert spaces before uppercase letters: "GetAllUsers" -> "Get All Users"
            var chars = new List<char>();
            for (int i = 0; i < methodName.Length; i++)
            {
                if (i > 0 && char.IsUpper(methodName[i]) && !char.IsUpper(methodName[i - 1]))
                    chars.Add(' ');
                chars.Add(methodName[i]);
            }
            return new string(chars.ToArray());
        }

        private record EndpointInfo
        {
            public string ControllerName { get; init; } = "";
            public string ControllerCode { get; init; } = "";
            public string ControllerRoute { get; init; } = "";
            public string Name { get; init; } = "";
            public string Code { get; init; } = "";
            public string Route { get; init; } = "";
            public string HttpMethod { get; init; } = "";
        }
    }
}
