using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechecomViet.Models;
using TechecomViet.Models.ViewModel;
using TechecomViet.Reponsitory;

namespace TechecomViet.Controllers
{
    public class BlogController : BaseController
    {
        private readonly SignInManager<UserModel> _signInManager;
        private readonly UserManager<UserModel> _userManager;
        private readonly DataContext _dataContext;
        public BlogController(DataContext context) : base(context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Blogs.OrderByDescending(b => b.Id).ToListAsync());
        }
        public async Task<IActionResult> Detail(int Id)
        {
            var blog = await _dataContext.Blogs.FirstOrDefaultAsync(b => b.Id == Id);
            if(blog != null)
            {
                TempData["error"] = "Bài viết không tồn tại";
            }
            var relateBlog = await _dataContext.Blogs
                .Where(b => b.Id != blog.Id && (b.Author == blog.Author) )
                .Take(4)
                .ToListAsync();
            var viewModel = new BlogDetailViewModel
            {
                Blog = blog,
                RelatedBlogs = relateBlog
            };
            return View(viewModel);

        }
    }
}
