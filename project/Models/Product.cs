using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class Product
    {
        [Display(Name = "Mã sản phẩm")]
        public int masp { get; set; }

        [Display(Name = "Tên sản phẩm")]
        public string tensp { get; set; }

        [Display(Name = "Mô tả")]
        public string mota { get; set; }

        [Display(Name = "Hình ảnh")]
        public string hinhanh { get; set; }

        [Display(Name = "Giá")]
        public Nullable<decimal> gia { get; set; }

        [Display(Name = "Số lượng")]
        public int soluong { get; set; }

        [Display(Name = "Đã bán")]
        public int daban { get; set; }

        [Display(Name = "Mã danh mục")]
        public int? madm { get; set; }

        [Display(Name = "Danh mục")]
        public Category Category { get; set; }

        [Display(Name = "Khuyến mãi")]
        public Promotion Promotion { get; set; }
    }
}
