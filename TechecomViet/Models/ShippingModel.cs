using System.ComponentModel.DataAnnotations;

namespace TechecomViet.Models
{
    public class ShippingModel
    {
        public int Id { get; set; }
        public int Price { get; set; }

        [Required(ErrorMessage = "Yêu cầu chọn tên xã phường")]
        public string Ward { get; set; }
        [Required(ErrorMessage = "Yêu cầu chọn quận huyện")]
        public string District { get; set; }
        [Required(ErrorMessage = "Yêu cầu chọn thành phố")]
        public string City { get; set; }
    }
}
