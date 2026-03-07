using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class Role : TenantEntity
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
        
        public bool IsDefault { get; set; } = false;
        
        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
