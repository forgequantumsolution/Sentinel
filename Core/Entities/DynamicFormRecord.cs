using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class DynamicFormRecord : TenantEntity
    {
        [Required]
        public Guid FormId { get; set; }

        [ForeignKey("FormId")]
        public virtual DynamicForm Form { get; set; } = null!;

        // 75 Generic Fields for dynamic column mapping
        public string? Field1 { get; set; }
        public string? Field2 { get; set; }
        public string? Field3 { get; set; }
        public string? Field4 { get; set; }
        public string? Field5 { get; set; }
        public string? Field6 { get; set; }
        public string? Field7 { get; set; }
        public string? Field8 { get; set; }
        public string? Field9 { get; set; }
        public string? Field10 { get; set; }
        public string? Field11 { get; set; }
        public string? Field12 { get; set; }
        public string? Field13 { get; set; }
        public string? Field14 { get; set; }
        public string? Field15 { get; set; }
        public string? Field16 { get; set; }
        public string? Field17 { get; set; }
        public string? Field18 { get; set; }
        public string? Field19 { get; set; }
        public string? Field20 { get; set; }
        public string? Field21 { get; set; }
        public string? Field22 { get; set; }
        public string? Field23 { get; set; }
        public string? Field24 { get; set; }
        public string? Field25 { get; set; }
        public string? Field26 { get; set; }
        public string? Field27 { get; set; }
        public string? Field28 { get; set; }
        public string? Field29 { get; set; }
        public string? Field30 { get; set; }
        public string? Field31 { get; set; }
        public string? Field32 { get; set; }
        public string? Field33 { get; set; }
        public string? Field34 { get; set; }
        public string? Field35 { get; set; }
        public string? Field36 { get; set; }
        public string? Field37 { get; set; }
        public string? Field38 { get; set; }
        public string? Field39 { get; set; }
        public string? Field40 { get; set; }
        public string? Field41 { get; set; }
        public string? Field42 { get; set; }
        public string? Field43 { get; set; }
        public string? Field44 { get; set; }
        public string? Field45 { get; set; }
        public string? Field46 { get; set; }
        public string? Field47 { get; set; }
        public string? Field48 { get; set; }
        public string? Field49 { get; set; }
        public string? Field50 { get; set; }
        public string? Field51 { get; set; }
        public string? Field52 { get; set; }
        public string? Field53 { get; set; }
        public string? Field54 { get; set; }
        public string? Field55 { get; set; }
        public string? Field56 { get; set; }
        public string? Field57 { get; set; }
        public string? Field58 { get; set; }
        public string? Field59 { get; set; }
        public string? Field60 { get; set; }
        public string? Field61 { get; set; }
        public string? Field62 { get; set; }
        public string? Field63 { get; set; }
        public string? Field64 { get; set; }
        public string? Field65 { get; set; }
        public string? Field66 { get; set; }
        public string? Field67 { get; set; }
        public string? Field68 { get; set; }
        public string? Field69 { get; set; }
        public string? Field70 { get; set; }
        public string? Field71 { get; set; }
        public string? Field72 { get; set; }
        public string? Field73 { get; set; }
        public string? Field74 { get; set; }
        public string? Field75 { get; set; }
    }
}
