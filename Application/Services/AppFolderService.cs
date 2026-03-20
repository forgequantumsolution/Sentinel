using System.Text.RegularExpressions;
using Application.DTOs;
using Application.Common.Pagination;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Core.Entities;
using Core.Enums;

namespace Application.Services
{
    public class AppFolderService : IAppFolderService
    {
        private readonly IActionObjectRepository _repository;
        private readonly IGraphConfigRepository _graphConfigRepository;
        private readonly IGraphService _graphService;

        public AppFolderService(
            IActionObjectRepository repository,
            IGraphConfigRepository graphConfigRepository,
            IGraphService graphService)
        {
            _repository = repository;
            _graphConfigRepository = graphConfigRepository;
            _graphService = graphService;
        }

        public async Task<AppFolderDto?> GetByIdAsync(Guid id)
        {
            var obj = await _repository.GetByIdAsync(id);
            if (obj == null || obj.ObjectType != ObjectType.Folder) return null;
            return MapToDto(obj);
        }

        public async Task<PagedResult<AppFolderDto>> GetAllTreeAsync(PageRequest pageRequest)
        {
            var pagedResult = await _repository.GetByTypeAsync(ObjectType.Folder, pageRequest);
            var allItems = pagedResult.Items.ToList();
            var roots = allItems.Where(f => f.ParentObjectId == null).ToList();
            var treeDtos = roots.Select(f => MapToTreeDto(f, allItems)).ToList();

            return new PagedResult<AppFolderDto>
            {
                Items = treeDtos,
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<AppFolderDto?> GetByRouteAsync(string route)
        {
            var obj = await _repository.GetByRouteAsync(route);
            if (obj == null || obj.ObjectType != ObjectType.Folder) return null;
            return MapToDto(obj);
        }

        public async Task<AppFolderDto> CreateAsync(CreateAppFolderDto dto, Guid? userId)
        {
            string route;
            if (!string.IsNullOrWhiteSpace(dto.Route))
            {
                route = NormalizeRoute(dto.Route);
                if (await _repository.RouteExistsAsync(route))
                    throw new ArgumentException($"Route '{route}' already exists.");
            }
            else
            {
                route = await GenerateDefaultRouteAsync(dto.Name, dto.ParentObjectId);
            }

            var folder = new ActionObject
            {
                Name = dto.Name,
                Code = dto.Code,
                ObjectType = ObjectType.Folder,
                Route = route,
                Description = dto.Description,
                Icon = dto.Icon,
                SortOrder = dto.SortOrder,
                ParentObjectId = dto.ParentObjectId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(folder);
            return MapToDto(folder);
        }

        public async Task<AppFolderDto?> UpdateAsync(Guid id, UpdateAppFolderDto dto)
        {
            var folder = await _repository.GetByIdAsync(id);
            if (folder == null || folder.ObjectType != ObjectType.Folder) return null;

            if (!string.IsNullOrWhiteSpace(dto.Route))
            {
                var newRoute = NormalizeRoute(dto.Route);
                if (newRoute != folder.Route && await _repository.RouteExistsAsync(newRoute, id))
                    throw new ArgumentException($"Route '{newRoute}' already exists.");
                folder.Route = newRoute;
            }

            folder.Name = dto.Name;
            folder.Code = dto.Code;
            folder.Description = dto.Description;
            folder.Icon = dto.Icon;
            folder.SortOrder = dto.SortOrder;
            folder.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(folder);
            return MapToDto(folder);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var folder = await _repository.GetByIdAsync(id);
            if (folder == null || folder.ObjectType != ObjectType.Folder) return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        /// <summary>
        /// Move any ActionObject under a folder by setting its ParentObjectId.
        /// </summary>
        public async Task<bool> MoveToFolderAsync(Guid folderId, Guid actionObjectId)
        {
            var folder = await _repository.GetByIdAsync(folderId);
            if (folder == null || folder.ObjectType != ObjectType.Folder) return false;

            var child = await _repository.GetByIdAsync(actionObjectId);
            if (child == null) return false;

            child.ParentObjectId = folderId;
            child.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(child);
            return true;
        }

        /// <summary>
        /// Remove an ActionObject from its parent folder (set ParentObjectId = null).
        /// </summary>
        public async Task<bool> RemoveFromFolderAsync(Guid actionObjectId)
        {
            var child = await _repository.GetByIdAsync(actionObjectId);
            if (child == null || child.ParentObjectId == null) return false;

            child.ParentObjectId = null;
            child.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(child);
            return true;
        }

        public async Task<ActionObjectDto> CreateObjectInFolderAsync(Guid folderId, CreateActionObjectInFolderDto dto, Guid? userId)
        {
            var folder = await _repository.GetByIdAsync(folderId);
            if (folder == null || folder.ObjectType != ObjectType.Folder)
                throw new ArgumentException("Folder not found.");

            string? route = null;
            if (!string.IsNullOrWhiteSpace(dto.Route))
            {
                route = NormalizeRoute(dto.Route);
                if (await _repository.RouteExistsAsync(route))
                    throw new ArgumentException($"Route '{route}' already exists.");
            }
            else if (dto.ObjectType == ObjectType.Folder)
            {
                route = await GenerateDefaultRouteAsync(dto.Name, folderId);
            }

            var actionObject = new ActionObject
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                ObjectType = dto.ObjectType,
                Route = route,
                Icon = dto.Icon,
                SortOrder = dto.SortOrder,
                ParentObjectId = folderId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(actionObject);

            return MapToActionObjectDto(actionObject);
        }

        public async Task<PagedResult<ActionObjectDto>> GetFolderChildrenAsync(Guid folderId, PageRequest pageRequest)
        {
            var folder = await _repository.GetByIdAsync(folderId);
            if (folder == null || folder.ObjectType != ObjectType.Folder)
                throw new ArgumentException("Folder not found.");

            var pagedResult = await _repository.GetChildrenPagedAsync(folderId, pageRequest);
            var children = pagedResult.Items.ToList();

            // Batch-load linked data by ObjectType
            var dtos = children.Select(MapToActionObjectDto).ToList();
            await PopulateLinkedDataAsync(children, dtos);

            return new PagedResult<ActionObjectDto>
            {
                Items = dtos,
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        /// <summary>
        /// Batch-loads linked entity data for each ActionObject based on its ObjectType.
        /// </summary>
        private async Task PopulateLinkedDataAsync(List<ActionObject> actionObjects, List<ActionObjectDto> dtos)
        {
            // Graph: load GraphConfigs by ActionObjectIds
            var graphIds = actionObjects
                .Where(ao => ao.ObjectType == ObjectType.Graph)
                .Select(ao => ao.Id)
                .ToList();

            if (graphIds.Count > 0)
            {
                var graphConfigs = await _graphConfigRepository.GetByActionObjectIdsAsync(graphIds);
                var graphLookup = graphConfigs.ToDictionary(g => g.ActionObjectId!.Value);

                foreach (var dto in dtos.Where(d => d.ObjectType == nameof(ObjectType.Graph)))
                {
                    if (graphLookup.TryGetValue(dto.Id, out var config))
                    {
                        // Execute data source if exists, fallback to static Data JSON
                        var payload = await _graphService.GetGraphPayloadAsync(config.Id);

                        dto.Data = new GraphConfigDto
                        {
                            Id = config.Id,
                            Name = config.Name,
                            ComponentType = payload.ComponentType,
                            Type = (int)payload.Type,
                            View = payload.View,
                            Data = payload.Data,
                            Meta = payload.Meta,
                            FiltersParams = config.FiltersParams,
                            IsActive = config.IsActive,
                            CreatedAt = config.CreatedAt,
                            UpdatedAt = config.UpdatedAt,
                            CreatedById = config.CreatedById,
                            OrganizationId = config.OrganizationId
                        };
                    }
                }
            }

            // Add more ObjectType handlers here as needed (Form, Report, etc.)
        }

        // ── Route generation ──

        /// <summary>
        /// Generates a default route that doesn't conflict with existing routes.
        /// Root: /{folderName-slug}
        /// Child: {parentRoute}/{folderName-slug}
        /// Appends -2, -3, etc. on conflict.
        /// </summary>
        private async Task<string> GenerateDefaultRouteAsync(string name, Guid? parentObjectId)
        {
            var slug = Slugify(name);
            string baseRoute;

            if (parentObjectId == null)
            {
                baseRoute = $"/{slug}";
            }
            else
            {
                var parent = await _repository.GetByIdAsync(parentObjectId.Value);
                if (parent == null)
                    throw new ArgumentException("Parent object not found.");

                var parentRoute = parent.Route ?? $"/{Slugify(parent.Name)}";
                baseRoute = $"{parentRoute}/{slug}";
            }

            var route = baseRoute;
            int suffix = 2;
            while (await _repository.RouteExistsAsync(route))
            {
                route = $"{baseRoute}-{suffix}";
                suffix++;
            }

            return route;
        }

        private static string Slugify(string name)
        {
            var slug = name.Trim().ToLowerInvariant();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"-+", "-");
            return slug.Trim('-');
        }

        private static string NormalizeRoute(string route)
        {
            route = route.Trim();
            if (!route.StartsWith('/'))
                route = "/" + route;
            route = Regex.Replace(route, @"/+", "/");
            return route.TrimEnd('/');
        }

        // ── Mapping ──

        private static AppFolderDto MapToDto(ActionObject obj)
        {
            return new AppFolderDto
            {
                Id = obj.Id,
                Name = obj.Name,
                Code = obj.Code,
                Route = obj.Route ?? string.Empty,
                Description = obj.Description,
                Icon = obj.Icon,
                SortOrder = obj.SortOrder,
                ParentObjectId = obj.ParentObjectId,
                ParentName = obj.ParentObject?.Name,
                IsActive = obj.IsActive,
                CreatedAt = obj.CreatedAt
            };
        }

        private static AppFolderDto MapToTreeDto(ActionObject obj, List<ActionObject> allFolders)
        {
            var dto = MapToDto(obj);
            var children = allFolders.Where(f => f.ParentObjectId == obj.Id).ToList();
            dto.Children = children.Select(c => MapToTreeDto(c, allFolders)).ToList();
            return dto;
        }

        private static ActionObjectDto MapToActionObjectDto(ActionObject obj)
        {
            return new ActionObjectDto
            {
                Id = obj.Id,
                Name = obj.Name,
                Code = obj.Code,
                Description = obj.Description,
                ObjectType = obj.ObjectType.ToString(),
                Route = obj.Route,
                Icon = obj.Icon,
                SortOrder = obj.SortOrder,
                ParentObjectId = obj.ParentObjectId,
                ParentName = obj.ParentObject?.Name,
                IsActive = obj.IsActive,
                CreatedAt = obj.CreatedAt
            };
        }

    }
}
