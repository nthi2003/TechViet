namespace TechecomViet.Models.ViewModel
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; }
        public int ShippingPrice { get; set; }
        public CouponModel Coupon { get; set; }
        public string AddressDetails { get; set; }
    }
}
