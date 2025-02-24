using Npgsql;
using project.Models; // Đảm bảo có model san_pham
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace project.Areas.admin.Controllers
{
    [CheckRole(1,3)]  
    public class QLSPController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        // GET: admin/QLSP
        public ActionResult Index()
        {
            List<Product> products = new List<Product>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM san_pham ORDER BY masp ASC";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = new Product
                            {
                                masp = (int)reader["masp"],
                                tensp = reader["tensp"].ToString(),
                                mota = reader["mota"].ToString(),
                                hinhanh = reader["hinhanh"].ToString(),
                                gia = (int)reader["gia"],
                                soluong = (int)reader["soluong"],
                                daban = (int)reader["daban"],
                                madm = reader["madm"] != DBNull.Value ? (int?)reader["madm"] : null
                            };
                            products.Add(product);
                        }
                    }
                }
            }
            return View(products);
        }
        private async Task<byte[]> RemoveBackgroundFromImage(Stream imageStream)
        {
            using (var client = new HttpClient())
            {
                // Thêm API key vào header
                client.DefaultRequestHeaders.Add("X-Api-Key", "vQcP2UnZ6omBjhcpLyBMZsWP");

                var formData = new MultipartFormDataContent();
                formData.Add(new StreamContent(imageStream), "image_file", "image.png");

                var response = await client.PostAsync("https://api.remove.bg/v1.0/removebg", formData);

                if (response.IsSuccessStatusCode)
                {
    
                    var content = await response.Content.ReadAsByteArrayAsync();
                    return content; 
                }
                else
                {
                    throw new Exception("Error removing background from image: " + response.ReasonPhrase);
                }
            }
        }


        public string ResizeImage(string imagePath)
        {
            // Đọc ảnh từ đường dẫn
            using (var image = Image.FromFile(imagePath))
            {
                var resizedImage = new Bitmap(image, new Size(500, 500));

                string fileName = "resized_" + Path.GetFileNameWithoutExtension(imagePath) + ".png";


                string resizedImagePath = Path.Combine(Path.GetDirectoryName(imagePath), fileName);

                resizedImage.Save(resizedImagePath, System.Drawing.Imaging.ImageFormat.Png);

                return fileName;
            }
        }

        // GET: admin/QLSP/Create
        public ActionResult Create()
        {
            List<SelectListItem> categories = new List<SelectListItem>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var query = "SELECT madm, ten_danh_muc FROM danh_muc"; 

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new SelectListItem
                            {
                                Value = reader["madm"].ToString(),
                                Text = reader["ten_danh_muc"].ToString()
                            });
                        }
                    }
                }
            }


            ViewBag.madm = new SelectList(categories, "Value", "Text");

            return View();
        }

        // POST: admin/QLSP/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Product model, HttpPostedFileBase hinhanh)
        {
            if (ModelState.IsValid)
            {
                if (hinhanh != null && hinhanh.ContentLength > 0)
                {
                    var fileName = "product_" + Path.GetFileName(hinhanh.FileName);
                    var imagePath = Path.Combine(Server.MapPath("~/Content/images/"), fileName);

                    // Gửi yêu cầu tới API remove.bg để xoá phông
                    var backgroundRemovedImage = await RemoveBackgroundFromImage(hinhanh.InputStream);

                    // Lưu ảnh đã xóa phông trực tiếp vào thư mục
                    System.IO.File.WriteAllBytes(imagePath, backgroundRemovedImage);

                    var resizedFileName = ResizeImage(imagePath); // Trả về tên file ảnh sau khi resize

                    // Gán đường dẫn của ảnh đã resize vào model
                    model.hinhanh = resizedFileName;
                }

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO san_pham (tensp, mota, hinhanh, gia, soluong, daban, madm) " +
                                   "VALUES (@tensp, @mota, @hinhanh, @gia, @soluong, @daban, @madm)";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@tensp", model.tensp);
                        command.Parameters.AddWithValue("@mota", model.mota);
                        command.Parameters.AddWithValue("@hinhanh", model.hinhanh);
                        command.Parameters.AddWithValue("@gia", model.gia);
                        command.Parameters.AddWithValue("@soluong", model.soluong);
                        command.Parameters.AddWithValue("@daban", model.daban);
                        command.Parameters.AddWithValue("@madm", model.madm);

                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }

            List<SelectListItem> categories = new List<SelectListItem>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT madm, ten_danh_muc FROM danh_muc ORDER BY madm ASC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new SelectListItem
                            {
                                Value = reader["madm"].ToString(),
                                Text = reader["ten_danh_muc"].ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.madm = new SelectList(categories, "Value", "Text");

            return View(model);
        }


        // GET: admin/QLSP/Edit/5
        public ActionResult Edit(int id)
        {
            Product product = null;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM san_pham WHERE masp = @masp";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@masp", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            product = new Product
                            {
                                masp = (int)reader["masp"],
                                tensp = reader["tensp"].ToString(),
                                mota = reader["mota"].ToString(),
                                hinhanh = reader["hinhanh"].ToString(),
                                gia = (int)reader["gia"],
                                soluong = (int)reader["soluong"],
                                madm = reader["madm"] != DBNull.Value ? (int?)reader["madm"] : null
                            };
                        }
                    }
                }
            }

            if (product == null)
            {
                return HttpNotFound();
            }

            List<SelectListItem> categories = new List<SelectListItem>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT madm, ten_danh_muc FROM danh_muc ORDER BY madm ASC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new SelectListItem
                            {
                                Value = reader["madm"].ToString(),
                                Text = reader["ten_danh_muc"].ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.madm = new SelectList(categories, "Value", "Text", product.madm);

            return View(product);
        }

        // POST: admin/QLSP/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Product model, HttpPostedFileBase hinhanh)
        {
            if (ModelState.IsValid)
            {
                string newImagePath = null;

                if (hinhanh != null && hinhanh.ContentLength > 0)
                {
                    var fileName = "product_" + Path.GetFileName(hinhanh.FileName);
                    var imagePath = Path.Combine(Server.MapPath("~/Content/images/"), fileName);

                    // Remove background from the uploaded image
                    var backgroundRemovedImage = await RemoveBackgroundFromImage(hinhanh.InputStream);
                    System.IO.File.WriteAllBytes(imagePath, backgroundRemovedImage);  // Save the background-removed image

                    // Resize the image and get the full path
                    newImagePath = ResizeImage(imagePath);

                    // Update the model with the new image path (full path to resized image)
                    model.hinhanh = newImagePath;
                }

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE san_pham SET tensp = @tensp, mota = @mota, hinhanh = @hinhanh, gia = @gia, soluong = @soluong, daban = @daban, madm = @madm WHERE masp = @masp";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@masp", model.masp);
                        command.Parameters.AddWithValue("@tensp", model.tensp);
                        command.Parameters.AddWithValue("@mota", model.mota);
                        command.Parameters.AddWithValue("@hinhanh", model.hinhanh);  // Save the full image path to the database
                        command.Parameters.AddWithValue("@gia", model.gia);
                        command.Parameters.AddWithValue("@soluong", model.soluong);
                        command.Parameters.AddWithValue("@madm", model.madm ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }

            List<SelectListItem> categories = new List<SelectListItem>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var query = "SELECT madm, ten_danh_muc FROM danh_muc";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new SelectListItem
                            {
                                Value = reader["madm"].ToString(),
                                Text = reader["ten_danh_muc"].ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.madm = new SelectList(categories, "Value", "Text", model.madm);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int masp)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();


                    string checkQuery = @"
                SELECT 
                    (SELECT COUNT(*) FROM chitiethoadon WHERE masp = @masp) AS OrderDetailCount,
                    (SELECT COUNT(*) FROM chitietnhaphang WHERE masp = @masp) AS ImportDetailCount,
                    (SELECT COUNT(*) FROM giohang WHERE masp = @masp) AS CartCount,
                    (SELECT COUNT(*) FROM phanhoi WHERE masp = @masp) AS FeedbackCount,
                    (SELECT COUNT(*) FROM khuyen_mai WHERE masp = @masp) AS PromotionCount";

                    using (var checkCommand = new NpgsqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@masp", masp);
                        using (var reader = checkCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int orderDetailCount = reader.GetInt32(0);
                                int importDetailCount = reader.GetInt32(1);
                                int cartCount = reader.GetInt32(2);
                                int feedbackCount = reader.GetInt32(3);
                                int promotionCount = reader.GetInt32(4);

                                if (orderDetailCount > 0 || importDetailCount > 0 || cartCount > 0 || feedbackCount > 0 || promotionCount > 0)
                                {
                                    TempData["ErrorMessage"] = "Không thể xóa sản phẩm này";
                                    return RedirectToAction("Index");
                                }
                            }
                        }
                    }


                    string deleteQuery = "DELETE FROM san_pham WHERE masp = @masp";
                    using (var deleteCommand = new NpgsqlCommand(deleteQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@masp", masp);
                        deleteCommand.ExecuteNonQuery();
                    }

                    TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }




    }
}
