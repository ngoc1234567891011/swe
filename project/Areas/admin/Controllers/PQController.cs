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
    public class PQController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        // GET: admin/PhanQuyen
        public ActionResult Index()
        {
            var phanQuyenList = GetPhanQuyenList();
            return View(phanQuyenList);
        }

        // GET: admin/PhanQuyen/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: admin/PhanQuyen/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PhanQuyen phanQuyen)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO phanquyen (mota) VALUES (@Mota)";
                    using (var command = new NpgsqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Mota", phanQuyen.mota);
                        command.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = "Thêm quyền thành công!";
                return RedirectToAction("Index");
            }

            return View(phanQuyen);
        }

        // GET: admin/PhanQuyen/Edit/5
        public ActionResult Edit(int id)
        {
            var phanQuyen = GetPhanQuyenById(id);
            if (phanQuyen == null)
            {
                return HttpNotFound();
            }
            return View(phanQuyen);
        }

        // POST: admin/PhanQuyen/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PhanQuyen phanQuyen)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE phanquyen SET mota = @Mota WHERE mapq = @Mapq";
                    using (var command = new NpgsqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Mapq", phanQuyen.mapq);
                        command.Parameters.AddWithValue("@Mota", phanQuyen.mota);
                        command.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = "Cập nhật quyền thành công!";
                return RedirectToAction("Index");
            }

            return View(phanQuyen);
        }

        // POST: admin/PhanQuyen/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int mapq)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM phanquyen WHERE mapq = @Mapq";
                using (var command = new NpgsqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@Mapq", mapq);
                    command.ExecuteNonQuery();
                }
            }

            TempData["SuccessMessage"] = "Quyền đã được xóa thành công!";
            return RedirectToAction("Index");
        }

        // Helper method to get list of PhanQuyen
        private List<PhanQuyen> GetPhanQuyenList()
        {
            var phanQuyenList = new List<PhanQuyen>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM phanquyen";
                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        phanQuyenList.Add(new PhanQuyen
                        {
                            mapq = reader.GetInt32(reader.GetOrdinal("mapq")),
                            mota = reader.GetString(reader.GetOrdinal("mota"))
                        });
                    }
                }
            }
            return phanQuyenList;
        }

        private PhanQuyen GetPhanQuyenById(int mapq)
        {
            PhanQuyen phanQuyen = null;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM phanquyen WHERE mapq = @Mapq";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Mapq", mapq);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            phanQuyen = new PhanQuyen
                            {
                                mapq = reader.GetInt32(reader.GetOrdinal("mapq")),
                                mota = reader.GetString(reader.GetOrdinal("mota"))
                            };
                        }
                    }
                }
            }
            return phanQuyen;
        }
    }
}
