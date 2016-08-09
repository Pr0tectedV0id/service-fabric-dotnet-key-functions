using System.Web.Http;
using Owin;
using Microsoft.Owin.Cors;


namespace WebApi1
{
    public static class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            #region Cors

            appBuilder.UseCors(CorsOptions.AllowAll);

            #endregion
            appBuilder.UseWebApi(config);
        }
    }
}
