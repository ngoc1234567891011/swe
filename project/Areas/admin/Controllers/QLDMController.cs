using Npgsql;
using project.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Net;
using System.Web.Mvc;

namespace project.Areas.Admin.Controllers
{
    [CheckRole(3)]
    public class QLDMController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        public ActionResult Index()
        {
            List<Category> categories = new List<Category>();

            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT madm, ten_danh_muc FROM danh_muc ORDER BY madm ASC";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Category category = new Category
                            {
                                madm = reader.GetInt32(reader.GetOrdinal("madm")),
                                ten_danh_muc = reader["ten_danh_muc"].ToString()
                            };
                            categories.Add(category);
                        }
                    }
                }
            }

            return View(categories);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add([Bind(Include = "ten_danh_muc")] Category r)
        {
            if (ModelState.IsValid)
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    string query = "INSERT INTO danh_muc (ten_danh_muc) VALUES (@category_name)";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@category_name", r.ten_danh_muc);
                        cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            return View(r);
        }


        public ActionResult Edit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Category category = null;

            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT * FROM danh_muc WHERE madm = @madm";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@madm", id);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            category = new Category
                            {
                                madm = reader.GetInt32(reader.GetOrdinal("madm")),
                                ten_danh_muc = reader["ten_danh_muc"].ToString()
                            };
                        }
                    }
                }
            }

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "madm, ten_danh_muc")] Category r)
        {
            if (ModelState.IsValid)
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    string query = "UPDATE danh_muc SET ten_danh_muc = @ten_danh_muc WHERE madm = @madm";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@madm", r.madm);
                        cmd.Parameters.AddWithValue("@ten_danh_muc", r.ten_danh_muc);
                        cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            return View(r);
        }

        public ActionResult Delete(int? id)
        {
            if (id == 0) // Kiểm tra nếu id không hợp lệ
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    string query = "DELETE FROM danh_muc WHERE madm = @madm";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@madm", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (PostgresException ex) 
            {
                if (ex.SqlState == "23503")
                {
                    TempData["ErrorMessage"] = "Không thể xóa danh mục vì nó đang được sử dụng.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa danh mục. Vui lòng thử lại.";
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi không xác định. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "Xóa danh mục thành công.";
            return RedirectToAction("Index");
        }

    }
}
