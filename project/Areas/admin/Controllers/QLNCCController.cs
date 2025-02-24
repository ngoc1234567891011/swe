using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient; // Bạn cần đổi thành Npgsql nếu sử dụng PostgreSQL
using System.Web.Mvc;
using Npgsql;
using project.Models;

namespace project.Areas.admin.Controllers
{
    [CheckRole(3)]
    public class QLNCCController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        // GET: admin/QLNCC
        public ActionResult Index()
        {
            var nhaCungCapList = new List<Supplier>();
            using (var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM nhacungcap";
                using (var command = new Npgsql.NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        nhaCungCapList.Add(new Supplier
                        {
                            Mancc = reader.GetInt32(reader.GetOrdinal("mancc")),
                            Tenncc = reader.GetString(reader.GetOrdinal("tenncc")),
                            Diachi = reader.IsDBNull(reader.GetOrdinal("diachi")) ? null : reader.GetString(reader.GetOrdinal("diachi")),
                            Sdt = reader.IsDBNull(reader.GetOrdinal("sdt")) ? null : reader.GetString(reader.GetOrdinal("sdt"))
                        });
                    }
                }
            }
            return View(nhaCungCapList);
        }

        // GET: admin/QLNCC/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: admin/QLNCC/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Supplier nhaCungCap)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new Npgsql.NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO nhacungcap (tenncc, diachi, sdt) VALUES (@Tenncc, @Diachi, @Sdt)";
                    using (var command = new Npgsql.NpgsqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Tenncc", nhaCungCap.Tenncc);
                        command.Parameters.AddWithValue("@Diachi", nhaCungCap.Diachi ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Sdt", nhaCungCap.Sdt ?? (object)DBNull.Value);
                        command.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = "Thêm nhà cung cấp thành công!";
                return RedirectToAction("Index");
            }

            return View(nhaCungCap);
        }

        // GET: admin/QLNCC/Edit/5
        public ActionResult Edit(int id)
        {
            Supplier nhaCungCap = null;
            using (var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM nhacungcap WHERE mancc = @Mancc";
                using (var command = new Npgsql.NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Mancc", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            nhaCungCap = new Supplier
                            {
                                Mancc = reader.GetInt32(reader.GetOrdinal("mancc")),
                                Tenncc = reader.GetString(reader.GetOrdinal("tenncc")),
                                Diachi = reader.IsDBNull(reader.GetOrdinal("diachi")) ? null : reader.GetString(reader.GetOrdinal("diachi")),
                                Sdt = reader.IsDBNull(reader.GetOrdinal("sdt")) ? null : reader.GetString(reader.GetOrdinal("sdt"))
                            };
                        }
                    }
                }
            }
            return View(nhaCungCap);
        }

        // POST: admin/QLNCC/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Supplier nhaCungCap)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new Npgsql.NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE nhacungcap SET tenncc = @Tenncc, diachi = @Diachi, sdt = @Sdt WHERE mancc = @Mancc";
                    using (var command = new Npgsql.NpgsqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Mancc", nhaCungCap.Mancc);
                        command.Parameters.AddWithValue("@Tenncc", nhaCungCap.Tenncc);
                        command.Parameters.AddWithValue("@Diachi", nhaCungCap.Diachi ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Sdt", nhaCungCap.Sdt ?? (object)DBNull.Value);
                        command.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = "Cập nhật nhà cung cấp thành công!";
                return RedirectToAction("Index");
            }

            return View(nhaCungCap);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int mancc)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string checkQuery = "SELECT COUNT(*) FROM hoadonnhaphang WHERE mancc = @Mancc";
                using (var checkCommand = new NpgsqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@Mancc", mancc);
                    int productCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (productCount > 0)
                    {

                        TempData["ErrorMessage"] = "Không thể xóa nhà cung cấp";
                        return RedirectToAction("Index");
                    }
                }
                string deleteQuery = "DELETE FROM nhacungcap WHERE mancc = @Mancc";
                using (var deleteCommand = new NpgsqlCommand(deleteQuery, connection))
                {
                    deleteCommand.Parameters.AddWithValue("@Mancc", mancc);
                    deleteCommand.ExecuteNonQuery();
                }
            }

            TempData["SuccessMessage"] = "Nhà cung cấp đã được xóa thành công!";
            return RedirectToAction("Index");
        }

        private List<Supplier> GetNhaCungCapList()
        {
            var nhaCungCapList = new List<Supplier>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM nhacungcap";
                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        nhaCungCapList.Add(new Supplier
                        {
                            Mancc = reader.GetInt32(reader.GetOrdinal("mancc")),
                            Tenncc = reader.GetString(reader.GetOrdinal("tenncc")),
                            Diachi = reader.GetString(reader.GetOrdinal("diachi")),
                            Sdt = reader.GetString(reader.GetOrdinal("sdt"))
                        });
                    }
                }
            }
            return nhaCungCapList;
        }

    }
}
