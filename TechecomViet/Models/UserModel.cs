using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechecomViet.Models
{
    public class UserModel : IdentityUser
    {
        public string? FullName { get; set; } = "";
        public int Status { get; set; } = 1;
        public string Address { get; set; } = "";
        public string Image { get; set; } = string.Empty;
        [NotMapped]
        public IFormFile? ImageUpload { get; set; }
    }
}
