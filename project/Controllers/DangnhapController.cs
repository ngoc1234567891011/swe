using Npgsql;
using project.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Facebook;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;


namespace project.Controllers
{
    public class DangnhapController : Controller
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;
        private static string _generatedOtp1;

        private string phoneNumber1;

        [HttpPost]
        public async Task<ActionResult> SendOtp(string phoneNumber)
        {
            // Loại bỏ các ký tự không phải là chữ số (nếu có)
            phoneNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Nếu số điện thoại bắt đầu với '0', thay thế bằng '+84'
            if (phoneNumber.StartsWith("0"))
            {
                phoneNumber1 = "+84" + phoneNumber.Substring(1);
            }
            // Kiểm tra số điện thoại đã tồn tại trong cơ sở dữ liệu
            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();

                // Kiểm tra số điện thoại đã có trong cơ sở dữ liệu
                NpgsqlCommand checkPhoneCmd = new NpgsqlCommand("SELECT COUNT(*) FROM khach_hang WHERE sdt = @sdt", con);
                checkPhoneCmd.Parameters.AddWithValue("@sdt", phoneNumber);
                int phoneCount = Convert.ToInt32(checkPhoneCmd.ExecuteScalar());

                if (phoneCount > 0)
                {
                    // Nếu số điện thoại đã tồn tại, trả về lỗi
                    return Json(new { success = false, message = "Số điện thoại đã tồn tại." });
                }
                else
                {
                    // Nếu số điện thoại chưa có trong hệ thống, thực hiện gửi OTP
                    _generatedOtp1 = await SmsService.SendOtpAsync(phoneNumber1);

                    if (!string.IsNullOrEmpty(_generatedOtp1))
                    {
                        // Lưu số điện thoại vào session để sử dụng khi xác minh
                        Session["PhoneNumber"] = phoneNumber;

                        return Json(new { success = true, message = "OTP đã được gửi tới số điện thoại của bạn!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Không thể gửi OTP. Vui lòng thử lại!" });
                    }
                }
            }
        }

        // API xác minh OTP
        [HttpPost]
        public ActionResult VerifyOtp(string inputOtp, string phoneNumber, string username, string password)
        {
            // Lấy OTP đã lưu trong session hoặc biến tĩnh
            string generatedOtp = _generatedOtp1;

            if (string.IsNullOrEmpty(generatedOtp))
            {
                return Json(new { success = false, message = "Mã OTP đã hết hạn hoặc không hợp lệ." });
            }

            if (inputOtp == generatedOtp)
            {
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    return Json(new { success = false, message = "Số điện thoại không hợp lệ." });
                }


                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    // Kiểm tra số điện thoại đã tồn tại trong cơ sở dữ liệu
                    NpgsqlCommand checkPhoneCmd = new NpgsqlCommand("SELECT COUNT(*) FROM khach_hang WHERE sdt = @sdt", con);
                    checkPhoneCmd.Parameters.AddWithValue("@sdt", phoneNumber);
                    int phoneCount = Convert.ToInt32(checkPhoneCmd.ExecuteScalar());

                    // Nếu số điện thoại đã tồn tại
                    if (phoneCount > 0)
                    {
                        return Json(new { success = false, message = "Số điện thoại đã tồn tại trong hệ thống." });
                    }

                    // Tiến hành lưu thông tin người dùng vào cơ sở dữ liệu
                    var sourceImagePath = Server.MapPath("~/Content/img/user.png");
                    var avatarFolderPath = Server.MapPath("~/Content/user");
                    var defaultFileName = "user.png";
                    var filePath = Path.Combine(avatarFolderPath, defaultFileName);
                    System.IO.File.Copy(sourceImagePath, filePath, true);

                    // Thực hiện câu lệnh INSERT để lưu thông tin người dùng
                    NpgsqlCommand insertUserCmd = new NpgsqlCommand("INSERT INTO khach_hang (tenkh, matkhau, sdt, hinhanh) VALUES (@Username, @Password, @SDT, @Avatar)", con);
                    insertUserCmd.Parameters.AddWithValue("@Username", username);  // Lấy tên người dùng từ client
                    insertUserCmd.Parameters.AddWithValue("@Password", password);  // Lấy mật khẩu từ client
                    insertUserCmd.Parameters.AddWithValue("@SDT", phoneNumber);  // Số điện thoại chuẩn hóa
                    insertUserCmd.Parameters.AddWithValue("@Avatar", defaultFileName);

                    int result = insertUserCmd.ExecuteNonQuery();

                    // Kiểm tra kết quả
                    if (result > 0)
                    {
                        // Đăng ký thành công, trả về thông báo
                        return Json(new { success = true, message = "Đăng ký thành công. Vui lòng đăng nhập." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Đăng ký thất bại. Vui lòng thử lại!" });
                    }
                }
            }
            else
            {
                // OTP không đúng
                return Json(new { success = false, message = "Mã OTP không chính xác!" });
            }
        }




        [System.Web.Mvc.HttpGet]
        public System.Web.Mvc.ActionResult register()
        {
            return View();
        }

        [System.Web.Mvc.HttpPost]
        public System.Web.Mvc.ActionResult register(Customer ac)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(ac.sdt))
                {
                    ModelState.AddModelError("", "Số điện thoại không được để trống");
                    return View(ac);
                }

                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    NpgsqlCommand checkPhoneCmd = new NpgsqlCommand("SELECT COUNT(*) FROM khach_hang WHERE sdt = @sdt", con);
                    checkPhoneCmd.Parameters.AddWithValue("@sdt", ac.sdt);
                    int phoneCount = Convert.ToInt32(checkPhoneCmd.ExecuteScalar());

                    if (phoneCount > 0)
                    {
                        ModelState.AddModelError("", "Số điện thoại đã tồn tại");
                    }
                    else
                    {
                        var sourceImagePath = Server.MapPath("~/Content/img/user.png");
                        var avatarFolderPath = Server.MapPath("~/Content/user");
                        var defaultFileName = "user.png";
                        var filePath = Path.Combine(avatarFolderPath, defaultFileName);
                        System.IO.File.Copy(sourceImagePath, filePath, true);

                        NpgsqlCommand insertUserCmd = new NpgsqlCommand("INSERT INTO khach_hang (email, tenkh, matkhau, sdt, hinhanh) VALUES (@email, @Username, @Password, @SDT, @Avatar)", con);
                        insertUserCmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(ac.email) ? (object)DBNull.Value : ac.email);
                        insertUserCmd.Parameters.AddWithValue("@Username", ac.tenkh);
                        insertUserCmd.Parameters.AddWithValue("@Password", ac.matkhau);
                        insertUserCmd.Parameters.AddWithValue("@SDT", ac.sdt);
                        insertUserCmd.Parameters.AddWithValue("@Avatar", defaultFileName);

                        int result = insertUserCmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ViewBag.S = "Đăng ký thành công";
                            ac = new Customer(); // Clear the model
                        }
                        else
                        {
                            ModelState.AddModelError("", "Đăng ký lỗi");
                        }
                    }
                }
            }

            return View(ac);
        }

        // Facebook Login
        public System.Web.Mvc.ActionResult login()
        {
            var fb = new FacebookClient();
            var fbLoginUrl = fb.GetLoginUrl(new
            {
                client_id = "449830960926726",
                redirect_uri = Url.Action("FacebookLogin", "Dangnhap", null, Request.Url.Scheme),
                scope = "public_profile"
            });
            ViewBag.FacebookUrl = fbLoginUrl;

            return View();
        }

        [System.Web.Mvc.HttpPost]
        public System.Web.Mvc.ActionResult login(string sdt, string password)
        {
            if (string.IsNullOrEmpty(sdt) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Thông tin đăng nhập không hợp lệ.");
                return View();
            }

            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();

                NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM khach_hang WHERE sdt = @sdt AND matkhau = @matkhau", con);
                cmd.Parameters.AddWithValue("@sdt", sdt);
                cmd.Parameters.AddWithValue("@matkhau", password);

                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    var user = new Customer
                    {
                        makh = reader.GetInt32(reader.GetOrdinal("makh")),
                        tenkh = reader.GetString(reader.GetOrdinal("tenkh")),
                        sdt = reader.GetString(reader.GetOrdinal("sdt")),
                        hinhanh = reader.GetString(reader.GetOrdinal("hinhanh"))
                    };

                    if (!reader.IsDBNull(reader.GetOrdinal("email")))
                    {
                        user.email = reader.GetString(reader.GetOrdinal("email"));
                    }

                    Session["user"] = user;
                    return RedirectToAction("Index", "Home");
                }

                TempData["error"] = "Tài khoản không đúng";
                return View();
            }
        }
        public System.Web.Mvc.ActionResult FacebookLogin(string code)
        {
            try
            {
                var fb = new FacebookClient();
                dynamic result = fb.Get("/oauth/access_token", new
                {
                    client_id = "449830960926726",
                    client_secret = "8826e5f026f9dce5a503a420f6cbb000",
                    redirect_uri = "https://localhost:44349/Dangnhap/FacebookLogin",
                    code = code
                });

                fb.AccessToken = result.access_token;

                dynamic me = fb.Get("/me?fields=id,name");
                string facebookId = me.id;
                string name = me.name;

                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    NpgsqlCommand checkUserCmd = new NpgsqlCommand("SELECT * FROM khach_hang WHERE facebook_id = @FacebookId", con);
                    checkUserCmd.Parameters.AddWithValue("@FacebookId", facebookId);

                    NpgsqlDataReader reader = checkUserCmd.ExecuteReader();

                    Customer user = null;

                    if (reader.Read())
                    {
                        user = new Customer
                        {
                            makh = reader.GetInt32(reader.GetOrdinal("makh")),
                            tenkh = reader.GetString(reader.GetOrdinal("tenkh")),
                            hinhanh = reader.GetString(reader.GetOrdinal("hinhanh")),
                            facebook_id = facebookId
                        };
                    }
                    else
                    {
                        reader.Close();

                        string defaultAvatar = "user.png";

                        NpgsqlCommand insertUserCmd = new NpgsqlCommand("INSERT INTO khach_hang (tenkh, facebook_id, hinhanh) VALUES (@Name, @FacebookId, @Avatar) RETURNING makh", con);
                        insertUserCmd.Parameters.AddWithValue("@Name", name);
                        insertUserCmd.Parameters.AddWithValue("@FacebookId", facebookId);
                        insertUserCmd.Parameters.AddWithValue("@Avatar", defaultAvatar);

                        int newUserId = (int)insertUserCmd.ExecuteScalar();


                        user = new Customer
                        {
                            makh = newUserId,
                            tenkh = name,
                            hinhanh = defaultAvatar,
                            facebook_id = facebookId
                        };
                    }

                    Session["user"] = user;

                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "Đăng nhập Facebook thất bại: " + ex.Message;
                return RedirectToAction("login");
            }
        }
        public System.Web.Mvc.ActionResult dangxuat()
        {
            Session.Remove("user");
            FormsAuthentication.SignOut();
            return RedirectToAction("login");
        }
        public ActionResult quenmk()
        {
            // Trả về view ForgotPassword.cshtml
            return View();
        }

        private static string _generatedOtp;

        // API gửi OTP
        [HttpPost]
        public async Task<ActionResult> SendOtpForPasswordReset(string phoneNumber)
        {
            // Kiểm tra số điện thoại có trong hệ thống không
            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();

                // Kiểm tra số điện thoại có trong cơ sở dữ liệu không
                NpgsqlCommand checkPhoneCmd = new NpgsqlCommand("SELECT COUNT(*) FROM khach_hang WHERE sdt = @sdt", con);
                checkPhoneCmd.Parameters.AddWithValue("@sdt", phoneNumber);
                int phoneCount = Convert.ToInt32(checkPhoneCmd.ExecuteScalar());

                if (phoneCount == 0)
                {
                    return Json(new { success = false, message = "Số điện thoại không tồn tại trong hệ thống." });
                }
            }

            if (phoneNumber.StartsWith("0"))
            {
                phoneNumber1 = "+84" + phoneNumber.Substring(1);
            }
            // Gửi OTP
            _generatedOtp = await SmsService.SendOtpAsync(phoneNumber1);

            return Json(new { success = true, message = "OTP đã được gửi đến số điện thoại của bạn!" });
        }

        [HttpPost]
        public ActionResult ResetPassword(string inputOtp, string newPassword, string phoneNumber)
        {
            string generatedOtp = _generatedOtp;

            // Kiểm tra OTP người dùng nhập vào
            if (inputOtp == generatedOtp)
            {
                // Chuẩn hóa mật khẩu mới
                if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
                {
                    return Json(new { success = false, message = "Mật khẩu mới không hợp lệ." });
                }

                // Mã hóa mật khẩu mới bằng SHA-256
                string hashedPassword = HashPasswordWithSHA256(newPassword);

                // Cập nhật mật khẩu trong cơ sở dữ liệu
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    // Cập nhật mật khẩu cho người dùng theo số điện thoại
                    NpgsqlCommand updatePasswordCmd = new NpgsqlCommand("UPDATE khach_hang SET matkhau = @matkhau WHERE sdt = @sdt", con);
                    updatePasswordCmd.Parameters.AddWithValue("@matkhau", hashedPassword);
                    updatePasswordCmd.Parameters.AddWithValue("@sdt", phoneNumber);

                    int result = updatePasswordCmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        return Json(new { success = true, message = "Mật khẩu đã được cập nhật thành công!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại." });
                    }
                }
            }
            else
            {
                return Json(new { success = false, message = "Mã OTP không chính xác!" });
            }
        }

        // Hàm mã hóa mật khẩu bằng SHA-256
        private string HashPasswordWithSHA256(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Chuyển mật khẩu thành byte array và tính hash
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Chuyển kết quả thành chuỗi hexadecimal
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        [HttpPost]
        public JsonResult GoogleLogin()
        {
            var idToken = string.Empty;

            using (var reader = new StreamReader(Request.InputStream))
            {
                var body = reader.ReadToEnd();
                Console.WriteLine("Request body: " + body);

                if (string.IsNullOrEmpty(body))
                {
                    return Json(new { success = false, message = "Không nhận được dữ liệu", error = "Empty request body" });
                }

                try
                {
                    dynamic data = JsonConvert.DeserializeObject(body);
                    idToken = data?.idToken;

                    if (string.IsNullOrEmpty(idToken))
                    {
                        return Json(new { success = false, message = "Token không hợp lệ", error = "Token is null or empty" });
                    }

                    // Xác thực token
                    var payload = GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { "11359001624-14faap13s963qi0n0pcfo9i16duhbrh8.apps.googleusercontent.com" }
                    }).Result;

                    var email = payload.Email ?? ""; // Nếu email null thì lưu là ""
                    var name = payload.Name ?? "";  // Nếu name null thì lưu là ""
                    var picture = payload.Picture ?? ""; // Nếu picture null thì lưu là ""
                    var googleId = payload.Subject ?? ""; // Google ID
                    var phone = ""; // Mặc định giá trị là ""
                    var address = ""; // Mặc định giá trị là ""
                    var password = ""; // Mặc định giá trị là ""
                    var provinceCode = ""; // Mặc định giá trị là ""
                    var districtCode = ""; // Mặc định giá trị là ""
                    var wardCode = ""; // Mặc định giá trị là ""
                    var anhThuDo = ""; // Mặc định giá trị là ""
                    var facebookId = ""; // Mặc định giá trị là ""
                    var gender = ""; // Mặc định giá trị là ""
                    var yearOfBirth = ""; // Mặc định giá trị là ""

                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        conn.Open();

                        // Kiểm tra xem email đã tồn tại chưa
                        var checkEmailQuery = "SELECT * FROM khach_hang WHERE email = @Email";
                        using (var cmd = new NpgsqlCommand(checkEmailQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@Email", email);
                            using (var reader2 = cmd.ExecuteReader())
                            {
                                if (reader2.Read())
                                {
                                    // Email đã tồn tại, lấy thông tin khách hàng từ database
                                    var existingCustomer = new Customer
                                    {
                                        makh = Convert.ToInt32(reader2["makh"]),
                                        email = reader2["email"]?.ToString() ?? "",
                                        tenkh = reader2["tenkh"]?.ToString() ?? "",
                                        hinhanh = reader2["hinhanh"]?.ToString() ?? "",
                                        google_id = reader2["google_id"]?.ToString() ?? "",
                                        sdt = reader2["sdt"]?.ToString() ?? "",
                                        diachi = reader2["diachi"]?.ToString() ?? "",
                                        province_code = reader2["province_code"]?.ToString() ?? "",
                                        district_code = reader2["district_code"]?.ToString() ?? "",
                                        ward_code = reader2["ward_code"]?.ToString() ?? "",
                                        anhthudo = reader2["anhthudo"]?.ToString() ?? "",
                                        facebook_id = reader2["facebook_id"]?.ToString() ?? "",
                                        gioitinh = reader2["gioitinh"]?.ToString() ?? "",
                                        namsinh = reader2["namsinh"]?.ToString() ?? ""
                                    };

                                    // Lưu vào Session
                                    Session["user"] = existingCustomer;

                                    return Json(new { success = true, message = "Đăng nhập thành công", redirectUrl = "/Home" });
                                }
                            }
                        }

                        // Nếu email chưa tồn tại, thêm khách hàng mới
                        var insertQuery = @"
                    INSERT INTO khach_hang 
                    (tenkh, email, hinhanh, google_id, sdt, diachi, matkhau, 
                     province_code, district_code, ward_code, anhthudo, facebook_id, 
                     gioitinh, namsinh)
                    VALUES 
                    (@Name, @Email, @Picture, @GoogleId, @Phone, @Address, @Password, 
                     @ProvinceCode, @DistrictCode, @WardCode, @AnhThuDo, @FacebookId, 
                     @Gender, @YearOfBirth)
                    RETURNING makh, tenkh, email, hinhanh, google_id, sdt, diachi, 
                              province_code, district_code, ward_code, anhthudo, facebook_id, 
                              gioitinh, namsinh";
                        using (var insertCmd = new NpgsqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@Name", name);
                            insertCmd.Parameters.AddWithValue("@Email", email);
                            insertCmd.Parameters.AddWithValue("@Picture", picture);
                            insertCmd.Parameters.AddWithValue("@GoogleId", googleId);
                            insertCmd.Parameters.AddWithValue("@Phone", phone);
                            insertCmd.Parameters.AddWithValue("@Address", address);
                            insertCmd.Parameters.AddWithValue("@Password", password);
                            insertCmd.Parameters.AddWithValue("@ProvinceCode", provinceCode);
                            insertCmd.Parameters.AddWithValue("@DistrictCode", districtCode);
                            insertCmd.Parameters.AddWithValue("@WardCode", wardCode);
                            insertCmd.Parameters.AddWithValue("@AnhThuDo", anhThuDo);
                            insertCmd.Parameters.AddWithValue("@FacebookId", facebookId);
                            insertCmd.Parameters.AddWithValue("@Gender", gender);
                            insertCmd.Parameters.AddWithValue("@YearOfBirth", yearOfBirth);

                            using (var reader1 = insertCmd.ExecuteReader())
                            {
                                if (reader1.Read())
                                {
                                    // Lấy thông tin khách hàng vừa được thêm
                                    var newCustomer = new Customer
                                    {
                                        makh = Convert.ToInt32(reader1["makh"]),
                                        email = reader1["email"]?.ToString() ?? "",
                                        tenkh = reader1["tenkh"]?.ToString() ?? "",
                                        hinhanh = reader1["hinhanh"]?.ToString() ?? "",
                                        google_id = reader1["google_id"]?.ToString() ?? "",
                                        sdt = reader1["sdt"]?.ToString() ?? "",
                                        diachi = reader1["diachi"]?.ToString() ?? "",
                                        province_code = reader1["province_code"]?.ToString() ?? "",
                                        district_code = reader1["district_code"]?.ToString() ?? "",
                                        ward_code = reader1["ward_code"]?.ToString() ?? "",
                                        anhthudo = reader1["anhthudo"]?.ToString() ?? "",
                                        facebook_id = reader1["facebook_id"]?.ToString() ?? "",
                                        gioitinh = reader1["gioitinh"]?.ToString() ?? "",
                                        namsinh = reader1["namsinh"]?.ToString() ?? ""
                                    };

                                    // Lưu vào Session
                                    Session["user"] = newCustomer;
                                }
                            }
                        }
                    }

                    return Json(new { success = true, message = "Đăng nhập thành công", redirectUrl = "/Home" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Xác thực thất bại", error = ex.Message });
                }
            }
        }


    }



}

