using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;


namespace project.Models
{
    public class SmsService
    {
        public static async Task<string> SendOtpAsync(string phoneNumber)
        {
            var options = new RestClientOptions("https://qde6yw.api.infobip.com")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/sms/2/text/advanced", Method.Post);

            // Tạo mã OTP 4 chữ số ngẫu nhiên
            Random random = new Random();
            string otpCode = random.Next(1000, 9999).ToString();

            // Thiết lập các header
            request.AddHeader("Authorization", "App 623bbd10760caa7c7c45bf05459ccfbc-9f1c6dfe-273d-4fec-9c8b-a74833716db7");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");

            // Tạo nội dung tin nhắn (gửi mã OTP)
            var body = $@"
        {{
            ""messages"":[ {{
                ""destinations"":[{{""to"":""{phoneNumber}""}}],
                ""from"":""SWE"",
                ""text"":""Mã OTP của bạn là {otpCode}. Vui lòng nhập mã này để xác minh.""
            }} ]
        }}";

            // Gửi yêu cầu
            request.AddStringBody(body, DataFormat.Json);
            RestResponse response = await client.ExecuteAsync(request);

            Console.WriteLine(response.Content);

            // Trả về mã OTP để lưu trữ trong hệ thống hoặc so sánh sau
            return otpCode;
        }
    }
}