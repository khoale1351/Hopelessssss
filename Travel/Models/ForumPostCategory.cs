using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Travel.Models
{
    public class ForumPostCategory
    {
        [Key]
        [Column(Order = 0)]
        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public virtual ForumPost Post { get; set; }

        [Key]
        [Column(Order = 1)]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual ForumCategory Category { get; set; }
    }
} 