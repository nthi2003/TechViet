using System.ComponentModel.DataAnnotations;

namespace TechecomViet.Models.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage="Yêu cầu nhập email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập mật khẩu")]
        public string Password { get; set; }

    }
}
