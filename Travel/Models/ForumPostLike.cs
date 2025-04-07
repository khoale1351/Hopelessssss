using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Travel.Models
{
    public class ForumPostLike
    {
        [Key]
        public int LikeId { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual ForumPost Post { get; set; }

        public bool IsLike { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 