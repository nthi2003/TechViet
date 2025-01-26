﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TechecomViet.Models
{
    public class ProductQuantityModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu không bỏ trống số lượng sản phẩm")]
        public int Quantity { get; set; }


        public int ProductId { get; set; }

        public DateTime DateCreated { get; set; }


        [ForeignKey("ProductId")]

        public ProductModel? Product { get; set; }
    }
}