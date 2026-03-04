using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Analytics_BE.Core.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();        
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;        
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Guid? CreatedById { get; set; } = null;

        [ForeignKey("CreatedById")]
        public virtual User? CreatedBy { get; set; } = null!;
    }
}
