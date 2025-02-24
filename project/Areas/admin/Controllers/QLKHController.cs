using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web.Mvc;
using Npgsql;
using project.Models;

namespace project.Areas.admin.Controllers
{
    [CheckRole(3)]
    public class QLKHController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        public ActionResult Index()
        {
            List<Customer> customers = new List<Customer>();
            getUser g = new getUser();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("SELECT * FROM khach_hang", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var customer = new Customer
                            {
                                makh = reader.GetInt32(reader.GetOrdinal("makh")),
                                tenkh = reader.GetString(reader.GetOrdinal("tenkh")),
                                sdt = reader.GetString(reader.GetOrdinal("sdt")),
                                email = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString(reader.GetOrdinal("email")),
                                hinhanh = reader.IsDBNull(reader.GetOrdinal("hinhanh")) ? "" : reader.GetString(reader.GetOrdinal("hinhanh")),
                                diachi = reader.IsDBNull(reader.GetOrdinal("diachi")) ? "" : reader.GetString(reader.GetOrdinal("diachi")),
                                province_code = reader.IsDBNull(reader.GetOrdinal("province_code")) ? "" : reader.GetString(reader.GetOrdinal("province_code")),
                                district_code = reader.IsDBNull(reader.GetOrdinal("district_code")) ? "" : reader.GetString(reader.GetOrdinal("district_code")),
                                ward_code = reader.IsDBNull(reader.GetOrdinal("ward_code")) ? "" : reader.GetString(reader.GetOrdinal("ward_code")),
                                anhthudo = reader.IsDBNull(reader.GetOrdinal("anhthudo")) ? "" : reader.GetString(reader.GetOrdinal("anhthudo")),
                                facebook_id = reader.IsDBNull(reader.GetOrdinal("facebook_id")) ? "" : reader.GetString(reader.GetOrdinal("facebook_id")),
                                google_id = reader.IsDBNull(reader.GetOrdinal("google_id")) ? "" : reader.GetString(reader.GetOrdinal("google_id")),
                                gioitinh = reader.IsDBNull(reader.GetOrdinal("gioitinh")) ? "" : reader.GetString(reader.GetOrdinal("gioitinh")),
                                namsinh = reader.IsDBNull(reader.GetOrdinal("namsinh")) ? "" : reader.GetString(reader.GetOrdinal("namsinh"))
                            };

                            // Lấy tên khu vực
                            customer.WardName = g.GetWardNameByCode(customer.ward_code);
                            customer.DistrictName = g.GetDistrictNameByCode(customer.district_code);
                            customer.ProvinceName = g.GetProvinceNameByCode(customer.province_code);

                            customers.Add(customer);
                        }
                    }
                }
            }

            return View(customers);
        }


        public ActionResult Details(int id)
        {
            Customer customer = null;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(@"
                    SELECT makh, tenkh, sdt, email, hinhanh, diachi, matkhau, province_code, 
                           district_code, ward_code, anhthudo, facebook_id, google_id, gioitinh, namsinh
                    FROM khach_hang 
                    WHERE makh = @id", connection))
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
                                sdt = reader.GetString(reader.GetOrdinal("sdt")),
                                email = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString(reader.GetOrdinal("email")),
                                hinhanh = reader.IsDBNull(reader.GetOrdinal("hinhanh")) ? "" : reader.GetString(reader.GetOrdinal("hinhanh")),
                                diachi = reader.IsDBNull(reader.GetOrdinal("diachi")) ? "" : reader.GetString(reader.GetOrdinal("diachi")),
                                province_code = reader.IsDBNull(reader.GetOrdinal("province_code")) ? "" : reader.GetString(reader.GetOrdinal("province_code")),
                                district_code = reader.IsDBNull(reader.GetOrdinal("district_code")) ? "" : reader.GetString(reader.GetOrdinal("district_code")),
                                ward_code = reader.IsDBNull(reader.GetOrdinal("ward_code")) ? "" : reader.GetString(reader.GetOrdinal("ward_code")),
                                anhthudo = reader.IsDBNull(reader.GetOrdinal("anhthudo")) ? "" : reader.GetString(reader.GetOrdinal("anhthudo")),
                                facebook_id = reader.IsDBNull(reader.GetOrdinal("facebook_id")) ? "" : reader.GetString(reader.GetOrdinal("facebook_id")),
                                google_id = reader.IsDBNull(reader.GetOrdinal("google_id")) ? "" : reader.GetString(reader.GetOrdinal("google_id")),
                                gioitinh = reader.IsDBNull(reader.GetOrdinal("gioitinh")) ? "" : reader.GetString(reader.GetOrdinal("gioitinh")),
                                namsinh = reader.IsDBNull(reader.GetOrdinal("namsinh")) ? "" : reader.GetString(reader.GetOrdinal("namsinh"))

                            };
                        }
                    }
                    // Nếu khách hàng tồn tại, lấy tên các khu vực từ mã
                    getUser g=new getUser();
                    if (customer != null)
                    {
                        customer.WardName = g.GetWardNameByCode(customer.ward_code);
                        customer.DistrictName = g.GetDistrictNameByCode(customer.district_code);
                        customer.ProvinceName = g.GetProvinceNameByCode(customer.province_code);
                    }
                }
            }

            if (customer == null)
            {
                return HttpNotFound();
            }

            return View(customer);
        }
    }

    
}
