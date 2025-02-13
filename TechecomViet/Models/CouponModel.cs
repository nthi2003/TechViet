using System.ComponentModel.DataAnnotations;

namespace TechecomViet.Models
{
    public class CouponModel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateExpired { get; set; }
        public int Quantity { get; set; }
        public int Status { get; set; }


    }
}
