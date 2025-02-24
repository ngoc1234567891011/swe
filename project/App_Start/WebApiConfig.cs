using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace project.App_Start
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Enable CORS for all domains
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));

        }
    }
}