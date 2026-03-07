using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Analytics_BE.Core.Entities
{
    //public abstract class TenantEntity
    //{
    //    [Key]
    //    public Guid Id { get; set; } = Guid.NewGuid();        
    //    public bool IsActive { get; set; } = true;
    //    public bool IsDeleted { get; set; } = false;        
    //    public DateTime? DeletedAt { get; set; }
    //    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    //    public DateTime? UpdatedAt { get; set; }

    //    public Guid? CreatedById { get; set; } = null;

    //    [ForeignKey("CreatedById")]
    //    public virtual User? CreatedBy { get; set; } = null!;

    //    // Multi-tenancy: Organization scope
    //    public Guid? OrganizationId { get; set; }

    //    [ForeignKey("OrganizationId")]
    //    public virtual Organization? Organization { get; set; }
    //}
    // --------------------------------------------------
    // Base Entity (Minimal)
    // --------------------------------------------------
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    // --------------------------------------------------
    // Auditable Entity
    // Adds Created/Updated tracking
    // --------------------------------------------------
    public abstract class AuditableEntity : BaseEntity
    {
        public Guid? CreatedById { get; set; }

        [ForeignKey(nameof(CreatedById))]
        public virtual User? CreatedBy { get; set; }
    }

    // --------------------------------------------------
    // Tenant Entity (Multi-tenancy support)
    // --------------------------------------------------
    public abstract class TenantEntity : AuditableEntity
    {
        public Guid? OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization? Organization { get; set; }
    }
}
