using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Npgsql;
using project.Models; 

namespace project.Areas.Admin.Controllers
{
    [CheckRole(1,3)]
    public class NhapHangController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        // GET: NhapHang/Create
        public ActionResult Create()
        {
            ViewBag.NhaCungCapList = GetNhaCungCap();
            ViewBag.NhanVienList = GetNhanVien();
            ViewBag.SanPhamList = GetSanPham();

            return View();
        }

        // POST: NhapHang/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HoaDonNhapHang hoaDon, List<ChiTietNhapHang> chiTietList)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

     
                    string insertHoadonQuery = "INSERT INTO hoadonnhaphang (manv, mancc, ngay, tongtien) VALUES (@MaNv, @MaNcc, @Ngay, @TongTien) RETURNING mahdnh";
                    int maHdnh;

                    using (var command = new NpgsqlCommand(insertHoadonQuery, connection))
                    {
                        command.Parameters.AddWithValue("@MaNv", hoaDon.manv);
                        command.Parameters.AddWithValue("@MaNcc", hoaDon.mancc);
                        command.Parameters.AddWithValue("@Ngay", hoaDon.ngay);
                        command.Parameters.AddWithValue("@TongTien", hoaDon.tongtien);

                        maHdnh = (int)command.ExecuteScalar(); 
                    }

             
                    foreach (var chiTiet in chiTietList)
                    {
                 
                        string insertChiTietQuery = "INSERT INTO chitietnhaphang (mahdnh, masp, soluong, gia) VALUES (@MaHdnh, @MaSp, @SoLuong, @Gia)";
                        using (var command = new NpgsqlCommand(insertChiTietQuery, connection))
                        {
                            command.Parameters.AddWithValue("@MaHdnh", maHdnh);
                            command.Parameters.AddWithValue("@MaSp", chiTiet.masp);
                            command.Parameters.AddWithValue("@SoLuong", chiTiet.soluong);
                            command.Parameters.AddWithValue("@Gia", chiTiet.gia);

                            command.ExecuteNonQuery();
                        }

      
                        int currentQuantity;
                        string selectQuery = "SELECT soluong FROM san_pham WHERE masp = @MaSp";
                        using (var command = new NpgsqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@MaSp", chiTiet.masp);
                            currentQuantity = (int)command.ExecuteScalar();
                        }
                    }

                }

                TempData["SuccessMessage"] = "Nhập hàng thành công!";
                return RedirectToAction("Index", "QLSP", new { area = "admin" });
            }


            ViewBag.NhaCungCapList = GetNhaCungCap();
            ViewBag.NhanVienList = GetNhanVien();
            ViewBag.SanPhamList = GetSanPham(); 

            return View(hoaDon);
        }

        private List<SelectListItem> GetNhaCungCap()
        {
            var nhaCungCapList = new List<SelectListItem>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT mancc, tenncc FROM nhacungcap";
                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        nhaCungCapList.Add(new SelectListItem
                        {
                            Value = reader["mancc"].ToString(),
                            Text = reader["tenncc"].ToString()
                        });
                    }
                }
            }
            return nhaCungCapList;
        }

        private List<SelectListItem> GetNhanVien()
        {
            var nhanVienList = new List<SelectListItem>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT manv, tennv FROM nhanvien"; 
                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        nhanVienList.Add(new SelectListItem
                        {
                            Value = reader["manv"].ToString(),
                            Text = reader["tennv"].ToString()
                        });
                    }
                }
            }
            return nhanVienList;
        }
        private List<SelectListItem> GetSanPham()
        {
            var sanPhamList = new List<SelectListItem>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT masp, tensp FROM san_pham"; 
                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sanPhamList.Add(new SelectListItem
                        {
                            Value = reader["masp"].ToString(),
                            Text = reader["tensp"].ToString()
                        });
                    }
                }
            }
            return sanPhamList;
        }

    }
}
