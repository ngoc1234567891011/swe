    using Npgsql;
using System;
using System.Configuration;
using System.Web.Mvc;
using project.Models;
using System.Collections.Generic;
using System.Linq;

namespace project.Controllers
{
    public class ShoppingCartController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        // GET: ShoppingCart
        public Cart GetCart()
        {
            Cart cart = Session["Cart"] as Cart;
            if (cart == null || Session["Cart"] == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }
            return cart;
        }

        [HttpPost]
        public ActionResult AddToCart(int id, int quantity)
        {
            try
            {
                if (Session["user"] == null)
                {
                    return Json(new { success = false, message = "Bạn phải đăng nhập." });
                }

                Cart cart = Session["Cart"] as Cart;
                if (cart == null)
                {
                    cart = new Cart();
                    Session["Cart"] = cart;
                }

                Product pro;
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(@"
                SELECT san_pham.*, 
                       khuyen_mai.makm,
                       khuyen_mai.ten_khuyen_mai,
                       khuyen_mai.dieu_kien,
                       khuyen_mai.thoi_gianbd,
                       khuyen_mai.thoi_giankt,
                       khuyen_mai.trang_thai
                FROM san_pham 
                LEFT JOIN khuyen_mai ON san_pham.masp = khuyen_mai.masp 
                WHERE san_pham.masp = @id", connection))
                    {
                        command.Parameters.AddWithValue("id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                pro = new Product
                                {
                                    masp = reader.GetInt32(reader.GetOrdinal("masp")),
                                    tensp = reader.GetString(reader.GetOrdinal("tensp")),
                                    hinhanh = reader.GetString(reader.GetOrdinal("hinhanh")),
                                    gia = reader.GetDecimal(reader.GetOrdinal("gia")),
                                    soluong = reader.GetInt32(reader.GetOrdinal("soluong")),
                                    Promotion = reader.IsDBNull(reader.GetOrdinal("makm")) ? null : new Promotion
                                    {
                                        makm = reader.GetInt32(reader.GetOrdinal("makm")),
                                        ten_khuyen_mai = reader.GetString(reader.GetOrdinal("ten_khuyen_mai")),
                                        dieu_kien = reader.IsDBNull(reader.GetOrdinal("dieu_kien")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("dieu_kien")),
                                        thoi_gianbd = reader.GetDateTime(reader.GetOrdinal("thoi_gianbd")),
                                        thoi_giankt = reader.GetDateTime(reader.GetOrdinal("thoi_giankt")),
                                        trang_thai = reader.GetString(reader.GetOrdinal("trang_thai"))
                                    }
                                };
                            }
                            else
                            {
                                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
                            }
                        }
                    }
                }

                if (pro != null)
                {
                    if (quantity <= pro.soluong && quantity > 0)
                    {
                        decimal finalPrice = (decimal)pro.gia;

                        if (pro.Promotion != null)
                        {
                            var now = DateTime.Now;

                            if (pro.Promotion.thoi_gianbd <= now && pro.Promotion.thoi_giankt >= now)
                            {
                                decimal discountPercentage = pro.Promotion.dieu_kien.HasValue ? (decimal)pro.Promotion.dieu_kien.Value / 100 : 0;

              
                                finalPrice = finalPrice * (1 - discountPercentage);
                            }
                        }


                        cart.Add(pro, quantity, finalPrice); 
                        return Json(new { success = true, message = "Sản phẩm đã được thêm vào giỏ hàng." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Số lượng hàng không đủ." });
                    }
                }

                return Json(new { success = false, message = "Lỗi khi thêm sản phẩm vào giỏ hàng." });
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine("Lỗi khi thêm sản phẩm vào giỏ hàng: " + ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }







        public ActionResult Show()
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("login", "Dangnhap");
            }

            Cart cart = Session["Cart"] as Cart;

            if (cart == null || !cart.Items.Any())
            {
                ViewBag.IsEmpty = true;
            }
            else
            {
                ViewBag.IsEmpty = false;
            }

            return View(cart);
        }




        [HttpPost]
        public ActionResult Update(FormCollection f)
        {
            Cart cart = Session["Cart"] as Cart;
            if (cart == null)
            {
                return RedirectToAction("Show");
            }

            int idsp = int.Parse(f["id"]);
            int sl = int.Parse(f["sl"]);

            Product product;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM san_pham WHERE masp = @id", connection))
                {
                    command.Parameters.AddWithValue("id", idsp);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            product = new Product
                            {
                                masp = reader.GetInt32(reader.GetOrdinal("masp")),
                                tensp = reader.GetString(reader.GetOrdinal("tensp")),
                                hinhanh = reader.GetString(reader.GetOrdinal("hinhanh")),
                                gia = reader.GetDecimal(reader.GetOrdinal("gia")), 
                                soluong = reader.GetInt32(reader.GetOrdinal("soluong"))
                            };
                        }
                        else
                        {
                            return HttpNotFound();
                        }
                    }
                }
            }

            if (product != null)
            {
                if (sl <= product.soluong && sl > 0)
                {
                    cart.Update(idsp, sl);
                    Session["Cart"] = cart; 
                    return RedirectToAction("Show");
                }
                else
                {
                    TempData["error"] = "Không đủ hàng.";
                    return RedirectToAction("Show");
                }
            }

            return HttpNotFound();
        }

        public ActionResult Remove(int id)
        {
            Cart cart = Session["Cart"] as Cart;
            if (cart != null)
            {
                cart.Remove(id);
                Session["Cart"] = cart; 
            }
            return RedirectToAction("Show");
        }

        public ActionResult AddToCartAndCheckout(int productId, int quantity)
        {
            try
            {
                if (Session["user"] == null)
                {
                    return Json(new { success = false, message = "Bạn phải đăng nhập." });
                }

                Cart cart = Session["Cart"] as Cart;
                if (cart == null)
                {
                    cart = new Cart();
                    Session["Cart"] = cart;
                }

                Product pro;
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(@"
                SELECT san_pham.*, 
                       khuyen_mai.makm,
                       khuyen_mai.ten_khuyen_mai,
                       khuyen_mai.dieu_kien,
                       khuyen_mai.thoi_gianbd,
                       khuyen_mai.thoi_giankt,
                       khuyen_mai.trang_thai
                FROM san_pham 
                LEFT JOIN khuyen_mai ON san_pham.masp = khuyen_mai.masp 
                WHERE san_pham.masp = @id", connection))
                    {
                        command.Parameters.AddWithValue("id", productId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                pro = new Product
                                {
                                    masp = reader.GetInt32(reader.GetOrdinal("masp")),
                                    tensp = reader.GetString(reader.GetOrdinal("tensp")),
                                    hinhanh = reader.GetString(reader.GetOrdinal("hinhanh")),
                                    gia = reader.GetDecimal(reader.GetOrdinal("gia")),
                                    soluong = reader.GetInt32(reader.GetOrdinal("soluong")),
                                    Promotion = reader.IsDBNull(reader.GetOrdinal("makm")) ? null : new Promotion
                                    {
                                        makm = reader.GetInt32(reader.GetOrdinal("makm")),
                                        ten_khuyen_mai = reader.GetString(reader.GetOrdinal("ten_khuyen_mai")),
                                        dieu_kien = reader.IsDBNull(reader.GetOrdinal("dieu_kien")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("dieu_kien")),
                                        thoi_gianbd = reader.GetDateTime(reader.GetOrdinal("thoi_gianbd")),
                                        thoi_giankt = reader.GetDateTime(reader.GetOrdinal("thoi_giankt")),
                                        trang_thai = reader.GetString(reader.GetOrdinal("trang_thai"))
                                    }
                                };
                            }
                            else
                            {
                                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
                            }
                        }
                    }
                }

                if (pro != null)
                {
                    if (quantity <= pro.soluong && quantity > 0)
                    {
                        decimal finalPrice = (decimal)pro.gia;

                        if (pro.Promotion != null)
                        {
                            var now = DateTime.Now;

                            if (pro.Promotion.thoi_gianbd <= now && pro.Promotion.thoi_giankt >= now)
                            {
                                decimal discountPercentage = pro.Promotion.dieu_kien.HasValue ? (decimal)pro.Promotion.dieu_kien.Value / 100 : 0;
                                finalPrice = finalPrice * (1 - discountPercentage);
                            }
                        }

                        cart.Add(pro, quantity, finalPrice); // Add product to cart
                        Session["Cart"] = cart;

                        // Redirect to the checkout page
                        return RedirectToAction("ThanhToan", "ShoppingCart");
                    }
                    else
                    {
                        return Json(new { success = false, message = "Số lượng hàng không đủ." });
                    }
                }

                return Json(new { success = false, message = "Lỗi khi thêm sản phẩm vào giỏ hàng." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi thêm sản phẩm vào giỏ hàng: " + ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        public ActionResult ThanhToan()
        {
            Cart cart = Session["Cart"] as Cart;

            if (cart == null)
            {
                return RedirectToAction("Show", "ShoppingCart");
            }

            var productIds = cart.Items.Select(item => item.shopping_sp.masp).Distinct().ToList();

            return View(cart);
        }
        public ActionResult DatHang(FormCollection f)
        {
            var u = (Customer)Session["user"];
            int makh = u.makh;


            // Tạo đối tượng getUser để lấy thông tin người dùng
            getUser userService = new getUser();
            var user = userService.GetById(makh);  // Sử dụng userId để lấy thông tin người dùng từ DB

            if (user == null || string.IsNullOrEmpty(user.tenkh) || string.IsNullOrEmpty(user.sdt) ||
                string.IsNullOrEmpty(user.email) || string.IsNullOrEmpty(user.diachi))
            {
                TempData["Message"] = "Vui lòng cập nhật đầy đủ thông tin cá nhân để tiếp tục thanh toán.";
                return RedirectToAction("ThanhToan");
            }
            string notes = Request.Form["notes"];
            int madh=0;
            Order ddh = new Order();
            Customer kh = (Customer)Session["user"];
            Cart cart = Session["Cart"] as Cart;

            if (cart != null)
            {
                ddh.Ngay = DateTime.Now;
                ddh.TongTien = (int)cart.Total();
                ddh.TrangThai = "Chờ xác nhận";
                ddh.GhiChu = notes;
                ddh.Makh = kh.makh;
                getUser userHelper = new getUser();
                Customer khs = userHelper.GetById(kh.makh);
                string diachi = khs.diachi;
                string wardName = khs.WardName;
                string districtName = khs.DistrictName;
                string provinceName = khs.ProvinceName;
                ddh.Diachi = $"{diachi}, {wardName}, {districtName}, {provinceName}";

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Insert Order and get madh
                            
                            using (var command = new NpgsqlCommand("INSERT INTO don_hang (ngay, tong_tien, trang_thai, ghi_chu, makh, diachi) VALUES (@ngay, @tong_tien, @trang_thai, @ghi_chu, @makh, @diachi) RETURNING madh", connection))
                            {
                                command.Parameters.AddWithValue("ngay", ddh.Ngay);
                                command.Parameters.AddWithValue("tong_tien", ddh.TongTien);
                                command.Parameters.AddWithValue("trang_thai", ddh.TrangThai);
                                command.Parameters.AddWithValue("ghi_chu", ddh.GhiChu ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("makh", ddh.Makh ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("diachi", ddh.Diachi ?? (object)DBNull.Value);

                                madh = (int)command.ExecuteScalar();
                                Session["OrderId"] = madh;
                                if (Session["OrderId"] == null)
                                {
                                    throw new Exception("Không thể lưu OrderId vào Session.");
                                }
                            }

                            // Insert order details and update the 'daban' column for each product
                            foreach (var item in cart.Items)
                            {
                                // Insert order details
                                using (var command = new NpgsqlCommand("INSERT INTO chitiethoadon (madh, masp, soluong, gia) VALUES (@madh, @masp, @soluong, @gia)", connection))
                                {
                                    command.Parameters.AddWithValue("madh", madh);
                                    command.Parameters.AddWithValue("masp", item.shopping_sp.masp);
                                    command.Parameters.AddWithValue("soluong", item.shopping_sl);
                                    command.Parameters.AddWithValue("gia", item.shopping_sp.gia);

                                    command.ExecuteNonQuery();
                                }

                                // Update the 'daban' column to reflect the quantity sold
                                using (var command = new NpgsqlCommand("UPDATE san_pham SET daban = daban + @soluong WHERE masp = @masp", connection))
                                {
                                    command.Parameters.AddWithValue("soluong", item.shopping_sl);
                                    command.Parameters.AddWithValue("masp", item.shopping_sp.masp);

                                    command.ExecuteNonQuery();
                                }
                            }

                            // Insert payment information
                            using (var command = new NpgsqlCommand("INSERT INTO thanhtoan (madh, trangthai, sotien, phuongthuc, thoigian) VALUES (@madh, @trangthai, @sotien, @phuongthuc, @thoigian)", connection))
                            {
                                command.Parameters.AddWithValue("madh", madh);
                                command.Parameters.AddWithValue("trangthai", "Thành công");
                                command.Parameters.AddWithValue("sotien", ddh.TongTien);
                                command.Parameters.AddWithValue("phuongthuc", "Thanh toán khi nhận hàng");
                                command.Parameters.AddWithValue("thoigian", DateTime.Now);

                                command.ExecuteNonQuery();
                            }

                            // Commit transaction
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            // Rollback in case of error
                            transaction.Rollback();
                            TempData["ErrorMessage"] = ex.Message;
                        }
                    }
                }
            }
            return RedirectToAction("Xacnhandonhang", "ShoppingCart", new { orderId = madh });
        }





        public ActionResult XacNhanDonHang(int? orderId)
        {
            Order order = null;
                List<Product> recommendedProducts = new List<Product>();

                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
            SELECT sp.*
            FROM chitiethoadon ctdh
            JOIN san_pham sp ON ctdh.masp = sp.masp
            WHERE ctdh.madh = @orderId";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            order = new Order { Products = new List<Product>() };
                            while (reader.Read())
                            {
                                Product product = new Product
                                {
                                    masp = reader.GetInt32(reader.GetOrdinal("masp")),
                                    tensp = reader.GetString(reader.GetOrdinal("tensp")),
                                    gia = reader.GetDecimal(reader.GetOrdinal("gia")),
                                    mota = reader.GetString(reader.GetOrdinal("mota")),
                                    hinhanh = reader.IsDBNull(reader.GetOrdinal("hinhanh")) ? null : reader.GetString(reader.GetOrdinal("hinhanh")),
                                    soluong = reader.GetInt32(reader.GetOrdinal("soluong")),
                                    daban = reader.GetInt32(reader.GetOrdinal("daban")),
                                };
                                order.Products.Add(product);
                            }
                        }
                    }

                    if (order == null || !order.Products.Any())
                    {
                        return HttpNotFound("Đơn hàng không tồn tại hoặc không có sản phẩm.");
                    }

                    foreach (var orderedProduct in order.Products)
                    {
                        recommendedProducts.AddRange(GetRelatedProducts(orderedProduct, con));
                    }

         
                    recommendedProducts = recommendedProducts
                        .GroupBy(p => p.masp)
                        .Select(g => g.First())
                        .ToList();
                }

                ViewBag.RecommendedProducts = recommendedProducts;
            Session["Cart"] = null;

            return View(order);

        }

        private List<Product> GetRelatedProducts(Product targetProduct, NpgsqlConnection con)
        {
            List<Product> relatedProducts = new List<Product>();

            // Thêm điều kiện lọc để giảm tải
            string query = "SELECT * FROM san_pham WHERE masp != @targetId";
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@targetId", targetProduct.masp);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    // Lấy danh sách sản phẩm từ cơ sở dữ liệu
                    List<Product> allProducts = new List<Product>();
                    while (reader.Read())
                    {
                        Product product = new Product
                        {
                            masp = reader.GetInt32(reader.GetOrdinal("masp")),
                            tensp = reader.GetString(reader.GetOrdinal("tensp")),
                            gia = reader.GetDecimal(reader.GetOrdinal("gia")),
                            mota = reader.GetString(reader.GetOrdinal("mota")),
                            hinhanh = reader.IsDBNull(reader.GetOrdinal("hinhanh")) ? null : reader.GetString(reader.GetOrdinal("hinhanh")),
                            soluong = reader.GetInt32(reader.GetOrdinal("soluong")),
                            daban = reader.GetInt32(reader.GetOrdinal("daban")),
                        };

                        allProducts.Add(product);
                    }

                    // Vector hóa mô tả bằng TF-IDF
                    var tfidfVectors = ComputeTFIDF(targetProduct.mota, allProducts.Select(p => p.mota).ToList());

                    // Lấy vector của sản phẩm mục tiêu
                    var targetVector = tfidfVectors[0];

                    // Tính độ tương đồng Cosine
                    for (int i = 1; i < tfidfVectors.Count; i++)
                    {
                        double similarity = CosineSimilarity(targetVector, tfidfVectors[i]);
                        if (similarity > 0.5)
                        {
                            relatedProducts.Add(allProducts[i - 1]); 
                        }
                    }
                }
            }

            return relatedProducts;
        }

        // Tính toán TF-IDF cho tất cả mô tả
        private List<List<double>> ComputeTFIDF(string targetText, List<string> allTexts)
        {
            var allWords = new HashSet<string>();
            var wordCountDocs = new List<Dictionary<string, int>>();

            allTexts.Insert(0, targetText); 
            foreach (var text in allTexts)
            {
                var wordCounts = TokenizeAndNormalize(text);
                wordCountDocs.Add(wordCounts);
                foreach (var word in wordCounts.Keys)
                {
                    allWords.Add(word);
                }
            }

            // Tính TF-IDF
            var tfidfVectors = new List<List<double>>();
            foreach (var wordCounts in wordCountDocs)
            {
                List<double> tfidfVector = new List<double>();
                foreach (var word in allWords)
                {
                    double tf = wordCounts.ContainsKey(word) ? wordCounts[word] / (double)wordCounts.Values.Sum() : 0;
                    double docWithWord = wordCountDocs.Count(d => d.ContainsKey(word));
                    double idf = Math.Log(allTexts.Count / (1.0 + docWithWord));
                    tfidfVector.Add(tf * idf);
                }
                tfidfVectors.Add(tfidfVector);
            }

            return tfidfVectors;
        }

        private Dictionary<string, int> TokenizeAndNormalize(string text)
        {
            var stopWords = new HashSet<string> { "là", "và", "hoặc", "nhưng", "có", "không", "được" }; 
            var wordCounts = new Dictionary<string, int>();
            var words = text.Split(new[] { ' ', '.', ',', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                var normalizedWord = word.ToLower().Trim();
                if (stopWords.Contains(normalizedWord) || normalizedWord.Length < 2) continue;

                if (wordCounts.ContainsKey(normalizedWord))
                    wordCounts[normalizedWord]++;
                else
                    wordCounts[normalizedWord] = 1;
            }

            return wordCounts;
        }

        // Tính độ tương đồng Cosine giữa hai vector
        private double CosineSimilarity(List<double> vector1, List<double> vector2)
        {
            double dotProduct = vector1.Zip(vector2, (x, y) => x * y).Sum();
            double magnitude1 = Math.Sqrt(vector1.Sum(x => x * x));
            double magnitude2 = Math.Sqrt(vector2.Sum(x => x * x));

            return dotProduct / (magnitude1 * magnitude2);
        }


        public ActionResult VnPayPayment()
        {
            var u = (Customer)Session["user"];
            int makh = u.makh;
           

            getUser userService = new getUser();
            var user = userService.GetById(makh);  

            if (user == null || string.IsNullOrEmpty(user.tenkh) || string.IsNullOrEmpty(user.sdt) ||
                string.IsNullOrEmpty(user.email) || string.IsNullOrEmpty(user.diachi))
            {
                TempData["Message"] = "Vui lòng cập nhật đầy đủ thông tin cá nhân để tiếp tục thanh toán.";
                return RedirectToAction("ThanhToan");  
            }
            Cart cart = Session["Cart"] as Cart;

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Show", "ShoppingCart");
            }

            try
            {
 
                string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"];
                string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];
                string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];

                long orderId = DateTime.Now.Ticks;

                decimal totalAmount = (decimal)cart.Total();

                long vnp_Amount = (long)(totalAmount * 100);

                VnPayLibrary vnpay = new VnPayLibrary();
                vnpay.AddRequestData("vnp_Version", "2.1.0");
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
                vnpay.AddRequestData("vnp_Amount", vnp_Amount.ToString());
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", Request.UserHostAddress);
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toán đơn hàng {orderId}");
                vnpay.AddRequestData("vnp_OrderType", "other"); 
                vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
                vnpay.AddRequestData("vnp_TxnRef", orderId.ToString());

                string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi tạo thanh toán VNPAY: " + ex.Message);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tạo thanh toán. Vui lòng thử lại.";
                return RedirectToAction("Show", "ShoppingCart");
            }
        }
        public ActionResult VNPAYReturn()
        {
            string notes = Request.Form["notes"];
            int madh=0;

            Order ddh = new Order();
            Customer kh = (Customer)Session["user"];
            Cart cart = Session["Cart"] as Cart;

            if (cart != null)
            {
                ddh.Ngay = DateTime.Now;
                ddh.TongTien = (int)cart.Total();
                ddh.TrangThai = "Chờ xác nhận";
                ddh.GhiChu = notes;
                ddh.Makh = kh.makh;

                getUser userHelper = new getUser();
                Customer khs = userHelper.GetById(kh.makh);
                string diachi = khs.diachi;
                string wardName = khs.WardName;
                string districtName = khs.DistrictName;
                string provinceName = khs.ProvinceName;
                ddh.Diachi = $"{diachi}, {wardName}, {districtName}, {provinceName}";

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Insert Order and get madh
                            using (var command = new NpgsqlCommand("INSERT INTO don_hang (ngay, tong_tien, trang_thai, ghi_chu, makh, diachi) VALUES (@ngay, @tong_tien, @trang_thai, @ghi_chu, @makh, @diachi) RETURNING madh", connection))
                            {
                                command.Parameters.AddWithValue("ngay", ddh.Ngay);
                                command.Parameters.AddWithValue("tong_tien", ddh.TongTien);
                                command.Parameters.AddWithValue("trang_thai", ddh.TrangThai);
                                command.Parameters.AddWithValue("ghi_chu", ddh.GhiChu ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("makh", ddh.Makh ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("diachi", ddh.Diachi ?? (object)DBNull.Value);


                                madh = (int)command.ExecuteScalar();
                                Session["OrderId"] = madh;
                                if (Session["OrderId"] == null)
                                {
                                    throw new Exception("Không thể lưu OrderId vào Session.");
                                }
                            }


                            foreach (var item in cart.Items)
                            {
                                using (var command = new NpgsqlCommand("INSERT INTO chitiethoadon (madh, masp, soluong, gia) VALUES (@madh, @masp, @soluong, @gia)", connection))
                                {
                                    command.Parameters.AddWithValue("madh", madh); 
                                    command.Parameters.AddWithValue("masp", item.shopping_sp.masp);
                                    command.Parameters.AddWithValue("soluong", item.shopping_sl);
                                    command.Parameters.AddWithValue("gia", item.shopping_sp.gia);

                                    command.ExecuteNonQuery();
                                }
                                // Update the 'daban' column to reflect the quantity sold
                                using (var command = new NpgsqlCommand("UPDATE san_pham SET daban = daban + @soluong WHERE masp = @masp", connection))
                                {
                                    command.Parameters.AddWithValue("soluong", item.shopping_sl);
                                    command.Parameters.AddWithValue("masp", item.shopping_sp.masp);

                                    command.ExecuteNonQuery();
                                }
                            }


                            // Commit transaction
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            // Rollback in case of error
                            transaction.Rollback();
                            TempData["ErrorMessage"] = ex.Message;
                        }
                    }
                }
            }



            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; // Chuỗi bí mật từ cấu hình
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }

                int orderId = (int)Session["OrderId"];
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));  // Mã giao dịch VNPAY
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");  // Mã phản hồi của VNPAY
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");  // Trạng thái giao dịch
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"];  // Hash ký mật
                string terminalId = Request.QueryString["vnp_TmnCode"];  // Mã website (Terminal ID)
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;  // Số tiền thanh toán (chia cho 100 để có giá trị đúng)
                string bankCode = Request.QueryString["vnp_BankCode"];  // Mã ngân hàng

                // Kiểm tra tính hợp lệ của chữ ký (signature)
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        // Giao dịch thành công
                        ViewBag.Message = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ.";

                        // Cập nhật trạng thái thanh toán trong cơ sở dữ liệu
                        UpdatePaymentStatus(orderId, "Thành công", vnp_Amount ,vnpayTranId);
                    }
                    else
                    {
                        // Giao dịch không thành công
                        ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý. Mã lỗi: " + vnp_ResponseCode;
                    }

                    // Hiển thị thông tin giao dịch
                    ViewBag.TerminalID = "Mã Website (Terminal ID): " + terminalId;
                    ViewBag.TransactionRef = "Mã giao dịch thanh toán: " + orderId.ToString();
                    ViewBag.VnpayTransactionNo = "Mã giao dịch tại VNPAY: " + vnpayTranId.ToString();
                    ViewBag.Amount = "Số tiền thanh toán (VND): " + vnp_Amount.ToString();
                    ViewBag.BankCode = "Ngân hàng thanh toán: " + bankCode;
                }
                else
                {
                    ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý";
                }
            }

            return RedirectToAction("Xacnhandonhang", "ShoppingCart", new { orderId = madh });
        }

        public void UpdatePaymentStatus(long madh, string trangthai, decimal sotien, long magiaodich)
        {

            using (var connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Insert into thanhtoan table (payment record)
                    using (var command = new NpgsqlCommand("INSERT INTO thanhtoan (madh, trangthai, sotien, phuongthuc, thoigian,magd) VALUES (@madh, @trangthai, @sotien, @phuongthuc, @thoigian, @magd)", connection))
                    {
                        command.Parameters.AddWithValue("madh", madh);
                        command.Parameters.AddWithValue("trangthai", trangthai);
                        command.Parameters.AddWithValue("sotien", sotien);
                        command.Parameters.AddWithValue("phuongthuc", "VNPAY");
                        command.Parameters.AddWithValue("thoigian", DateTime.Now); 
                        command.Parameters.AddWithValue("magd", magiaodich); 

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi nếu cần
                    System.Diagnostics.Debug.WriteLine("Lỗi khi cập nhật trạng thái thanh toán: " + ex.Message);
                }
            }
        }

    }
}
