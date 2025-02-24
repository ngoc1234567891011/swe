using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace project.Models
{
    public class ChiTietNhapHang
    {
        public int masp { get; set; }
        public string tensp { get; set; }

        public int mahdnh { get; set; }
        public int soluong { get; set; }
        public DateTime ngay { get; set; }
        public int gia { get; set; }
    }
}