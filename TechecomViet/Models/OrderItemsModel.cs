using System.ComponentModel.DataAnnotations.Schema;

namespace TechecomViet.Models
{
    public class OrderItemsModel
    {
        public int Id { get; set; }
        [ForeignKey("OrderId")]
        public int OrderId { get; set; }
        public string Image { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public OrderModel Order { get; set; }
    }
}
