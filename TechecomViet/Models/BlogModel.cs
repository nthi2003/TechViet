using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechecomViet.Models
{
    public class BlogModel
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Image { get; set; } = string.Empty;
        [NotMapped]
        public IFormFile? ImageUpload { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
