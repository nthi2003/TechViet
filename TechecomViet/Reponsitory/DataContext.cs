﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechecomViet.Models;

namespace TechecomViet.Reponsitory
{
    public class DataContext : IdentityDbContext<UserModel>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<BrandModel> Brands { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<ProductQuantityModel> ProductQuantities { get; set; }
        public DbSet<CouponModel> Coupons { get; set; }
        public DbSet<ShippingModel> Shippings { get; set; }
        public DbSet<SliderModel> Sliders { get; set; }
        public DbSet<WishlistModel> Wishlists { get; set; }
        public DbSet<CartItemModel> CartItem { get; set; }
        public DbSet<CartModel> Carts { get; set; }
    }
    
}
