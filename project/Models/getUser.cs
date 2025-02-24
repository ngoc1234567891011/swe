using System;
using System.Configuration;
using Npgsql;

namespace project.Models
{
    public class getUser
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        public Customer GetById(int id)
        {
            Customer customer = null;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("SELECT * FROM khach_hang WHERE makh = @id", connection))
                {
                    // Sửa lại tham số để đúng với câu lệnh SQL
                    command.Parameters.AddWithValue("id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customer = new Customer
                            {
                                makh = reader.GetInt32(reader.GetOrdinal("makh")),
                                tenkh = reader.IsDBNull(reader.GetOrdinal("tenkh")) ? "" : reader.GetString(reader.GetOrdinal("tenkh")),
                                sdt = reader.IsDBNull(reader.GetOrdinal("sdt")) ? "" : reader.GetString(reader.GetOrdinal("sdt")),
                                matkhau = reader.IsDBNull(reader.GetOrdinal("matkhau")) ? "" : reader.GetString(reader.GetOrdinal("matkhau")),
                                email = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString(reader.GetOrdinal("email")),
                                namsinh = reader.IsDBNull(reader.GetOrdinal("namsinh")) ? "" : reader.GetString(reader.GetOrdinal("namsinh")),
                                gioitinh = reader.IsDBNull(reader.GetOrdinal("gioitinh")) ? "" : reader.GetString(reader.GetOrdinal("gioitinh")),
                                diachi = reader.IsDBNull(reader.GetOrdinal("diachi")) ? "" : reader.GetString(reader.GetOrdinal("diachi")),
                                ward_code = reader.IsDBNull(reader.GetOrdinal("ward_code")) ? "" : reader.GetString(reader.GetOrdinal("ward_code")),
                                district_code = reader.IsDBNull(reader.GetOrdinal("district_code")) ? "" : reader.GetString(reader.GetOrdinal("district_code")),
                                province_code = reader.IsDBNull(reader.GetOrdinal("province_code")) ? "" : reader.GetString(reader.GetOrdinal("province_code"))
                            };
                        }
                    }
                }

                // Nếu customer không null, gọi thêm các phương thức lấy tên của các khu vực
                if (customer != null)
                {
                    customer.WardName = GetWardNameByCode(customer.ward_code);
                    customer.DistrictName = GetDistrictNameByCode(customer.district_code);
                    customer.ProvinceName = GetProvinceNameByCode(customer.province_code);
                }
            }

            return customer;
        }

        public string GetWardNameByCode(string wardCode)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("SELECT name FROM wards WHERE code = @code", connection))
                {
                    // Kiểm tra giá trị của wardCode
                    if (string.IsNullOrEmpty(wardCode))
                    {
                        command.Parameters.AddWithValue("code", DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("code", wardCode);
                    }

                    // Trả về null nếu không tìm thấy, tránh lỗi
                    return command.ExecuteScalar()?.ToString() ?? "";
                }
            }
        }

        public string GetDistrictNameByCode(string districtCode)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("SELECT name FROM districts WHERE code = @code", connection))
                {
                    // Kiểm tra giá trị của districtCode
                    if (string.IsNullOrEmpty(districtCode))
                    {
                        command.Parameters.AddWithValue("code", DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("code", districtCode);
                    }

                    // Trả về null nếu không tìm thấy, tránh lỗi
                    return command.ExecuteScalar()?.ToString() ?? "";
                }
            }
        }

        public string GetProvinceNameByCode(string provinceCode)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("SELECT name FROM provinces WHERE code = @code", connection))
                {
                    // Kiểm tra giá trị của provinceCode
                    if (string.IsNullOrEmpty(provinceCode))
                    {
                        command.Parameters.AddWithValue("code", DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("code", provinceCode);
                    }

                    // Trả về null nếu không tìm thấy, tránh lỗi
                    return command.ExecuteScalar()?.ToString() ?? "";
                }
            }
        }

    }
}
