using System.Web.Http;
using Wayfinder.API.Filters;

namespace Wayfinder.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            
            // Enable attribute routing
            config.MapHttpAttributeRoutes();

            // Convention-based routing
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Configure JSON serialization
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = 
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = 
                Newtonsoft.Json.DateTimeZoneHandling.Utc;
            
            // Remove XML formatter to return JSON by default
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
