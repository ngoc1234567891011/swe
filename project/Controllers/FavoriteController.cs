using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using Npgsql;
using project.Models;

namespace project.Controllers
{
    public class FavoriteController : Controller
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        [HttpPost]
        public JsonResult Add(int masp)
        {
            try
            {
                var favoriteList = Session["FavItem"] as List<int> ?? new List<int>();
                if (!favoriteList.Contains(masp))
                {
                    favoriteList.Add(masp);
                    Session["FavItem"] = favoriteList;
                }
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public JsonResult Remove(int masp)
        {
            try
            {
                var favoriteList = Session["FavItem"] as List<int>;
                if (favoriteList != null && favoriteList.Contains(masp))
                {
                    favoriteList.Remove(masp);
                    Session["FavItem"] = favoriteList;
                }
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        public ActionResult Show()
        {
            var favoriteList = Session["FavItem"] as List<int> ?? new List<int>();
            var products = new List<Product>(); // Danh sách sản phẩm sẽ trả về View

            if (favoriteList.Count > 0)
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                SELECT p.masp, p.tensp, p.gia, p.hinhanh, p.mota, 
                       c.madm, c.ten_danh_muc,
                       km.ten_khuyen_mai, km.dieu_kien, km.thoi_gianbd, km.thoi_giankt, km.trang_thai
                FROM san_pham p
                JOIN danh_muc c ON p.madm = c.madm
                LEFT JOIN khuyen_mai km ON p.masp = km.masp
                WHERE p.masp = ANY(@FavoriteList)
            ";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        // Chuyển danh sách yêu thích sang mảng cho truy vấn
                        command.Parameters.AddWithValue("FavoriteList", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, favoriteList);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var product = new Product
                                {
                                    masp = reader.GetInt32(reader.GetOrdinal("masp")),
                                    tensp = reader.GetString(reader.GetOrdinal("tensp")),
                                    gia = reader.GetDecimal(reader.GetOrdinal("gia")),
                                    hinhanh = reader.GetString(reader.GetOrdinal("hinhanh")),
                                    mota = reader.GetString(reader.GetOrdinal("mota")),
                                    Category = new Category
                                    {
                                        madm = reader.GetInt32(reader.GetOrdinal("madm")),
                                        ten_danh_muc = reader.GetString(reader.GetOrdinal("ten_danh_muc"))
                                    },
                                    Promotion = new Promotion
                                    {
                                        ten_khuyen_mai = reader.IsDBNull(reader.GetOrdinal("ten_khuyen_mai")) ? null : reader.GetString(reader.GetOrdinal("ten_khuyen_mai")),
                                        dieu_kien = reader.IsDBNull(reader.GetOrdinal("dieu_kien")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("dieu_kien")),
                                        thoi_gianbd = reader.IsDBNull(reader.GetOrdinal("thoi_gianbd")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("thoi_gianbd")),
                                        thoi_giankt = reader.IsDBNull(reader.GetOrdinal("thoi_giankt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("thoi_giankt")),
                                        trang_thai = reader.IsDBNull(reader.GetOrdinal("trang_thai")) ? null : reader.GetString(reader.GetOrdinal("trang_thai"))
                                    }
                                };

                                products.Add(product);
                            }
                        }
                    }
                }
            }

            return View(products); // Trả danh sách sản phẩm cho View
        }

    }
}
