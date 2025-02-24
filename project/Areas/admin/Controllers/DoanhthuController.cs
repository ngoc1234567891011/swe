using Npgsql;
using project.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;

namespace project.Areas.admin.Controllers
{
    [CheckRole(3)]
    public class DoanhthuController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        public ActionResult Index(DateTime? startDate, DateTime? endDate, string filterType = "Day")
        {
            DateTime currentDate = DateTime.Now.AddDays(1);

            DateTime startDateValue = startDate ?? currentDate.AddDays(-6).Date;
            DateTime endDateValue = endDate ?? currentDate.Date;

            var doanhThuList = GetDoanhThu(startDateValue, endDateValue);

            var topProducts = GetTopProducts(startDateValue, endDateValue);
            var topCustomers = GetTopCustomers(startDateValue, endDateValue);

            // Tính toán KPI
            var totalRevenue = doanhThuList.Sum(d => d.TongTien);
            var totalOrders = doanhThuList.Count();
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.AverageOrderValue = averageOrderValue;
            ViewBag.TopProducts = topProducts;
            ViewBag.TopCustomers = topCustomers;

            // Truyền giá trị StartDate và EndDate vào ViewBag
            ViewBag.StartDate = startDateValue;
            ViewBag.EndDate = endDateValue;
            ViewBag.FilterType = filterType;

            // Biểu đồ
            if (doanhThuList.Any())
            {
                var labels = doanhThuList.Select(d => d.Ngay.ToString("dd/MM/yyyy")).ToList();
                var values = doanhThuList.Select(d => d.TongTien).ToList();

                ViewBag.BarChart = new Chart(width: 800, height: 400)
                    .AddTitle("Doanh thu theo thời gian")
                    .AddSeries(chartType: "Bar", xValue: labels, yValues: values)
                    .GetBytes("png");

                ViewBag.LineChart = new Chart(width: 800, height: 400)
                    .AddTitle("Xu hướng doanh thu")
                    .AddSeries(chartType: "Line", xValue: labels, yValues: values)
                    .GetBytes("png");
            }
            else
            {
                ViewBag.BarChart = null;
                ViewBag.LineChart = null;
            }

            return View(doanhThuList);
        }



        private List<DoanhThu> GetDoanhThu(DateTime startDate, DateTime endDate)
        {
            var doanhThuList = new List<DoanhThu>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                string query = @"
                    SELECT don_hang.ngay, 
                           SUM(chitiethoadon.soluong) AS SoLuong, 
                           don_hang.tong_tien
                    FROM chitiethoadon
                    JOIN san_pham ON chitiethoadon.masp = san_pham.masp
                    JOIN don_hang ON chitiethoadon.madh = don_hang.madh
                    WHERE don_hang.ngay BETWEEN @StartDate AND @EndDate
                    AND don_hang.trang_thai IN ('Đã xử lý', 'Đang giao hàng') 
                    GROUP BY don_hang.ngay, don_hang.tong_tien
                    ORDER BY don_hang.ngay;
                ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime ngay = reader.GetDateTime(0); 
                            doanhThuList.Add(new DoanhThu
                            {
                                Ngay = ngay,
                                SoLuong = reader.GetInt32(1),
                                TongTien = reader.GetInt32(2)
                            });
                        }
                    }
                }
            }

            return doanhThuList;
        }

        private List<TopProduct> GetTopProducts(DateTime startDate, DateTime endDate)
        {
            var topProducts = new List<TopProduct>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                string query = @"
                    SELECT san_pham.tensp, 
                           SUM(chitiethoadon.soluong) AS TotalQuantity
                    FROM chitiethoadon
                    JOIN san_pham ON chitiethoadon.masp = san_pham.masp
                    JOIN don_hang ON chitiethoadon.madh = don_hang.madh
                    WHERE don_hang.ngay BETWEEN @StartDate AND @EndDate
                    AND don_hang.trang_thai IN ('Đã xử lý') 
                    GROUP BY san_pham.tensp
                    ORDER BY TotalQuantity DESC
                    LIMIT 5;
                ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            topProducts.Add(new TopProduct
                            {
                                ProductName = reader.GetString(0),
                                TotalQuantity = reader.GetInt32(1)
                            });
                        }
                    }
                }
            }

            return topProducts;
        }

        private List<TopCustomer> GetTopCustomers(DateTime startDate, DateTime endDate)
        {
            var topCustomers = new List<TopCustomer>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                string query = @"
                    SELECT khach_hang.tenkh, 
                           SUM(don_hang.tong_tien) AS TotalSpent
                    FROM don_hang
                    JOIN khach_hang ON don_hang.makh = khach_hang.makh
                    WHERE don_hang.ngay BETWEEN @StartDate AND @EndDate
                    AND don_hang.trang_thai IN ('Đã xử lý')
                    GROUP BY khach_hang.tenkh
                    ORDER BY TotalSpent DESC
                    LIMIT 5;
                ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            topCustomers.Add(new TopCustomer
                            {
                                CustomerName = reader.GetString(0),
                                TotalSpent = reader.GetDecimal(1)
                            });
                        }
                    }
                }
            }

            return topCustomers;
        }

    }


    public class TopProduct
    {
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class TopCustomer
    {
        public string CustomerName { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
