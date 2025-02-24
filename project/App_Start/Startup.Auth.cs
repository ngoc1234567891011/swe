using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Owin.Cors;


[assembly: OwinStartup(typeof(project.Startup))]

namespace project
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            // Cấu hình Google Authentication
            
            app.UseCors(CorsOptions.AllowAll); // Cấu hình CORS để cho phép tất cả các domain

        }
    }
}
