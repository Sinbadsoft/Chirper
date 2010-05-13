using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using HectorSharp;

namespace JavaGeneration.Chirper
{
    public class MvcApplication : HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default",                                              // Route name
                "{controller}/{action}/{id}",                           // URL with parameters
                new { controller = "Home", action = "Index", id = string.Empty }  // Parameter defaults
                );

        }

        protected void Application_Start()
        {
            RegisterRoutes(RouteTable.Routes);

            // TODO(cnakhli) Cassabdra config should be read from Web.config
            CassandraClients.Factory = new KeyedCassandraClientFactory(
                new CassandraClientPoolFactory().Create(), 
                new KeyedCassandraClientFactory.Config());
            CassandraClients.Endpoint = new Endpoint("localhost", 9160);
        }
    }
}