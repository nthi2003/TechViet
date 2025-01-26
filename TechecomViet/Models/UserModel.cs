using Microsoft.AspNetCore.Identity;

namespace TechecomViet.Models
{
    public class UserModel : IdentityUser
    {
        public string FullName { get; set; }
    }
}
