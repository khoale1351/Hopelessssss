using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Travel.Controllers
{
    public class ForumController : Controller
    {
        // Danh sách bài viết (tạm thời lưu trong bộ nhớ)
        private static List<Post> Posts = new List<Post>();

        // Hiển thị trang forum
        public IActionResult Index()
        {
            return View(Posts);
        }

        // Hiển thị form đăng bài viết
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý đăng bài viết (POST)
        [HttpPost]
        public IActionResult Create(string title, string content)
        {
            // Tạo bài viết mới
            var post = new Post
            {
                Id = Posts.Count + 1,
                Title = title,
                Content = content,
                Likes = 0,
                Dislikes = 0
            };

            // Thêm bài viết vào danh sách
            Posts.Add(post);

            return RedirectToAction("Index");
        }

        // Xử lý like/dislike (POST)
        [HttpPost]
        public IActionResult Vote(int postId, string action)
        {
            var post = Posts.Find(p => p.Id == postId);
            if (post != null)
            {
                if (action == "like")
                {
                    post.Likes++;
                }
                else if (action == "dislike")
                {
                    post.Dislikes++;
                }
            }

            return RedirectToAction("Index");
        }

        // Model cho bài viết
        public class Post
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public int Likes { get; set; }
            public int Dislikes { get; set; }
        }
    }
}