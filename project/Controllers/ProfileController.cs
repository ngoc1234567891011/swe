using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Npgsql;
using project.Models;

namespace project.Controllers
{
    public class ProfileController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        // GET: Profile/User
        public ActionResult User(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Customer customer;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM khach_hang WHERE makh = @id", connection))
                {
                    command.Parameters.AddWithValue("id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customer = new Customer
                            {
                                makh = reader.GetInt32(reader.GetOrdinal("makh")),
                                tenkh = reader.GetString(reader.GetOrdinal("tenkh")),
                                email = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString(reader.GetOrdinal("email")),
                                sdt = reader.GetString(reader.GetOrdinal("sdt")),
                                hinhanh = reader.GetString(reader.GetOrdinal("hinhanh")),
                                diachi = reader.IsDBNull(reader.GetOrdinal("diachi")) ? "" : reader.GetString(reader.GetOrdinal("diachi")),
                                matkhau = reader.IsDBNull(reader.GetOrdinal("matkhau")) ? "" : reader.GetString(reader.GetOrdinal("matkhau")),
                                province_code = reader.IsDBNull(reader.GetOrdinal("province_code")) ? null : reader.GetString(reader.GetOrdinal("province_code")),
                                district_code = reader.IsDBNull(reader.GetOrdinal("district_code")) ? null : reader.GetString(reader.GetOrdinal("district_code")),
                                ward_code = reader.IsDBNull(reader.GetOrdinal("ward_code")) ? null : reader.GetString(reader.GetOrdinal("ward_code"))

                            };
                        }
                        else
                        {
                            return HttpNotFound();
                        }
                    }
                }
            }

            return View(customer);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult User(Customer customer, HttpPostedFileBase avatar, string oldimage, string province_code, string district_code, string ward_code)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload
                    if (avatar != null && avatar.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(avatar.FileName);
                        string path = Path.Combine(Server.MapPath("~/Content/user"), fileName);
                        avatar.SaveAs(path);
                        customer.hinhanh = fileName;

                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(oldimage))
                        {
                            string oldImagePath = Path.Combine(Server.MapPath("~/Content/user"), oldimage);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                    }
                    else
                    {
                        customer.hinhanh = oldimage;
                    }

                    // Update address fields
                    customer.province_code = string.IsNullOrEmpty(province_code) ? null : province_code;
                    customer.district_code = string.IsNullOrEmpty(district_code) ? null : district_code;
                    customer.ward_code = string.IsNullOrEmpty(ward_code) ? null : ward_code;


                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        using (var command = new NpgsqlCommand("UPDATE khach_hang SET tenkh = @tenkh, email = @email, sdt = @sdt, diachi = @diachi, gioitinh = @gioitinh, namsinh = @namsinh, hinhanh = @hinhanh, province_code = @province_code, district_code = @district_code, ward_code = @ward_code WHERE makh = @makh", connection))
                        {
                            command.Parameters.AddWithValue("makh", customer.makh);
                            command.Parameters.AddWithValue("tenkh", string.IsNullOrEmpty(customer.tenkh) ? (object)DBNull.Value : customer.tenkh);
                            command.Parameters.AddWithValue("email", string.IsNullOrEmpty(customer.email) ? (object)DBNull.Value : customer.email);
                            command.Parameters.AddWithValue("sdt", string.IsNullOrEmpty(customer.sdt) ? (object)DBNull.Value : customer.sdt);
                            command.Parameters.AddWithValue("diachi", string.IsNullOrEmpty(customer.diachi) ? (object)DBNull.Value : customer.diachi);
                            command.Parameters.AddWithValue("gioitinh", string.IsNullOrEmpty(customer.gioitinh) ? (object)DBNull.Value : customer.gioitinh);
                            command.Parameters.AddWithValue("namsinh", customer.namsinh == null ? (object)DBNull.Value : customer.namsinh);
                            command.Parameters.AddWithValue("hinhanh", string.IsNullOrEmpty(customer.hinhanh) ? (object)DBNull.Value : customer.hinhanh);
                            command.Parameters.AddWithValue("province_code", string.IsNullOrEmpty(customer.province_code) ? (object)DBNull.Value : customer.province_code);
                            command.Parameters.AddWithValue("district_code", string.IsNullOrEmpty(customer.district_code) ? (object)DBNull.Value : customer.district_code);
                            command.Parameters.AddWithValue("ward_code", string.IsNullOrEmpty(customer.ward_code) ? (object)DBNull.Value : customer.ward_code);

                            command.ExecuteNonQuery();
                        }

                    }

                    Session["user"] = customer;
                    return RedirectToAction("User", new { id = customer.makh });
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Có lỗi xảy ra: " + ex.Message;
                }
            }

            // Re-populate dropdowns in case of validation errors
            ViewBag.Provinces = GetProvinces();
            ViewBag.Districts = GetDistricts(customer.province_code);
            ViewBag.Wards = GetWards(customer.district_code);

            return View(customer);
        }




        // GET: Profile/LichSu
        public ActionResult LichSu()
        {
            var customer = Session["user"] as Customer;
            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }

            IEnumerable<Order> orders;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM don_hang WHERE makh = @makh", connection))
                {
                    command.Parameters.AddWithValue("makh", customer.makh);
                    using (var reader = command.ExecuteReader())
                    {
                        var orderList = new List<Order>();
                        while (reader.Read())
                        {
                            orderList.Add(new Order
                            {
                                Madh = reader.GetInt32(reader.GetOrdinal("madh")),
                                Ngay = reader.GetDateTime(reader.GetOrdinal("ngay")),
                                TongTien = reader.GetInt32(reader.GetOrdinal("tong_tien")),
                                Diachi = !reader.IsDBNull(reader.GetOrdinal("diachi")) ? reader.GetString(reader.GetOrdinal("diachi")) : "",
                                TrangThai = reader.GetString(reader.GetOrdinal("trang_thai")),
                                GhiChu = !reader.IsDBNull(reader.GetOrdinal("ghi_chu")) ? reader.GetString(reader.GetOrdinal("ghi_chu")) : ""
                            });
                        }
                        orders = orderList;
                    }
                }
            }

            return View(orders);
        }

        // GET: Profile/Details
        public ActionResult Details(int id)
        {
            IEnumerable<OrderDetail> orderDetails;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(@"
                        SELECT 
                            cthd.madh,
                            cthd.masp,
                            cthd.soluong,
                            cthd.gia,
                            sp.*
                        FROM chitiethoadon cthd
                        JOIN san_pham sp ON cthd.masp = sp.masp
                        WHERE cthd.madh = @id", connection))
                {
                    command.Parameters.AddWithValue("id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        var detailsList = new List<OrderDetail>();
                        while (reader.Read())
                        {
                            detailsList.Add(new OrderDetail
                            {
                                Madh = reader.GetInt32(reader.GetOrdinal("madh")),
                                Masp = reader.GetInt32(reader.GetOrdinal("masp")),
                                SoLuong = reader.GetInt32(reader.GetOrdinal("soluong")),
                                hinhanh = reader.GetString(reader.GetOrdinal("hinhanh")),
                                tensp = reader.GetString(reader.GetOrdinal("tensp")),
                                Gia = reader.GetInt32(reader.GetOrdinal("gia")),

                            });
                        }
                        orderDetails = detailsList;
                    }
                }
            }

            return View(orderDetails);
        }

        // GET: Profile/GetProvinces
        public JsonResult GetProvinces()
        {
            List<SelectListItem> provinces = new List<SelectListItem>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT code, name FROM provinces ORDER BY name", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            provinces.Add(new SelectListItem
                            {
                                Value = reader.GetString(reader.GetOrdinal("code")),
                                Text = reader.GetString(reader.GetOrdinal("name"))
                            });
                        }
                    }
                }
            }

            return Json(provinces, JsonRequestBehavior.AllowGet);
        }

        // GET: Profile/GetDistricts
        public JsonResult GetDistricts(string provinceCode)
        {
            List<SelectListItem> districts = new List<SelectListItem>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT code, name FROM districts WHERE province_code = @provinceCode ORDER BY name", connection))
                {
                    command.Parameters.AddWithValue("provinceCode", provinceCode);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            districts.Add(new SelectListItem
                            {
                                Value = reader.GetString(reader.GetOrdinal("code")),
                                Text = reader.GetString(reader.GetOrdinal("name"))
                            });
                        }
                    }
                }
            }

            return Json(districts, JsonRequestBehavior.AllowGet);
        }

        // GET: Profile/GetWards
        public ActionResult GetWards(string districtCode)
        {
            List<SelectListItem> wards = new List<SelectListItem>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT code, name FROM wards WHERE district_code = @districtCode ORDER BY name", connection))
                {
                    command.Parameters.AddWithValue("districtCode", districtCode);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            wards.Add(new SelectListItem
                            {
                                Value = reader.GetString(reader.GetOrdinal("code")),
                                Text = reader.GetString(reader.GetOrdinal("name"))
                            });
                        }
                    }
                }
            }

            return Json(wards, JsonRequestBehavior.AllowGet);
        }
    }
}
