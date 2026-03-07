using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Analytics_BE.Core.Entities
{
    public class Department : TenantEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? Code { get; set; }

        public Guid? ParentDepartmentId { get; set; }

        [ForeignKey("ParentDepartmentId")]
        public virtual Department? ParentDepartment { get; set; }

        public virtual ICollection<Department> ChildDepartments { get; set; } = new List<Department>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
