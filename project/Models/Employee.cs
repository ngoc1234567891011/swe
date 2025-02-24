using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class Employee
    {
        [Key]
        [Display(Name = "Mã nhân viên")]
        public int manv { get; set; } 

        [Required(ErrorMessage = "Tên nhân viên không được để trống")]
        [StringLength(255, ErrorMessage = "Tên nhân viên không được vượt quá 255 ký tự")]
        [Display(Name = "Tên nhân viên")]
        public string tennv { get; set; } 

        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string sdt { get; set; } 

        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [Display(Name = "Email")]
        public string email { get; set; } 

        [Display(Name = "Hình ảnh")]
        public string hinhanh { get; set; } 

        [Display(Name = "Địa chỉ")]
        public string diachi { get; set; } 

        [Display(Name = "CCCD")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "CCCD phải có 12 chữ số")]
        public string cccd { get; set; } 

        [Display(Name = "Tình trạng")]
        public string tinhtrang { get; set; } 

        [Display(Name = "Mã quyền hạn")]
        public int? mapq { get; set; } 

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string matkhau { get; set; }
    }
}
