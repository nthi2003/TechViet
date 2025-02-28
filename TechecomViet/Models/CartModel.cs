using System.ComponentModel.DataAnnotations;

namespace TechecomViet.Models
{
    public class CartModel
    {
        [Key]
        public int Id { get; set; } 
        public string UserId { get; set; }
        public List<CartItemModel> Items { get; set; } = new List<CartItemModel>();
        public int TotalAmount => Items.Sum(i => i.TotalPrice);
    }
}
