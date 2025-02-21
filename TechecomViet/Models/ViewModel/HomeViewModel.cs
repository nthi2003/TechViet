namespace TechecomViet.Models.ViewModel
{
    public class HomeViewModel
    {
        public IEnumerable<CategoryModel> Categories { get; set; }
        public IEnumerable<BrandModel> Brands { get; set; }
        public IEnumerable<ProductModel> Products { get; set; }
    }
}
