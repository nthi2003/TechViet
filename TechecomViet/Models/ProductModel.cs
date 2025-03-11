using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechecomViet.Validate;

namespace TechecomViet.Models
{
    public class ProductModel
    {
        [Key]
        public int Id { get; set; }

        [RequiredField]
        public string Name { get; set; }

        [RequiredField]
        public string Description { get; set; }

        [RequiredField]

        public int Price { get; set; }
        public int Quantity { get; set; }
        public int SoldOut { get; set; }
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public CategoryModel Category { get; set; }
        public BrandModel Brand { get; set; }
        public int DiscountPercentage { get; set; }

        public List<string> Images { get; set; } = new List<string>();

        [NotMapped]
        public List<IFormFile>? ImageUploads { get; set; }
    }
}
