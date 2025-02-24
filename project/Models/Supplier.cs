using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class Supplier
    {
        [Key]
        [Display(Name = "Mã nhà cung cấp")] 
        public int Mancc { get; set; }

        [Display(Name = "Tên nhà cung cấp")] 
        public string Tenncc { get; set; }

        [Display(Name = "Địa chỉ")] 
        public string Diachi { get; set; }

        [Display(Name = "Số điện thoại")] 
        public string Sdt { get; set; }
    }
}
