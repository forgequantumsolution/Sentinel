using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;

namespace Core.Entities
{
    /// <summary>
    /// Represents any object in the application — features, folders, URLs, files, UI components, etc.
    /// Uses ObjectType for type discrimination and ParentObjectId for hierarchy.
    /// Folders are simply ActionObjects with ObjectType = Folder.
    /// </summary>
    public class ActionObject : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Code { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public ObjectType ObjectType { get; set; } = ObjectType.Feature;

        /// <summary>
        /// Route path for navigation. Auto-calculated for Folders if not provided.
        /// Root: /{name-slug}, Child: {parentRoute}/{name-slug}
        /// </summary>
        [MaxLength(1000)]
        public string? Route { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; }

        public int SortOrder { get; set; } = 0;

        // ── Hierarchy ──

        public Guid? ParentObjectId { get; set; }

        [ForeignKey("ParentObjectId")]
        public virtual ActionObject? ParentObject { get; set; }

        public virtual ICollection<ActionObject> ChildObjects { get; set; } = new List<ActionObject>();

        // ── Navigation ──

        public virtual ICollection<ActionObjectPermissionAssignment> Assignments { get; set; } = new List<ActionObjectPermissionAssignment>();
    }
}
