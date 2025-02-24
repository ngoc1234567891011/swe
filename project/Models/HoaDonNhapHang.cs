using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class HoaDonNhapHang
    {
        [Key]
        [Display(Name = "Mã hóa đơn nhập")]
        public int mahdnh { get; set; } // Mã hóa đơn nhập hàng

        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Display(Name = "Mã nhân viên")]
        public int manv { get; set; } 

        [Required(ErrorMessage = "Mã nhà cung cấp không được để trống")]
        [Display(Name = "Mã nhà cung cấp")]
        public int mancc { get; set; } 

        [Required(ErrorMessage = "Ngày nhập hàng không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày nhập")]
        public DateTime ngay { get; set; } 

        [Required(ErrorMessage = "Tổng tiền không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Tổng tiền")]
        public int tongtien { get; set; } 

        [Display(Name = "Chi tiết nhập hàng")]
        public List<ChiTietNhapHang> ChiTietNhapHang { get; set; } = new List<ChiTietNhapHang>();
    }
}
