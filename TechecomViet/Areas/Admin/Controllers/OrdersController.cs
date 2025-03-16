using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechecomViet.Reponsitory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace TechecomViet.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class OrdersController : Controller
    {
        private readonly DataContext _dataContext;  
        public OrdersController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index(int? page , int? status , string? orderCode)
        {
            var find = _dataContext.Orders.AsQueryable();
            if (status.HasValue)
            {
                find = find.Where(o => o.Status == status.Value);
            }
            if(orderCode != null)
            {
                find = find.Where(b => b.OrderCode.Contains(orderCode));
            }
            int totalItems = await find.CountAsync();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int skip = (pageNumber - 1) * pageSize;
            var orders = await find
                                          .OrderByDescending(o => o.Id)
                                          .Skip(skip)
                                          .Take(pageSize)
                                          .ToListAsync();
            ViewBag.TotalItems = totalItems;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.SelectedStatus = status;
            return View(orders);
        }
        [HttpGet("OrderDetail/{Id}")]
        public async Task<IActionResult> OrderDetail(int Id)
        {
            var orderItems = await _dataContext.OrderItems
             .Where(o => o.OrderId == Id)
             .ToListAsync();

            return View(orderItems);
        }
        [HttpPost]
        [Route("UpdateOrder/{Id}")]
        public async Task<IActionResult> UpdateOrderAdmin(int Id, int Status)
        {
            var checkOrder = await _dataContext.Orders.FirstOrDefaultAsync(o => o.Id == Id);
            if (checkOrder == null)
            {
                TempData["error"] = "Đơn hàng không tồn tại";
                return RedirectToAction("Index");
            }

            checkOrder.Status = Status;
            _dataContext.Update(checkOrder);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Cập nhật đơn hàng thành công";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<ActionResult> DownloadOrderPdf(int orderId)
        {
            // Lấy dữ liệu đơn hàng từ database
            var order = await _dataContext.Orders
                .Include(o => o.OrderItems) // Đảm bảo load dữ liệu OrderItems
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound("Đơn hàng không tồn tại");
            }

            // Tạo tài liệu PDF
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4); // Kích thước trang A4
                    page.Margin(2, Unit.Centimetre); // Lề 2cm
                    page.PageColor(Colors.White); // Màu nền trang
                    page.DefaultTextStyle(x => x.FontSize(14)); // Cỡ chữ mặc định

                    page.Header()
                   .Text("Công ty TNHH TECH VIET\nĐịa chỉ: Thành phố Đà Nẵng\nSố điện thoại: 097318881")
                   .SemiBold().FontSize(14).AlignCenter();
                    // Content
                    page.Content()
                  .PaddingVertical(1, Unit.Centimetre)
                  .Column(x =>
                  {
                      x.Spacing(10); // Khoảng cách giữa các phần tử
                      x.Item().Text($"Khách hàng: {order.UserName}");
                      x.Item().Text($"Ngày tạo: {order.CreatedDate:dd/MM/yyyy}");
                      x.Item().Text($"Phương thức thanh toán: {order.PaymentMethod}");
                      x.Item().Text($"Địa chỉ giao hàng: {order.AddressDetails}");

                      // Bảng chi tiết sản phẩm
                      x.Item().Table(table =>
                      {
                          table.ColumnsDefinition(columns =>
                          {
                              columns.RelativeColumn(); // STT
                              columns.RelativeColumn(3); // Tên sản phẩm
                              columns.RelativeColumn(); // Số lượng
                              columns.RelativeColumn(); // Đơn giá
                              columns.RelativeColumn(); // Thành tiền
                          });

                          // Tiêu đề bảng
                          table.Header(header =>
                          {
                              header.Cell().Text("STT").Bold();
                              header.Cell().Text("Tên sản phẩm").Bold();
                              header.Cell().Text("Số lượng").Bold();
                              header.Cell().Text("Đơn giá").Bold();
                              header.Cell().Text("Thành tiền").Bold();
                          });

                          // Dữ liệu bảng (sử dụng dữ liệu từ OrderItems)
                          for (int i = 0; i < order.OrderItems.Count; i++)
                          {
                              var item = order.OrderItems[i];
                              table.Cell().Text((i + 1).ToString());
                              table.Cell().Text(item.ProductName); // Tên sản phẩm
                              table.Cell().Text(item.Quantity.ToString()); // Số lượng
                              table.Cell().Text(item.Price.ToString("#,##0 VNĐ")); // Đơn giá
                              table.Cell().Text((item.Quantity * item.Price).ToString("#,##0 VNĐ")); // Thành tiền
                          }
                      });

                      // Tính tổng tiền sau khi áp dụng giảm giá
                      var totalPrice = order.OrderItems.Sum(item => item.Quantity * item.Price);


                      // Hiển thị tổng tiền và giảm giá
                      x.Item().AlignRight().Text($"Tổng tiền: {totalPrice:#,##0 VNĐ}").Bold();
                      x.Item().AlignRight().Text($"Giảm giá: {order.DiscountPercentage}%").Bold();
                      x.Item().AlignRight().Text($"Thành tiền: {order.TotalPrices.ToString("#,##0 VNĐ")}").Bold();
                  });


                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Trang ");
                            x.CurrentPageNumber();
                        });
                });
            });

   
            var stream = new MemoryStream();
            document.GeneratePdf(stream);

            stream.Position = 0;
            return File(stream, "application/pdf", $"HoaDon_{order.OrderCode}.pdf");
        }
    }
}
