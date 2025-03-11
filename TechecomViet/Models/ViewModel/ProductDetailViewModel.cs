namespace TechecomViet.Models.ViewModel
{
    public class ProductDetailViewModel
    {
        public ProductModel Product { get; set; }
        public List<ReviewModel> Reviews { get; set; }
        public UserModel Users { get; set; }
        public bool CheckReview { get; set; }
    }
}
