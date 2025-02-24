using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class Category
    {
        [Display(Name = "Mã danh mục")]
        public int madm { get; set; }

        [Display(Name = "Tên danh mục")]
        public string ten_danh_muc { get; set; }
    }
}
