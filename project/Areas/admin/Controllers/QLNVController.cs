using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Npgsql;
using project.Models;

namespace project.Areas.admin.Controllers
{
    [CheckRole(3)]
    public class QLNVController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        // GET: admin/QLNhanVien
        public ActionResult Index()
        {
            var nhanVienList = GetNhanVienList();
            return View(nhanVienList);
        }

        // GET: admin/QLNV/Create
        public ActionResult Create()
        {
            // Lấy danh sách quyền từ bảng phan_quyen
            List<SelectListItem> roleList = new List<SelectListItem>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                string query = "SELECT mapq, mota FROM phanquyen"; 
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roleList.Add(new SelectListItem
                            {
                                Value = reader.GetInt32(0).ToString(),
                                Text = reader.GetString(1) 
                            });
                        }
                    }
                }
            }

            ViewBag.RoleList = new SelectList(roleList, "Value", "Text");

            return View();
        }


        // POST: admin/QLNhanVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Employee nhanVien)
        {
            // Lấy danh sách quyền từ bảng phan_quyen
            List<SelectListItem> roleList = new List<SelectListItem>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                string query = "SELECT mapq, mota FROM phanquyen";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roleList.Add(new SelectListItem
                            {
                                Value = reader.GetInt32(0).ToString(),
                                Text = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            ViewBag.RoleList = new SelectList(roleList, "Value", "Text");
            if (ModelState.IsValid)
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO nhanvien (tennv, sdt, email, hinhanh, diachi, cccd, tinhtrang, mapq, matkhau) " +
                                         "VALUES (@Tennv, @Sdt, @Email, @Hinhanh, @Diachi, @Cccd, @Tinhtrang, @Mapq, @Matkhau)";
                    using (var command = new NpgsqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Tennv", nhanVien.tennv);
                        command.Parameters.AddWithValue("@Sdt", nhanVien.sdt);
                        command.Parameters.AddWithValue("@Email", nhanVien.email);
                        command.Parameters.AddWithValue("@Hinhanh", nhanVien.hinhanh);
                        command.Parameters.AddWithValue("@Diachi", nhanVien.diachi);
                        command.Parameters.AddWithValue("@Cccd", nhanVien.cccd);
                        command.Parameters.AddWithValue("@Tinhtrang", nhanVien.tinhtrang);
                        command.Parameters.AddWithValue("@Mapq", nhanVien.mapq);
                        command.Parameters.AddWithValue("@Matkhau", nhanVien.matkhau);
                        command.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                return RedirectToAction("Index");
            }

            return View(nhanVien);
        }

        // GET: admin/QLNhanVien/Edit/5
        public ActionResult Edit(int id)
        {
            var nhanVien = GetNhanVienById(id);
            return View(nhanVien);
        }

        // POST: admin/QLNhanVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Employee nhanVien)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE nhanvien SET tennv = @Tennv, sdt = @Sdt, email = @Email, hinhanh = @Hinhanh, " +
                                         "diachi = @Diachi, cccd = @Cccd, tinhtrang = @Tinhtrang, mapq = @Mapq, " +
                                         "matkhau = @Matkhau WHERE manv = @Manv";
                    using (var command = new NpgsqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Manv", nhanVien.manv);
                        command.Parameters.AddWithValue("@Tennv", nhanVien.tennv);
                        command.Parameters.AddWithValue("@Sdt", nhanVien.sdt);
                        command.Parameters.AddWithValue("@Email", nhanVien.email);
                        command.Parameters.AddWithValue("@Hinhanh", nhanVien.hinhanh);
                        command.Parameters.AddWithValue("@Diachi", nhanVien.diachi);
                        command.Parameters.AddWithValue("@Cccd", nhanVien.cccd);
                        command.Parameters.AddWithValue("@Tinhtrang", nhanVien.tinhtrang);
                        command.Parameters.AddWithValue("@Mapq", nhanVien.mapq);
                        command.Parameters.AddWithValue("@Matkhau", nhanVien.matkhau);
                        command.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = "Cập nhật nhân viên thành công!";
                return RedirectToAction("Index");
            }

            return View(nhanVien);
        }

        // POST: admin/QLNhanVien/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? manv)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM nhanvien WHERE manv = @Manv";
                using (var command = new NpgsqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@Manv", manv);
                    command.ExecuteNonQuery();
                }
            }

            TempData["SuccessMessage"] = "Nhân viên đã được xóa thành công!";
            return RedirectToAction("Index");
        }

        private List<Employee> GetNhanVienList()
        {
            var nhanVienList = new List<Employee>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM nhanvien";
                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        nhanVienList.Add(new Employee
                        {
                            manv = reader.GetInt32(reader.GetOrdinal("manv")),
                            tennv = reader.GetString(reader.GetOrdinal("tennv")),
                            sdt = reader.GetString(reader.GetOrdinal("sdt")),
                            email = reader.GetString(reader.GetOrdinal("email")),
                            hinhanh = reader.GetString(reader.GetOrdinal("hinhanh")),
                            diachi = reader.GetString(reader.GetOrdinal("diachi")),
                            cccd = reader.GetString(reader.GetOrdinal("cccd")),
                            tinhtrang = reader.GetString(reader.GetOrdinal("tinhtrang")),
                            mapq = reader.IsDBNull(reader.GetOrdinal("mapq")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("mapq")),
                            matkhau = reader.GetString(reader.GetOrdinal("matkhau"))
                        });
                    }
                }
            }
            return nhanVienList;
        }

        private Employee GetNhanVienById(int manv)
        {
            Employee nhanVien = null;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM nhanvien WHERE manv = @Manv";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Manv", manv);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            nhanVien = new Employee
                            {
                                manv = reader.GetInt32(reader.GetOrdinal("manv")),
                                tennv = reader.GetString(reader.GetOrdinal("tennv")),
                                sdt = reader.GetString(reader.GetOrdinal("sdt")),
                                email = reader.GetString(reader.GetOrdinal("email")),
                                hinhanh = reader.IsDBNull(reader.GetOrdinal("hinhanh")) ? null : reader.GetString(reader.GetOrdinal("hinhanh")),
                                diachi = reader.GetString(reader.GetOrdinal("diachi")),
                                cccd = reader.GetString(reader.GetOrdinal("cccd")),
                                tinhtrang = reader.GetString(reader.GetOrdinal("tinhtrang")),
                                mapq = reader.IsDBNull(reader.GetOrdinal("mapq")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("mapq")),
                                matkhau = reader.GetString(reader.GetOrdinal("matkhau"))
                            };
                        }
                    }
                }
            }
            return nhanVien;
        }

    }
}
