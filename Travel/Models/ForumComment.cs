using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Travel.Models
{
    public class ForumComment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public ForumPost Post { get; set; }

        public int? ParentCommentId { get; set; }

        [ForeignKey("ParentCommentId")]
        public ForumComment ParentComment { get; set; }

        public ICollection<ForumComment> Replies { get; set; } = new List<ForumComment>();
    }
} 