using System;
using System.Web;
using System.Web.Http;

namespace Wayfinder.API
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            
            // Log the error (in production, use a proper logging framework)
            System.Diagnostics.Debug.WriteLine($"Unhandled exception: {exception?.Message}");
            
            // Clear the error
            Server.ClearError();
            
            // Return a generic error response
            Response.StatusCode = 500;
            Response.ContentType = "application/json";
            Response.Write("{\"error\":\"An internal server error occurred.\"}");
            Response.End();
        }
    }
}
