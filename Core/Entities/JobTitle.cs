using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Analytics_BE.Core.Entities
{
    public class JobTitle : TenantEntity
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public Guid? DepartmentId { get; set; }
        
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }
        
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
