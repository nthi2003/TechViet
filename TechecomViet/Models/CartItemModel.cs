using System.ComponentModel.DataAnnotations.Schema;

namespace TechecomViet.Models
{
    public class CartItemModel
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        [NotMapped]
        public int TotalPrice => Quantity * Price;
        [ForeignKey("CartId")]
        public CartModel Cart { get; set; }
        public ProductModel Product { get; set; }
    }
}
