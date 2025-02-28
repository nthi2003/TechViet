using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechecomViet.Models;
using TechecomViet.Reponsitory;

namespace TechecomViet.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;

        public CheckoutController(DataContext dataContext, IEmailSender emailSender)
        {
            _dataContext = dataContext;
            _emailSender = emailSender;
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);

            if (userId == null)
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
                OrderCode = Guid.NewGuid().ToString(),
                UserId = userId,
                UserName = userName,
                Status = 1,
                CreatedDate = DateTime.Now,
                TotalPrices = totalPrice,
                ShippingPrice = shippingPrice,
                CouponCode = couponCode ?? "",
                DiscountPercentage = discountPercentage,
                OrderItems = new List<OrderItemsModel>()
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

            _dataContext.Orders.Add(newOrder);
            await _dataContext.SaveChangesAsync();

            _dataContext.Carts.Remove(cart);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Đặt hàng thành công!";
            return RedirectToAction("Index", "Cart");
        }

    }
}