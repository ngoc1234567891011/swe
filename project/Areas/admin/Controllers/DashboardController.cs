using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using Npgsql;
using project.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;

namespace project.Areas.admin.Controllers
{
    [CheckRole(2, 3)]
    public class DashboardController : Controller
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        public async Task<ActionResult> Index(string status = null)
        {
            List<Order> orders = new List<Order>();
            string query = "SELECT * FROM don_hang";

            if (!string.IsNullOrEmpty(status))
            {
                query += " WHERE trang_thai = @status";
            }

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        command.Parameters.AddWithValue("@status", status);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var order = new Order
                            {
                                Madh = reader.GetInt32(reader.GetOrdinal("madh")),
                                Ngay = reader.GetDateTime(reader.GetOrdinal("ngay")),
                                TongTien = reader.GetInt32(reader.GetOrdinal("tong_tien")),
                                TrangThai = reader.GetString(reader.GetOrdinal("trang_thai")),
                                GhiChu = reader.IsDBNull(reader.GetOrdinal("ghi_chu")) ? "" : reader.GetString(reader.GetOrdinal("ghi_chu")),
                                Diachi = reader.IsDBNull(reader.GetOrdinal("diachi")) ? "" : reader.GetString(reader.GetOrdinal("diachi")),
                            };

                            string productQuery = @"
                                            SELECT c.masp, s.tensp, c.soluong, c.gia, s.hinhanh
                                            FROM chitiethoadon c
                                            INNER JOIN san_pham s ON c.masp = s.masp
                                            WHERE c.madh = @madh";
                            using (var productConnection = new NpgsqlConnection(connectionString))
                            {
                                await productConnection.OpenAsync();
                                using (var productCommand = new NpgsqlCommand(productQuery, productConnection))
                                {
                                    productCommand.Parameters.AddWithValue("@madh", order.Madh);
                                    using (var productReader = await productCommand.ExecuteReaderAsync())
                                    {
                                        var products = new List<Product>();
                                        while (await productReader.ReadAsync())
                                        {
                                            products.Add(new Product
                                            {
                                                masp = productReader.GetInt32(productReader.GetOrdinal("masp")),
                                                tensp = productReader.GetString(productReader.GetOrdinal("tensp")),
                                                soluong = productReader.GetInt32(productReader.GetOrdinal("soluong")),
                                                gia = productReader.GetInt32(productReader.GetOrdinal("gia")),
                                                hinhanh = productReader.GetString(productReader.GetOrdinal("hinhanh")),
                                            });
                                        }
                                        order.Products = products;
                                    }
                                }
                            }

                            orders.Add(order);
                        }
                    }
                }
            }

            ViewBag.Status = status;
            return View(orders);
        }
        private List<Order> GetOrdersByStatus(string status)
        {
            List<Order> orders = new List<Order>();
            string query = "SELECT * FROM don_hang";

            if (!string.IsNullOrEmpty(status))
            {
                query += " WHERE trang_thai = @status";
            }

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        command.Parameters.AddWithValue("@status", status);
                    }

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
                                Diachi = reader.IsDBNull(reader.GetOrdinal("diachi")) ? "" : reader.GetString(reader.GetOrdinal("diachi")),
                            });
                        }
                    }
                }
            }

            return orders;
        }

        private Dictionary<string, int> GetOrderStatusCounts()
        {
            var counts = new Dictionary<string, int>
            {
                { "Chờ xác nhận", 0 },
                { "Chờ lấy hàng", 0 },
                { "Đã xử lý", 0 },
                { "Đơn hủy", 0 }
            };

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(
                    "SELECT trang_thai, COUNT(*) AS count FROM don_hang GROUP BY trang_thai", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var status = reader["trang_thai"].ToString();
                            var count = Convert.ToInt32(reader["count"]);

                            if (counts.ContainsKey(status))
                            {
                                counts[status] = count;
                            }
                        }
                    }
                }
            }

            return counts;
        }
        [HttpPost]
        public ActionResult UpdateOrderStatus(int madh, string newStatus)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    var query = "UPDATE don_hang SET trang_thai = @newStatus WHERE madh = @madh";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@newStatus", newStatus);
                        command.Parameters.AddWithValue("@madh", madh);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Order not found or status not changed." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult GetOrderCounts()
        {
            var orderCounts = GetOrderStatusCounts();

            return Json(orderCounts, JsonRequestBehavior.AllowGet);
        }
        public ActionResult INHOADON(int orderId)
        {
            var order = GetOrderById(orderId); 
            if (order == null)
            {
                return HttpNotFound("Không tìm thấy đơn hàng!");
            }

            MemoryStream stream = new MemoryStream();
            PdfWriter writer = new PdfWriter(stream);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

            // Sử dụng font hỗ trợ Unicode cho tiếng Việt
            string fontPath = "C:\\Windows\\Fonts\\arial.ttf"; // Đường dẫn đến font Arial Unicode MS
            PdfFont vietnameseFont = PdfFontFactory.CreateFont(fontPath, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

            // Thêm nội dung hóa đơn với phông chữ tiếng Việt
            document.Add(new Paragraph("HÓA ĐƠN BÁN HÀNG")
                .SetFont(vietnameseFont).SetFontSize(16));

            document.Add(new Paragraph($"Mã đơn hàng: {order.Madh}")
                .SetFont(vietnameseFont));
            document.Add(new Paragraph($"Ngày đặt hàng: {order.Ngay.ToString("dd/MM/yyyy:HH:ss:mm")}")
                .SetFont(vietnameseFont));
            document.Add(new Paragraph($"Khách hàng: {order.TenKhachHang}")
                .SetFont(vietnameseFont));
            document.Add(new Paragraph($"Tổng tiền: {order.TongTien.ToString()} vnđ")
                .SetFont(vietnameseFont));

            document.Add(new Paragraph(" "));
            document.Add(new Paragraph("Danh sách sản phẩm:")
                .SetFont(vietnameseFont).SetBold());

            Table table = new Table(UnitValue.CreatePercentArray(3)).UseAllAvailableWidth();
            table.AddHeaderCell("Tên sản phẩm").SetFont(vietnameseFont);
            table.AddHeaderCell("Số lượng").SetFont(vietnameseFont);
            table.AddHeaderCell("Giá").SetFont(vietnameseFont);

            foreach (var item in order.Products)
            {
                table.AddCell(item.tensp).SetFont(vietnameseFont);
                table.AddCell(item.soluong.ToString()).SetFont(vietnameseFont);
                table.AddCell(item.gia.ToString()).SetFont(vietnameseFont);
            }
            document.Add(table);

            document.Close();

            // Trả file PDF về trình duyệt
            byte[] fileBytes = stream.ToArray();
            stream.Close();

            Response.ContentType = "application/pdf";
            Response.AddHeader("Content-Disposition", $"inline; filename=HoaDon_{order.Madh}.pdf");
            Response.BinaryWrite(fileBytes);
            Response.End();

            return null;
        }

        private Order GetOrderById(int orderId)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT dh.madh, dh.ngay, kh.tenkh, dh.tong_tien
                    FROM don_hang dh
                    INNER JOIN khach_hang kh ON dh.makh = kh.makh
                    WHERE dh.madh = @Madh";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Madh", orderId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Order
                            {
                                Madh = reader.GetInt32(reader.GetOrdinal("madh")),
                                Ngay = reader.GetDateTime(reader.GetOrdinal("ngay")),
                                TenKhachHang = reader.GetString(reader.GetOrdinal("tenkh")),
                                TongTien = reader.GetInt32(reader.GetOrdinal("tong_tien")),
                                Products = GetOrderProducts(orderId)
                            };
                        }
                    }
                }
            }
            return null;
        }

        private List<Product> GetOrderProducts(int orderId)
        {
            var products = new List<Product>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT sp.tensp, ctdh.soluong, ctdh.gia
                    FROM chitiethoadon ctdh
                    INNER JOIN san_pham sp ON ctdh.masp = sp.masp
                    WHERE ctdh.madh = @Madh";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Madh", orderId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                tensp = reader.GetString(reader.GetOrdinal("tensp")),
                                soluong = reader.GetInt32(reader.GetOrdinal("soluong")),
                                gia = reader.GetDecimal(reader.GetOrdinal("gia"))
                            });
                        }
                    }
                }
            }
            return products;
        }
    }
}
