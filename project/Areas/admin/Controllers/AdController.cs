using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Npgsql;
using project.Models;

namespace project.Areas.admin.Controllers
{
    public class AdController : Controller
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        public ActionResult adlogin()
        { 
            return View();
        }

        [HttpPost]
        public ActionResult adlogin(string manv, string mk)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var query = "SELECT * FROM public.nhanvien WHERE manv = @manv AND matkhau = @mk";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@manv", int.Parse(manv));
                    command.Parameters.AddWithValue("@mk", mk);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var nhanVien = new Employee
                            {
                                manv = Convert.ToInt32(reader["manv"]),
                                tennv = reader["tennv"].ToString(),
                                mapq = reader["mapq"] != DBNull.Value ? (int)reader["mapq"] : (int?)null
                            };

                            // Lưu thông tin nhân viên vào session
                            Session["ad"] = nhanVien;

                            // Kiểm tra mã quyền và phân quyền
                            if (nhanVien.mapq.HasValue)
                            {
                                Session["UserRole"] = nhanVien.mapq.Value;

                                // Phân quyền dựa trên mã quyền (Ví dụ: mã quyền 1 cho nhân viên kho, mã quyền 2 cho nhân viên quản lý đơn hàng)
                                if (nhanVien.mapq == 1)
                                {
                                    return RedirectToAction("Index", "QLSP", new { area = "admin" }); // Redirect nhân viên kho
                                }
                                else if (nhanVien.mapq == 2)
                                {
                                    return RedirectToAction("Index", "Dashboard", new { area = "admin" }); // Redirect nhân viên quản lý đơn hàng
                                } 
                                else if (nhanVien.mapq == 3)
                                {
                                    return RedirectToAction("Index", "Dashboard", new { area = "admin" }); // Redirect nhân viên quản lý đơn hàng
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", "Nhân viên không có quyền hạn.");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
                        }
                    }
                }
            }

            return View();
        }



        public ActionResult Logout()
        {
            Session.Clear(); 
            return RedirectToAction("adlogin", "Ad");
        }
    }
}
