namespace project.Models
{
    public class Customer
    {
        public int makh { get; set; }
        public string tenkh { get; set; }
        public string sdt { get; set; }
        public string email { get; set; }
        public string hinhanh { get; set; }
        public string diachi { get; set; }
        public string matkhau { get; set; }
        public string gioitinh { get; set; }
        public string namsinh { get; set; }
        public string anhthudo { get; set; }

        public string facebook_id { get; set; }
        public string google_id { get; set; } 


        // New properties for address
        public string ward_code { get; set; }
        public string district_code { get; set; }
        public string province_code { get; set; }
        public string WardName { get; set; } // Tên xã
        public string DistrictName { get; set; } // Tên huyện
        public string ProvinceName { get; set; }
    }

}
