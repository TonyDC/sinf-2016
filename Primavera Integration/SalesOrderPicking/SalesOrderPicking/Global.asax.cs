using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using SalesOrderPicking.Lib_Primavera.Model;

namespace SalesOrderPicking {

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication {

        protected void Application_Start() {

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();

            // ------------------------------------------------------------------------
            // Inicializar o Engine do Primavera

            string company = SalesOrderPicking.Properties.Settings.Default.Company.Trim(),
                    username = SalesOrderPicking.Properties.Settings.Default.User.Trim(),
                    password = SalesOrderPicking.Properties.Settings.Default.Password.Trim();

            bool isInitialised = SalesOrderPicking.Lib_Primavera.PriEngine.InitializeCompany(company, username, password);
            if (!isInitialised)
                throw new HttpResponseException(System.Net.HttpStatusCode.InternalServerError);
        }
    }
}