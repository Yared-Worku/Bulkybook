using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bulkybookweb.Models
{
    public class Category
    {
        [Key] 
        [Column("Category_Code")] 
        public Guid CategoryCode { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Display order must be between 1 and 100 only!")]
        public int? DisplayOrder { get; set; }

        [Column("Created_by")]
        public Guid? CreatedBy { get; set; }

        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;

        [ForeignKey("CreatedBy")]
        public virtual Users? Creator { get; set; }
    }
}