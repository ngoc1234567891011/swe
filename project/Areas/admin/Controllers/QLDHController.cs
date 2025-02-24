using Npgsql;
using project.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace project.Areas.admin.Controllers
{
    [CheckRole(3)]
    public class QLDHController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        public ActionResult Index()
        {
            List<Order> orders = new List<Order>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM don_hang", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new Order
                            {
                                Madh = reader.GetInt32(reader.GetOrdinal("madh")),
                                Ngay = reader.GetDateTime(reader.GetOrdinal("ngay")),
                                TongTien = reader.GetInt32(reader.GetOrdinal("tong_tien")),
                                TrangThai = reader.GetString(reader.GetOrdinal("trang_thai")),
                                GhiChu = reader.IsDBNull(reader.GetOrdinal("ghi_chu")) ? "" : reader.GetString(reader.GetOrdinal("ghi_chu")),
                                Makh = reader.IsDBNull(reader.GetOrdinal("makh")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("makh")),
                                Diachi = reader.IsDBNull(reader.GetOrdinal("diachi")) ? "" : reader.GetString(reader.GetOrdinal("diachi"))
                            });
                        }
                    }
                }
            }
            ViewBag.PendingCount = GetPendingOrderCount();

            return View(orders);
        }
        private int GetPendingOrderCount()
        {
            int count = 0;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM don_hang WHERE trang_thai = 'Đơn mới'", connection))
                {
                    count = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return count;
        }
        public ActionResult Details(int id)
        {
            List<OrderDetail> orderDetails = new List<OrderDetail>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "SELECT * FROM chitiethoadon WHERE madh = @id", connection))
                {
                    command.Parameters.AddWithValue("id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orderDetails.Add(new OrderDetail
                            {
                                Madh = reader.GetInt32(reader.GetOrdinal("madh")),
                                Masp = reader.GetInt32(reader.GetOrdinal("masp")),
                                SoLuong = reader.GetInt32(reader.GetOrdinal("soluong")),
                                Gia = reader.GetInt32(reader.GetOrdinal("gia"))
                            });
                        }
                    }
                }
            }

            return View(orderDetails);
        }
        public ActionResult Thongbao()
        {
            List<Order> pendingOrders = new List<Order>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM don_hang WHERE trang_thai = 'Đơn mới' ORDER BY ngay DESC", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pendingOrders.Add(new Order
                            {
                                Madh = reader.GetInt32(reader.GetOrdinal("madh")),
                                Ngay = reader.GetDateTime(reader.GetOrdinal("ngay")),
                                TongTien = reader.GetInt32(reader.GetOrdinal("tong_tien")),
                                Diachi = reader.GetString(reader.GetOrdinal("diachi")),
                                TrangThai = reader.GetString(reader.GetOrdinal("trang_thai"))
                            });
                        }
                    }
                }
            }

            return View(pendingOrders);
        }
        [HttpGet]
        public JsonResult GetPendingCount()
        {
            int count = GetPendingOrderCount(); // Sử dụng hàm đã viết ở trên
            return Json(new { count = count }, JsonRequestBehavior.AllowGet);
        }


    }
}