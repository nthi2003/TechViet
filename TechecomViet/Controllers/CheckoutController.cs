using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechecomViet.Models;
using TechecomViet.Reponsitory;
using TechecomViet.Services.Vnpay;

namespace TechecomViet.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        private readonly IVnPayService _vnPayService;
        private readonly UserManager<UserModel> _userManager;

        public CheckoutController(DataContext dataContext, UserManager<UserModel> userManager, IEmailSender emailSender, IVnPayService vnPayService)
        {
            _dataContext = dataContext;
            _emailSender = emailSender;
            _vnPayService = vnPayService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(string PaymentMethod, string PaymentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _dataContext.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return BadRequest("Giỏ hàng của bạn đang trống.");
            }

            var couponCode = Request.Cookies["CouponTitle"];
            var discountPercentage = 0;

            if (couponCode != null)
            {
                var parts = couponCode.Split(new[] { "||" }, StringSplitOptions.None);
                if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int parsedDiscountPercentage))
                {
                    discountPercentage = parsedDiscountPercentage;
                }
            }

            var shippingPrice = 0;
            if (int.TryParse(Request.Cookies["ShippingPrice"], out int parsedShippingPrice))
            {
                shippingPrice = parsedShippingPrice;
            }

            var totalPrice = cart.TotalAmount;
            if (discountPercentage > 0)
            {
                var discountAmount = totalPrice * discountPercentage / 100;
                totalPrice -= discountAmount;
            }
            totalPrice += shippingPrice;

            var newOrder = new OrderModel
            {
                OrderCode = Guid.NewGuid().ToString("N").Substring(0, 8),
                UserId = userId,
                UserName = userName,
                Status = 1,
                Email = email,
                CreatedDate = DateTime.Now,
                TotalPrices = totalPrice,
                ShippingPrice = shippingPrice,
                CouponCode = couponCode ?? "",
                DiscountPercentage = discountPercentage,
                OrderItems = new List<OrderItemsModel>(),
                AddressDetails = user.Address
            };

            foreach (var item in cart.Items)
            {
                var product = await _dataContext.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity = product.Quantity - item.Quantity;
                    product.SoldOut = product.SoldOut + item.Quantity;
                    _dataContext.Products.Update(product);
                }

                newOrder.OrderItems.Add(new OrderItemsModel
                {
                    Image = item.Product.Images[0],
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }
            if (PaymentMethod == "VnPay")
            {
                newOrder.PaymentMethod = "Vnpay" + PaymentId;
                newOrder.Status = 6;
            }

            else newOrder.PaymentMethod = "COD";

            _dataContext.Orders.Add(newOrder);
            await _dataContext.SaveChangesAsync();

            _dataContext.Carts.Remove(cart);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Đặt hàng thành công!";
            return RedirectToAction("Index", "Cart");
        }
        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpayAsync()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response.VnPayResponseCode == "00")
            {
                var newVnpayInsert = new VpayModel
                {
                    OrderId = response.OrderId,
                    PaymentMethod = response.PaymentMethod,
                    OrderDescription = response.OrderDescription,
                    TransactionId = response.TransactionId,
                    PaymentId = response.PaymentId,
                    DateCreated = DateTime.Now
                };
                _dataContext.Add(newVnpayInsert);
                await _dataContext.SaveChangesAsync();
                var PaymentMethod = response.PaymentMethod;
                var PaymentId = response.PaymentId;
                await Checkout(PaymentMethod, PaymentId);
            }
            else
            {
                TempData["success"] = "Giao dịch Vnpay thành công.";
                return RedirectToAction("Index", "Cart");
            }

            return View(response);
        }

    }
}
