using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Npgsql;
using PagedList;
using project.Models;
using ImageMagick;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace project.Controllers
{
    public class SanPhamController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        public ActionResult Index(int? page, string categoryId, string searchString)
        {
            if (page == null) page = 1;

            List<Product> products = new List<Product>();

            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();

                // Khởi tạo câu truy vấn SQL
                string query = @"
        SELECT p.*, c.ten_danh_muc, km.ten_khuyen_mai, km.dieu_kien, km.thoi_gianbd, km.thoi_giankt, km.trang_thai
        FROM san_pham p 
        JOIN danh_muc c ON p.madm = c.madm 
        LEFT JOIN khuyen_mai km ON p.masp = km.masp";

                // Kiểm tra điều kiện để thêm vào câu truy vấn
                if (!string.IsNullOrEmpty(categoryId) || !string.IsNullOrEmpty(searchString))
                {
                    query += " WHERE";

                    if (!string.IsNullOrEmpty(categoryId))
                    {
                        query += " p.madm = @categoryId";
                    }

                    if (!string.IsNullOrEmpty(searchString))
                    {
                        query += !string.IsNullOrEmpty(categoryId) ? " AND" : "";
                        query += " p.tensp ILIKE @searchString";
                    }
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                {
                    // Thêm tham số nếu có
                    if (!string.IsNullOrEmpty(categoryId))
                    {
                        cmd.Parameters.AddWithValue("@categoryId", int.Parse(categoryId));
                    }

                    if (!string.IsNullOrEmpty(searchString))
                    {
                        cmd.Parameters.AddWithValue("@searchString", "%" + searchString + "%");
                        ViewBag.Keyword = searchString;
                    }
                    else
                    {
                        ViewBag.Keyword = "";
                    }

                    // Đọc dữ liệu từ cơ sở dữ liệu
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Product product = new Product
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
                                    thoi_gianbd = reader.IsDBNull(reader.GetOrdinal("thoi_gianbd")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("thoi_gianbd")),
                                    thoi_giankt = reader.IsDBNull(reader.GetOrdinal("thoi_giankt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("thoi_giankt")),
                                    trang_thai = reader.IsDBNull(reader.GetOrdinal("trang_thai")) ? null : reader.GetString(reader.GetOrdinal("trang_thai"))
                                }
                            };
                            products.Add(product);
                        }
                    }
                }
            }

            // Phân trang
            int pageSize = 9;
            int pageNumber = (page ?? 1);

            var pagedProducts = products.OrderBy(p => p.masp).ToPagedList(pageNumber, pageSize);

            // Gán giá trị cho ViewBag để sử dụng ở View
            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryName = !string.IsNullOrEmpty(categoryId)
                ? products.FirstOrDefault()?.Category.ten_danh_muc
                : "Tất cả sản phẩm";

            return View(pagedProducts);
        }


        public ActionResult Details(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product;
            List<Feedback> comments = new List<Feedback>();

            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();
                NpgsqlCommand cmd = new NpgsqlCommand("SELECT masp, tensp, gia, hinhanh, mota, soluong, daban FROM san_pham WHERE masp = @id", con);
                cmd.Parameters.AddWithValue("@id", id);
                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    product = new Product
                    {
                        masp = reader.GetInt32(0),
                        tensp = reader.GetString(1),
                        gia = reader.GetDecimal(2),
                        hinhanh = reader.GetString(3),
                        mota = reader.GetString(reader.GetOrdinal("mota")),
                        soluong = reader.GetInt32(reader.GetOrdinal("soluong")),
                        daban = reader.GetInt32(reader.GetOrdinal("daban")),
                    };
                }
                else
                {
                    return HttpNotFound();
                }

                reader.Close();

                cmd = new NpgsqlCommand("SELECT c.maph, c.masp, c.makh, c.binhluan, u.tenkh, c.ngay FROM phanhoi c JOIN khach_hang u ON c.makh = u.makh WHERE c.masp = @id", con);
                cmd.Parameters.AddWithValue("@id", id);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Feedback comment = new Feedback
                    {
                        maph = reader.GetInt32(0),
                        masp = reader.GetInt32(1),
                        makh = reader.GetInt32(2),
                        binhluan = reader.GetString(3),
                        tenkh = reader.GetString(4),
                        ngay = reader.GetDateTime(5)
                    };
                    comments.Add(comment);
                }
            }

            ViewBag.Comments = comments;

            return View(product);
        }

        [HttpPost]
        public ActionResult Comment(int id, FormCollection f)
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("login", "Dangnhap");
            }

            Customer user = (Customer)Session["user"];
            Feedback comment = new Feedback
            {
                makh = user.makh,
                masp = id,
                ngay = System.DateTime.Now,
                binhluan = f["comment"]
            };

            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();
                NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO phanhoi (masp, makh, binhluan, ngay) VALUES (@masp, @makh, @binhluan, @ngay)", con);
                cmd.Parameters.AddWithValue("@masp", comment.masp);
                cmd.Parameters.AddWithValue("@makh", comment.makh);
                cmd.Parameters.AddWithValue("@binhluan", comment.binhluan);
                cmd.Parameters.AddWithValue("@ngay", comment.ngay);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Details", new { id = id });
        }
        [HttpPost]
        public async Task<ActionResult> ThuDo(HttpPostedFileBase imageUpload, int masp)
        {
            if (imageUpload == null || imageUpload.ContentLength == 0)
            {
                return Json(new { success = false, message = "Không có hình ảnh nào được tải lên." });
            }

            string category = GetCategoryFromProduct(masp);

            string categoryForAPI = "";

            if (category.Contains("váy") || category.Contains("đầm"))
            {
                categoryForAPI = "Dress";
            }
            else if (category.Contains("áo"))
            {
                categoryForAPI = "Upper body";
            }
            else if (category.Contains("quần"))
            {
                categoryForAPI = "Lower body";
            }
            else
            {
                categoryForAPI = "Upper body";  
            }

            string fileName = Path.GetFileName(imageUpload.FileName);
            string modelImagePath = Path.Combine(Server.MapPath("~/Content/tryon"), fileName);

            try
            {
                imageUpload.SaveAs(modelImagePath);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi lưu hình ảnh: " + ex.Message });
            }

            string apiKey = "vQcP2UnZ6omBjhcpLyBMZsWP";
            string noBgImagePath = Path.Combine(Server.MapPath("~/Content/tryon"), "no-bg-" + fileName);
            try
            {
                using (var client = new HttpClient())
                using (var formData = new MultipartFormDataContent())
                {
                    formData.Headers.Add("X-Api-Key", apiKey);
                    formData.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(modelImagePath)), "image_file", fileName);
                    formData.Add(new StringContent("auto"), "size");

                    var response = await client.PostAsync("https://api.remove.bg/v1.0/removebg", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        using (var fileStream = new FileStream(noBgImagePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Lỗi khi xóa nền ảnh: " + response.Content.ReadAsStringAsync().Result });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi kết nối đến API Remove.bg: " + ex.Message });
            }

            string clothImagePath = GetClothImagePath(masp);
            if (string.IsNullOrEmpty(clothImagePath))
            {
                return Json(new { success = false, message = "Không tìm thấy hình ảnh quần áo." });
            }

            string resultImageUrl;
            try
            {
                resultImageUrl = await ProcessTryOn(noBgImagePath, clothImagePath, categoryForAPI);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
                return Json(new { success = false, message = "Lỗi khi kết nối đến API thử đồ: " + ex.Message });
            }

            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    var customerId = ((Customer)Session["user"]).makh;
                    string updateQuery = "UPDATE khach_hang SET anhthudo = @anhthudo WHERE makh = @makh";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@anhthudo", "no-bg-" + fileName);
                        cmd.Parameters.AddWithValue("@makh", customerId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật dữ liệu khách hàng: " + ex.Message });
            }

            string tryOnImagePath = Path.Combine(Server.MapPath("~/Content/tryon"), "tryon-" + fileName);
            try
            {
                using (var client = new HttpClient())
                using (var formData = new MultipartFormDataContent())
                {
                    formData.Headers.Add("X-Api-Key", "vQcP2UnZ6omBjhcpLyBMZsWP");  
                    formData.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(resultImageUrl)), "image_file", "tryon-" + fileName);
                    formData.Add(new StringContent("auto"), "size");

                    var response = await client.PostAsync("https://api.remove.bg/v1.0/removebg", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        using (var fileStream = new FileStream(tryOnImagePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Lỗi khi xóa nền ảnh thử đồ: " + response.Content.ReadAsStringAsync().Result });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi kết nối đến API Remove.bg lần 2: " + ex.Message });
            }

            return Json(new { success = true, message = "Thử đồ thành công", imageUrl = "/Content/tryon/" + "tryon-" + fileName });
        }
    


        private async Task<string> ProcessTryOn(string modelImagePath, string clothImagePath, string category)
        {
            string apiKey = "SG_a074a84c54938f44";
            string url = "https://api.segmind.com/v1/try-on-diffusion";

            try
            {
                string modelImageB64 = Convert.ToBase64String(System.IO.File.ReadAllBytes(modelImagePath));
                string clothImageB64 = Convert.ToBase64String(System.IO.File.ReadAllBytes(clothImagePath));

                var data = new
                {
                    model_image = modelImageB64,
                    cloth_image = clothImageB64,
                    category = category,  
                    num_inference_steps = 35,
                    guidance_scale = 5,
                    seed = 2935092812,
                    base64 = false 
                };

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
                    var jsonData = JsonConvert.SerializeObject(data);
                    var content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(url, content);
                    var responseData = await response.Content.ReadAsStringAsync();

                

                    if (response.Content.Headers.ContentType.MediaType == "application/json")
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            dynamic responseJson = JsonConvert.DeserializeObject(responseData);
                            return responseJson?.image_url ?? string.Empty;
                        }
                        else
                        {
                            throw new Exception($"API Error: {response.StatusCode} - {responseData}");
                        }
                    }
                    else if (response.Content.Headers.ContentType.MediaType == "image/jpeg")
                    {
                        string fileName = $"output_image_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                        string outputFilePath = Path.Combine(Server.MapPath("~/Content/tryon"), fileName);
                        using (var imageStream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                            {
                                await imageStream.CopyToAsync(fileStream);
                            }
                        }

                        return outputFilePath; 
                    }
                    else
                    {
                        throw new Exception($"Unexpected content type: {response.Content.Headers.ContentType.MediaType}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi kết nối đến API thử đồ: " + ex.Message);
            }
        }

        private string GetCategoryFromProduct(int masp)
        {
            string category = string.Empty;

            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();
                // Lấy tên danh mục từ bảng danh_muc theo madanhmuc của sản phẩm
                string query = "SELECT dm.ten_danh_muc FROM san_pham sp JOIN danh_muc dm ON sp.madm = dm.madm WHERE sp.masp = @masp";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@masp", masp);
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        category = result.ToString().ToLower(); 
                    }
                }
            }

            return category;
        }



        private string GetClothImagePath(int masp)
        {
            string clothImagePath = null;

            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT hinhanh FROM san_pham WHERE masp = @masp";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@masp", masp);
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        string fileName = result.ToString();
                        clothImagePath = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                        if (!System.IO.File.Exists(clothImagePath))
                        {
                            throw new Exception("Hình ảnh quần áo không tồn tại tại: " + clothImagePath);
                        }
                    }
                }
            }

            return clothImagePath;
        }


    }

}