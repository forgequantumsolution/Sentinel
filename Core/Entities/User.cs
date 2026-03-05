using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Analytics_BE.Core.Interfaces;
using Analytics_BE.Core.Enums;
using System.Text.Json.Serialization;

namespace Analytics_BE.Core.Entities
{
    public class User : BaseEntity, IRequestFlow
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        // Email verification fields
        public bool? IsEmailVerified { get; set; } = null;
        [JsonIgnore]
        public string? EmailVerificationToken { get; set; }
        [JsonIgnore]
        public DateTime? EmailVerificationTokenExpires { get; set; }

        // Password reset fields
        [JsonIgnore]
        public string? PasswordResetToken { get; set; }
        [JsonIgnore]
        public DateTime? PasswordResetTokenExpires { get; set; }


        [Required]
        public Guid RoleId { get; set; }
        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }
        
        public Guid? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        public Guid? JobTitleId { get; set; }
        [ForeignKey("JobTitleId")]
        public virtual JobTitle? JobTitle { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public Guid? ManagerId { get; set; }
        [ForeignKey("ManagerId")]
        public virtual User? Manager { get; set; }

        [MaxLength(100)]
        public string? EmployeeId { get; set; }

        public DateTime? HireDate { get; set; }

        [MaxLength(50)]
        public string? EmploymentType { get; set; } // Full-time, Part-time, Contractor, etc.

        [MaxLength(100)]
        public string? CostCenter { get; set; }

        [MaxLength(100)]
        public string? Division { get; set; }

        [MaxLength(100)]
        public string? BusinessUnit { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.InReview;

        // Navigation properties
        // (Removing dms-backend specific collections like OwnedFolders for now to keep it clean for Analytics_BE)
    }
}
