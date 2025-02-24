using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace project.Models
{
    public class Promotion
    {
        public int makm { get; set; }
        public int masp { get; set; }
        public string ten_khuyen_mai { get; set; }
        public double? dieu_kien { get; set; }
        public DateTime? thoi_gianbd { get; set; }
        public DateTime? thoi_giankt { get; set; }
        public string trang_thai { get; set; }
    }
}
