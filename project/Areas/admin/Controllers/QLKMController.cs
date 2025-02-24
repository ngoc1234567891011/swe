using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Npgsql;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using project.Models;

namespace project.Areas.admin.Controllers
{
    [CheckRole(3)]
    public class QLKMController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        // GET: admin/QLKM
        public ActionResult Index()
        {
            List<Promotion> promotions = new List<Promotion>();
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                string query = "SELECT * FROM khuyen_mai";
                NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
                connection.Open();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    promotions.Add(new Promotion
                    {
                        makm = Convert.ToInt32(reader["makm"]),
                        ten_khuyen_mai = reader["ten_khuyen_mai"].ToString(),
                        masp = Convert.ToInt32(reader["masp"]),
                        dieu_kien = Convert.ToDouble(reader["dieu_kien"]),
                        thoi_gianbd = Convert.ToDateTime(reader["thoi_gianbd"]),
                        thoi_giankt = Convert.ToDateTime(reader["thoi_giankt"]),
                        trang_thai = reader["trang_thai"].ToString()
                    });
                }
            }
            return View(promotions);
        }

        // GET: admin/QLKM/Create
        public ActionResult Create()
        {
            List<Product> products = new List<Product>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                string query = "SELECT masp, tensp FROM san_pham";
                NpgsqlCommand cmd = new NpgsqlCommand(query, connection);

                connection.Open();
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            masp = reader.GetInt32(0),
                            tensp = reader.GetString(1)
                        });
                    }
                }
            }

            // Chuyển List<Product> thành SelectList
            ViewBag.ProductList = new SelectList(products, "masp", "tensp");

            return View();
        }

        // POST: admin/QLKM/Create
        [HttpPost]
        public ActionResult Create(Promotion model)
        {
            List<Product> products = new List<Product>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                string query = "SELECT masp, tensp FROM san_pham";
                NpgsqlCommand cmd = new NpgsqlCommand(query, connection);

                connection.Open();
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            masp = reader.GetInt32(0),
                            tensp = reader.GetString(1)
                        });
                    }
                }
            }

            // Chuyển List<Product> thành SelectList
            ViewBag.ProductList = new SelectList(products, "masp", "tensp");
            if (ModelState.IsValid)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        string query = "INSERT INTO khuyen_mai (ten_khuyen_mai, masp, dieu_kien, thoi_gianbd, thoi_giankt, trang_thai) VALUES (@TenKM, @MaSP, @DieuKien, @ThoiGianBD, @ThoiGianKT, @TrangThai)";
                        NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@TenKM", model.ten_khuyen_mai);
                        cmd.Parameters.AddWithValue("@MaSP", model.masp);
                        cmd.Parameters.AddWithValue("@DieuKien", model.dieu_kien);
                        cmd.Parameters.AddWithValue("@ThoiGianBD", model.thoi_gianbd);
                        cmd.Parameters.AddWithValue("@ThoiGianKT", model.thoi_giankt);
                        cmd.Parameters.AddWithValue("@TrangThai", model.trang_thai);

                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }

                    return RedirectToAction("Index"); 
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu khuyến mãi: " + ex.Message);
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        // GET: admin/QLKM/Edit/{id}
        public ActionResult Edit(int id)
        {
            Promotion promotion = null;
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                string query = "SELECT * FROM khuyen_mai WHERE makm = @makm";
                NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@makm", id);
                connection.Open();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    promotion = new Promotion
                    {
                        makm = Convert.ToInt32(reader["makm"]),
                        ten_khuyen_mai = reader["ten_khuyen_mai"].ToString(),
                        masp = Convert.ToInt32(reader["masp"]),
                        dieu_kien = Convert.ToDouble(reader["dieu_kien"]),
                        thoi_gianbd = Convert.ToDateTime(reader["thoi_gianbd"]),
                        thoi_giankt = Convert.ToDateTime(reader["thoi_giankt"]),
                        trang_thai = reader["trang_thai"].ToString()
                    };
                }
            }
            List<Product> products = new List<Product>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                string query = "SELECT masp, tensp FROM san_pham";
                NpgsqlCommand cmd = new NpgsqlCommand(query, connection);

                connection.Open();
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            masp = reader.GetInt32(0),
                            tensp = reader.GetString(1)
                        });
                    }
                }
            }

            ViewBag.ProductList = new SelectList(products, "masp", "tensp");

            return View(promotion);
        }

        // POST: admin/QLKM/Edit/{id}
        [HttpPost]
        public ActionResult Edit(Promotion model)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    string query = "UPDATE khuyen_mai SET ten_khuyen_mai = @TenKM, masp = @MaSP, dieu_kien = @DieuKien, thoi_gianbd = @ThoiGianBD, thoi_giankt = @ThoiGianKT, trang_thai = @TrangThai WHERE makm = @makm";
                    NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@TenKM", model.ten_khuyen_mai);
                    cmd.Parameters.AddWithValue("@MaSP", model.masp);
                    cmd.Parameters.AddWithValue("@DieuKien", model.dieu_kien);
                    cmd.Parameters.AddWithValue("@ThoiGianBD", model.thoi_gianbd);
                    cmd.Parameters.AddWithValue("@ThoiGianKT", model.thoi_giankt);
                    cmd.Parameters.AddWithValue("@TrangThai", model.trang_thai);
                    cmd.Parameters.AddWithValue("@makm", model.makm);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: admin/QLKM/Delete/{id}
        public ActionResult Delete(int? id)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    string query = "DELETE FROM khuyen_mai WHERE makm = @makm";
                    NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@makm", id);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
