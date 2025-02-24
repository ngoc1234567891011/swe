using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace project.Models
{
        public class Order
        {
            public int Madh { get; set; } 
            public DateTime Ngay { get; set; } 
            public int TongTien { get; set; } 
            public string TrangThai { get; set; } 
            public string GhiChu { get; set; } 
            public string TenKhachHang { get; set; } 
            public string Diachi { get; set; }

            public int? Makh { get; set; } 
            public int OrderId { get; internal set; }
        public List<Product> Products { get; set; }

    }

}