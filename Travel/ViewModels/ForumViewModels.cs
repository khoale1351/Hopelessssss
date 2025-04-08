using System.ComponentModel.DataAnnotations;

namespace Travel.ViewModels
{
    public class ForumPostViewModel
    {
        public int PostId { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }

        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ViewCount { get; set; }
        public int CommentCount { get; set; }
        public bool IsPinned { get; set; }
        public List<int> CategoryIds { get; set; } = new List<int>();
    }

    public class ForumCommentViewModel
    {
        public int CommentId { get; set; }

        [Required(ErrorMessage = "Nội dung bình luận không được để trống")]
        public string Content { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ParentCommentId { get; set; }
        public int PostId { get; set; }
        public List<ForumCommentViewModel> Replies { get; set; } = new List<ForumCommentViewModel>();
    }

    public class ForumCategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string Description { get; set; }

        public int PostCount { get; set; }
    }
} 