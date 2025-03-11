using System.ComponentModel.DataAnnotations.Schema;

namespace TechecomViet.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public int ShippingPrice { get; set; }
        public string? AddressDetails { get; set; }
        public string? CouponCode { get; set; }
        public int DiscountPercentage { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalPrices { get; set; }
        public int Status { get; set; }
        public List<OrderItemsModel> OrderItems { get; set; } = new List<OrderItemsModel>();
        public string PaymentMethod {  get; set; }
    }

}
