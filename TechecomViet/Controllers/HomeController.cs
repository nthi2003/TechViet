using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechecomViet.Models;
using TechecomViet.Models.ViewModel;
using TechecomViet.Reponsitory;



namespace TechecomViet.Controllers
{
    
    public class HomeController : BaseController
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<UserModel> _userManager;
        public HomeController( DataContext context , UserManager<UserModel> userManager) : base(context)
        {
            _dataContext = context;
            _userManager = userManager;

        }
        public async Task<IActionResult> IndexAsync()
        {
            await SetCartItemCountAsync();
            var categories = _dataContext.Categories.Where(c => c.Status == 1).ToList();
            var brands = _dataContext.Brands.Where(b => b.Status == 1).ToList();
            var products = _dataContext.Products.ToList();
            var model = new HomeViewModel
            {
                Categories = categories,
                Brands = brands,
                Products = products
            };

            return View(model);
        }
        public async Task<IActionResult> Wishlist()
        {
            await SetCartItemCountAsync();
            var wishlist_product = await (from w in _dataContext.Wishlists
                                          join p in _dataContext.Products on w.ProductId equals p.Id
                                          select new { Product = p, Wishlists = w })
                               .ToListAsync();

            return View(wishlist_product);
        }
        [HttpPost]
        public async Task<IActionResult> AddWishlist(int Id, WishlistModel wishlistmodel)
        {
            var user = await _userManager.GetUserAsync(User);

            var existingWishList = await _dataContext.Wishlists.FirstOrDefaultAsync(w => w.ProductId == Id && w.UserId == user.Id);
            if(existingWishList != null)
            {
                return BadRequest(new { success = false, message = "Sản phẩm đã có trong danh sách yêu thích" });
            }
            var wishlistProduct = new WishlistModel
            {
                ProductId = Id,
                UserId = user.Id
            };
            _dataContext.Wishlists.Add(wishlistProduct);
             await _dataContext.SaveChangesAsync();
             return Ok(new { success = true, message = "Thêm sản phẩm yêu thích thành công" });
           

        }
        public async Task<IActionResult> DeleteWishlist(int Id)
        {
            var wishlist = await _dataContext.Wishlists.FindAsync(Id);

            if (wishlist == null)
            {
                TempData["error"] = "Không tìm thấy sản phẩm yêu thích.";
                return RedirectToAction("Index", "Home");
            }

            _dataContext.Wishlists.Remove(wishlist);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Xóa sản phẩm yêu thích thành công";
            return RedirectToAction("Wishlist", "Home");
        }

    }
}
