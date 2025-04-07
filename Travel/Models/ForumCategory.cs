using System.ComponentModel.DataAnnotations;

namespace Travel.Models
{
    public class ForumCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int PostCount { get; set; }

        public virtual ICollection<ForumPostCategory> Posts { get; set; } = new List<ForumPostCategory>();
    }
} 