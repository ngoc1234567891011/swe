using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using Npgsql;
using project.Models;

namespace project.Controllers
{
    public class lienheController : Controller
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        // GET: contact
        public ActionResult contact()
        {
            return View();
        }

        [HttpPost]
        public ActionResult contact([Bind(Include = "ma_gy, makh, chu_de, noi_dung, ngay, trang_thai, ad_phanhoi")] Ykr r)
        {
            if (ModelState.IsValid)
            {
                Customer kh = (Customer)Session["user"];
                r.makh = kh.makh;
                r.ngay = DateTime.Now;
                r.trang_thai = "s";
                r.ad_phanhoi = r.ad_phanhoi ?? string.Empty; // Nếu ad_phanhoi là null, đặt giá trị thành chuỗi rỗng

                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    string query = "INSERT INTO gop_y (makh, chu_de, noi_dung, ngay, trang_thai, ad_phanhoi) VALUES (@makh, @chu_de, @noi_dung, @ngay, @trang_thai, @ad_phanhoi)";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@makh", r.makh);
                        cmd.Parameters.AddWithValue("@chu_de", r.chu_de);
                        cmd.Parameters.AddWithValue("@noi_dung", r.noi_dung);
                        cmd.Parameters.AddWithValue("@ngay", r.ngay);
                        cmd.Parameters.AddWithValue("@trang_thai", r.trang_thai);
                        cmd.Parameters.AddWithValue("@ad_phanhoi", r.ad_phanhoi);

                        cmd.ExecuteNonQuery();
                    }
                }

                ViewBag.MessageSent = true;
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
    }
}
