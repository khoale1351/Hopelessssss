using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Travel.Models
{
    public class ForumPost
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int ViewCount { get; set; } = 0;

        public bool IsPinned { get; set; } = false;

        public bool IsLocked { get; set; } = false;

        public int LikeCount { get; set; } = 0;

        public int DislikeCount { get; set; } = 0;

        public virtual ICollection<ForumComment> Comments { get; set; } = new List<ForumComment>();

        public virtual ICollection<ForumPostCategory> Categories { get; set; } = new List<ForumPostCategory>();

        public virtual ICollection<ForumPostLike> Likes { get; set; } = new List<ForumPostLike>();
    }
} 