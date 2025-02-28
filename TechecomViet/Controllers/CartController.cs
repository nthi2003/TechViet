using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechecomViet.Models;
using TechecomViet.Models.ViewModel;
using TechecomViet.Reponsitory;

namespace TechecomViet.Controllers
{
    public class CartController : BaseController
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<UserModel> _userManager;
        public CartController(DataContext context, UserManager<UserModel> userManager) : base(context)
        {
            _dataContext = context;
            _userManager = userManager;

        }
        public async Task<IActionResult> Index()
        {
            await SetCartItemCountAsync();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _dataContext.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var cartItems = cart?.Items.Select(i => new CartItemModel
            {
                Id = i.Id,
                Product = i.Product,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList() ?? new List<CartItemModel>();

            var viewModel = new CartItemViewModel
            {
                CartItems = cartItems,
              
            };

            return View(viewModel);
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { message = "Bạn chưa đăng nhập" });
            }

            var cart = await _dataContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new CartModel
                {
                    UserId = user.Id,
                    Items = new List<CartItemModel>()
                };
                _dataContext.Carts.Add(cart);
                await _dataContext.SaveChangesAsync();
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                var product = await _dataContext.Products.FindAsync(productId);
                if (product == null)
                {
                    return NotFound(new { message = "Sản phẩm không tồn tại" });
                }

                cartItem = new CartItemModel
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = product.Price
                };

                cart.Items.Add(cartItem);
            }

            await _dataContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Thêm vào giỏ hàng thành công!" });
        }
        [Route("DeleteCartItem")]
        public async Task<IActionResult> DeleteCartItem(int Id)
        {
            var cartItem = await _dataContext.CartItem.FindAsync(Id);
            if(cartItem == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại trong giỏ hàng" });
            }
            _dataContext.CartItem.Remove(cartItem);
            await _dataContext.SaveChangesAsync();
            TempData["Message"] = "Đã xóa sản phẩm thành công";
            return RedirectToAction("Index", "Cart");
        }
        [Route("ClearCart")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            var cart = await _dataContext.Carts
               .Include(c => c.Items)
               .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                return NotFound(new { message = "Giỏ hàng đã trống" });
            }
            _dataContext.CartItem.RemoveRange(cart.Items);
            await _dataContext.SaveChangesAsync();
            TempData["Message"] = "Đã xóa toàn bộ giỏ hàng!";
            return RedirectToAction("Index", "Cart"); 
        }
        [HttpPost]
        [Route("AddCoupon")]
        public async Task<IActionResult> AddCoupon(CouponModel couponModel, string coupon_value)
        {
            var coupon = await _dataContext.Coupons.FirstOrDefaultAsync(x => x.Name == coupon_value);

            if (coupon == null)
            {
                TempData["error"] = "Mã giảm giá không tồn tại";
                return RedirectToAction("Index", "Cart");
            }
            if (coupon.Quantity < 1)
            {
                TempData["error"] = "Mã giảm giá đã hết";
                return RedirectToAction("Index", "Cart");
            }
            if (coupon.DateExpired < DateTime.Now)
            {
                TempData["error"] = "Mã giảm giá đã hết hạn";
                return RedirectToAction("Index", "Cart");
            }
            string couponTitle = $"{coupon.Name} | {coupon.Description} || {coupon.DiscountPercentage}";
            TimeSpan remainingTime = coupon.DateExpired - DateTime.Now;
            int daysRemaining = remainingTime.Days;

            try
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true // Bảo mật cookie
                };

                Response.Cookies.Append("CouponTitle", couponTitle, cookieOptions);

                TempData["success"] = $"Mã giảm giá '{coupon.Name}' đã được áp dụng thành công! Còn {daysRemaining} ngày trước khi hết hạn.";
                return RedirectToAction("Index", "Cart");
            }
            catch
            {
                TempData["error"] = "Có lỗi xảy ra khi áp dụng mã giảm giá";
            }

            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        [Route("AddShipping")]
        public async Task<IActionResult> AddShipping(ShippingModel shippingModel, string quan, string tinh, string phuong)
        {
            var shipping = await _dataContext.Shippings
                .FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

            int shippingPrice = shipping != null ? shipping.Price : 50000;

            try
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true
                };
                Response.Cookies.Append("ShippingPrice", shippingPrice.ToString(), cookieOptions);
            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi áp dụng giá giao hàng" });
            }

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Cart") });
        }

        public async Task<IActionResult> Decrease(int Id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["error"] = "Bạn chưa đăng nhập.";
                return RedirectToAction("Index", "Cart");
            }

            var product = await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == Id);
            if (product == null)
            {
                TempData["error"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index", "Cart");
            }

            var cart = await _dataContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || cart.Items == null)
            {
                TempData["error"] = "Giỏ hàng không tồn tại.";
                return RedirectToAction("Index", "Cart");
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == Id);
            if (cartItem == null)
            {
                TempData["error"] = "Sản phẩm không tồn tại trong giỏ hàng.";
                return RedirectToAction("Index", "Cart");
            }

            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
                TempData["success"] = "Giảm số lượng sản phẩm thành công!";
            }
            else
            {
                cart.Items.Remove(cartItem);
                TempData["success"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
            }

            await _dataContext.SaveChangesAsync();
            return RedirectToAction("Index", "Cart");
        }


        public async Task<IActionResult> Increase(int Id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["error"] = "Bạn chưa đăng nhập.";
                return RedirectToAction("Index", "Cart");
            }

            var product = await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == Id);
            if (product == null)
            {
                TempData["error"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index", "Cart");
            }

            var cart = await _dataContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || cart.Items == null)
            {
                TempData["error"] = "Giỏ hàng không tồn tại.";
                return RedirectToAction("Index", "Cart");
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == Id);
            if (cartItem == null)
            {
                TempData["error"] = "Sản phẩm không tồn tại trong giỏ hàng.";
                return RedirectToAction("Index", "Cart");
            }

            if (cartItem.Quantity < product.Quantity)
            {
                cartItem.Quantity++;
                TempData["success"] = "Tăng số lượng sản phẩm thành công!";
            }
            else
            {
                TempData["error"] = "Số lượng tối đa đã đạt!";
            }

            await _dataContext.SaveChangesAsync();
            return RedirectToAction("Index", "Cart");
        }


    }
}