using System.ComponentModel.DataAnnotations;

namespace TechecomViet.Models.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Yêu cầu nhập email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập mật khẩu")]
        public string Password { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
    }
}
