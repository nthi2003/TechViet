using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechecomViet.Models;
using TechecomViet.Reponsitory;

namespace TechecomViet.Controllers
{
    public class ReviewController : BaseController
    {
        private readonly DataContext _dataContext;
        public ReviewController(DataContext context) : base(context)
        {
            _dataContext = context;
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReview(int productId, string comment, int rating)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var review = new ReviewModel
            {
                ProductId = productId,
                UserId = userId,
                Comment = comment,
                Rating = rating,
                CreatedAt = DateTime.Now
            };
            _dataContext.Reviews.Add(review);
            await _dataContext.SaveChangesAsync();

            TempData["Succes"] = "Đánh giá của bạn đã được gửi thành công!";
            return RedirectToAction("Detail", "Product", new { Id = productId });
        }
        public async Task<IActionResult> GetReview(int productId)
        {
            var review = await _dataContext.Reviews
                .Where(r => r.ProductId == productId)
                .ToListAsync();
            return View(review);
        }
    }
}
