using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechecomViet.Reponsitory;

namespace TechecomViet.Areas.Admin.Controllers
{
    public class SendCoupon : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        public SendCoupon(DataContext dataContext, IEmailSender emailSender)
        {
            _dataContext = dataContext;
            _emailSender = emailSender;
        }
        //public async Task<IActionResult> SendEmailToUsersByTotalSpent(int lowerThreshold, int upperThreshold )
        //{
        //    if (lowerThreshold >= upperThreshold)
        //    {
        //        return BadRequest("Ngưỡng dưới phải nhỏ hơn ngưỡng trên.");
        //    }

        //    var usersInRange = await _dataContext.Orders
        //        .GroupBy(o => o.UserId) 
        //        .Select(g => new
        //        {
        //            UserId = g.Key, 
        //            TotalSpent = g.Sum(o => o.TotalPrices), 
        //            UserEmail = g.First().Users.Email 
        //        })
        //        .Where(u => u.TotalSpent > lowerThreshold && u.TotalSpent < upperThreshold) // Lọc theo ngưỡng
        //        .ToListAsync();

        //    // Gửi email cho từng người dùng
        //    foreach (var user in usersInRange)
        //    {
        //        var receiver = user.UserEmail;
        //        var subject = "Cảm ơn bạn đã ủng hộ!";
        //        var message = $"Chào bạn, cảm ơn bạn đã ủng hộ cửa hàng với tổng giá trị đơn hàng {user.TotalSpent.ToString("#,##0 VNĐ")}. " +
        //                      "Chúng tôi có một số ưu đãi đặc biệt dành riêng cho bạn!";

        //        await _emailSender.SendEmailAsync(receiver, subject, message);
        //    }

        //    // Trả về thông báo thành công
        //    return Ok($"Đã gửi email thành công cho {usersInRange.Count} người dùng.");
        //}
    }
}
