using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TechecomViet.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tên danh mục")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập mô tả danh mục")]
        public string Description { get; set; }
        public int? Status { get; set; }
        [Column("Image")]
        public string Image { get; set; } = string.Empty;
        [NotMapped]
        public IFormFile? ImageUpload { get; set; }
    }
}
