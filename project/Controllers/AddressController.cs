using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace project.Controllers
{
    public class AddressController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        public ActionResult GetProvinces()
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

        public ActionResult GetDistricts(string provinceCode)
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