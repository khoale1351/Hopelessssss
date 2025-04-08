using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travel.Data;
using Travel.Models;
using Travel.ViewModels;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Travel.Controllers
{
    public class ForumController : Controller
    {
        private readonly TourismDbContext _context;
        private readonly ILogger<ForumController> _logger;

        public ForumController(TourismDbContext context, ILogger<ForumController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Forum
        public async Task<IActionResult> Index()
        {
            var categories = await _context.ForumCategories
                .Select(c => new ForumCategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    PostCount = c.Posts.Count
                })
                .ToListAsync();

            var pinnedPosts = await _context.ForumPosts
                .Where(p => p.IsPinned)
                .Include(p => p.User)
                .Select(p => new ForumPostViewModel
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    UserId = p.UserId,
                    UserName = p.User.FullName,
                    CreatedAt = p.CreatedAt,
                    ViewCount = p.ViewCount,
                    CommentCount = p.Comments.Count,
                    IsPinned = p.IsPinned
                })
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentPosts = await _context.ForumPosts
                .Include(p => p.User)
                .Select(p => new ForumPostViewModel
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    UserId = p.UserId,
                    UserName = p.User.FullName,
                    CreatedAt = p.CreatedAt,
                    ViewCount = p.ViewCount,
                    CommentCount = p.Comments.Count,
                    IsPinned = p.IsPinned
                })
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.PinnedPosts = pinnedPosts;
            ViewBag.RecentPosts = recentPosts;

            return View();
        }

        // GET: Forum/Category/{id}
        public async Task<IActionResult> Category(int id)
        {
            var category = await _context.ForumCategories
                .Include(c => c.Posts)
                .ThenInclude(pc => pc.Post)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            var posts = category.Posts
                .Select(pc => new ForumPostViewModel
                {
                    PostId = pc.Post.PostId,
                    Title = pc.Post.Title,
                    UserId = pc.Post.UserId,
                    UserName = pc.Post.User.FullName,
                    CreatedAt = pc.Post.CreatedAt,
                    ViewCount = pc.Post.ViewCount,
                    CommentCount = pc.Post.Comments.Count,
                    IsPinned = pc.Post.IsPinned
                })
                .OrderByDescending(p => p.IsPinned)
                .ThenByDescending(p => p.CreatedAt)
                .ToList();

            ViewBag.Category = category;
            return View(posts);
        }

        // GET: Forum/Post/{id}
        public async Task<IActionResult> Post(int id)
        {
            var post = await _context.ForumPosts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            // Tăng lượt xem
            post.ViewCount++;
            await _context.SaveChangesAsync();

            var comments = post.Comments
                .Where(c => c.ParentCommentId == null)
                .Select(c => new ForumCommentViewModel
                {
                    CommentId = c.CommentId,
                    Content = c.Content,
                    UserId = c.UserId,
                    UserName = c.User.FullName,
                    CreatedAt = c.CreatedAt,
                    Replies = c.Replies
                        .Select(r => new ForumCommentViewModel
                        {
                            CommentId = r.CommentId,
                            Content = r.Content,
                            UserId = r.UserId,
                            UserName = r.User.FullName,
                            CreatedAt = r.CreatedAt,
                            ParentCommentId = r.ParentCommentId
                        })
                        .ToList()
                })
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            ViewBag.Comments = comments;
            return View(post);
        }

        // GET: Forum/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.ForumCategories.ToList();
            return View();
        }

        // POST: Forum/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(ForumPostViewModel model)
        {
            try
            {
                _logger.LogInformation("Bắt đầu tạo bài viết mới");
                
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState không hợp lệ");
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            _logger.LogWarning($"Lỗi validation: {error.ErrorMessage}");
                        }
                    }
                    ViewBag.Categories = await _context.ForumCategories.ToListAsync();
                    return View(model);
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("Không tìm thấy UserId");
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation($"UserId: {userId}");
                
                var post = new ForumPost
                {
                    Title = model.Title,
                    Content = model.Content,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation($"Tạo bài viết: {post.Title}");
                
                _context.ForumPosts.Add(post);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Đã lưu bài viết với ID: {post.PostId}");

                // Thêm categories
                if (model.CategoryIds != null && model.CategoryIds.Any())
                {
                    _logger.LogInformation($"Số danh mục được chọn: {model.CategoryIds.Count}");
                    
                    foreach (var categoryId in model.CategoryIds)
                    {
                        _logger.LogInformation($"Thêm danh mục với ID: {categoryId}");
                        var postCategory = new ForumPostCategory
                        {
                            PostId = post.PostId,
                            CategoryId = categoryId
                        };
                        _context.ForumPostCategories.Add(postCategory);
                    }
                    
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Đã lưu liên kết bài viết với danh mục");
                }
                else
                {
                    _logger.LogWarning("Không có danh mục nào được chọn");
                }

                return RedirectToAction(nameof(Post), new { id = post.PostId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bài viết");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo bài viết. Vui lòng thử lại sau.");
            }

            ViewBag.Categories = await _context.ForumCategories.ToListAsync();
            return View(model);
        }

        // POST: Forum/Comment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Comment(ForumCommentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var comment = new ForumComment
                {
                    Content = model.Content,
                    UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    PostId = model.PostId,
                    ParentCommentId = model.ParentCommentId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ForumComments.Add(comment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Post), new { id = model.PostId });
            }

            return RedirectToAction(nameof(Post), new { id = model.PostId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Like(int postId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var post = await _context.ForumPosts.FindAsync(postId);
            
            if (post == null)
            {
                return Json(new { success = false, message = "Bài viết không tồn tại" });
            }

            var existingLike = await _context.ForumPostLikes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike == null)
            {
                var like = new ForumPostLike
                {
                    PostId = postId,
                    UserId = userId,
                    IsLike = true
                };
                _context.ForumPostLikes.Add(like);
                post.LikeCount++;
            }
            else if (!existingLike.IsLike)
            {
                existingLike.IsLike = true;
                post.LikeCount++;
                post.DislikeCount--;
            }
            else
            {
                _context.ForumPostLikes.Remove(existingLike);
                post.LikeCount--;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Dislike(int postId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var post = await _context.ForumPosts.FindAsync(postId);
            
            if (post == null)
            {
                return Json(new { success = false, message = "Bài viết không tồn tại" });
            }

            var existingLike = await _context.ForumPostLikes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike == null)
            {
                var like = new ForumPostLike
                {
                    PostId = postId,
                    UserId = userId,
                    IsLike = false
                };
                _context.ForumPostLikes.Add(like);
                post.DislikeCount++;
            }
            else if (existingLike.IsLike)
            {
                existingLike.IsLike = false;
                post.DislikeCount++;
                post.LikeCount--;
            }
            else
            {
                _context.ForumPostLikes.Remove(existingLike);
                post.DislikeCount--;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleLock(int postId)
        {
            var post = await _context.ForumPosts.FindAsync(postId);
            if (post == null)
            {
                return Json(new { success = false, message = "Bài viết không tồn tại" });
            }

            post.IsLocked = !post.IsLocked;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isLocked = post.IsLocked });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int postId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var post = await _context.ForumPosts
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (post == null)
            {
                return Json(new { success = false, message = "Bài viết không tồn tại" });
            }

            if (!User.IsInRole("Admin") && post.UserId != userId)
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa bài viết này" });
            }

            _context.ForumPostLikes.RemoveRange(post.Likes);
            _context.ForumComments.RemoveRange(post.Comments);
            _context.ForumPosts.Remove(post);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}