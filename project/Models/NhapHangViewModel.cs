using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace project.Models
{
    public class NhapHangViewModel
{
    public HoaDonNhapHang HoaDonNhap { get; set; }
    public List<ChiTietNhapHang> ChiTietNhapHangs { get; set; }
    public Product SanPham { get; set; }
    public SelectList Suppliers { get; set; } // Danh sách nhà cung cấp
    public SelectList Employees { get; set; } // Danh sách nhân viên
}

}