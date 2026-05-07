using Core.Enums;

namespace Application.DTOs
{
    public class AppFolderDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string Route { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
        public Guid? ParentObjectId { get; set; }
        public string? ParentName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<AppFolderDto> Children { get; set; } = new();
    }

    public class CreateAppFolderDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Route { get; set; } // null = auto-calculate default
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int SortOrder { get; set; } = 0;
        public Guid? ParentObjectId { get; set; }
        public Guid? DepartmentId { get; set; }
    }

    public class UpdateAppFolderDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Route { get; set; } // null = keep existing
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
        public Guid? DepartmentId { get; set; }
    }

    public class MoveToFolderDto
    {
        public Guid ActionObjectId { get; set; }
    }

    public class CreateActionObjectInFolderDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public ObjectType ObjectType { get; set; } = ObjectType.Feature;
        public string? Route { get; set; }
        public string? Icon { get; set; }
        public int SortOrder { get; set; } = 0;
        public Guid? DepartmentId { get; set; }
    }

    public class ActionObjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string ObjectType { get; set; } = string.Empty;
        public string? Route { get; set; }
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
        public Guid? ParentObjectId { get; set; }
        public string? ParentName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The actual linked entity data based on ObjectType.
        /// e.g., GraphConfigDto for Graph, null for Folder, etc.
        /// </summary>
        public bool HasChildren { get; set; }
        public object? Data { get; set; }
        public List<ActionObjectWithPermissionsDto>? ChildObjects { get; set; }
    }
}
