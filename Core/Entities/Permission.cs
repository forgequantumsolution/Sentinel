using System.ComponentModel.DataAnnotations;
using Analytics_BE.Core.Enums;

namespace Analytics_BE.Core.Entities
{
    public class Permission : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public List<string> Allowed { get; set; } = []; // create, read, update, delete

        public bool Inherited { get; set; } = true;

        public string? Description { get; set; }
        public PermissionType? _For { get; set; } // "File" or "Folder" or "System"
    }
}
