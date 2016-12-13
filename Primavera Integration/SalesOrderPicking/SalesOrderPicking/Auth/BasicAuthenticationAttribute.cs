using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Http.Filters;

namespace SalesOrderPicking.Auth {
    // Adapted from: https://stevescodingblog.co.uk/basic-authentication-with-asp-net-webapi/
    public class BasicAuthenticationAttribute : System.Web.Http.Filters.ActionFilterAttribute {
      
        protected string Username { get; set; }
        protected string Password { get; set; }

        public BasicAuthenticationAttribute(string username, string password) {
            this.Username = username;
            this.Password = password;
        }

        public BasicAuthenticationAttribute() {
            this.Username = SalesOrderPicking.Properties.Settings.Default.APIUser.Trim();
            this.Password = SalesOrderPicking.Properties.Settings.Default.APIPassword.Trim();
        }

        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext) {

            if (actionContext.Request.Headers.Authorization == null) {
                actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            
            } else {
                string authToken = actionContext.Request.Headers.Authorization.Parameter;
                string decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(authToken));
                string username = decodedToken.Substring(0, decodedToken.IndexOf(":"));
                string password = decodedToken.Substring(decodedToken.IndexOf(":") + 1);

                if (String.Compare(username, this.Username) != 0 || String.Compare(password, this.Password) != 0)
                    actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                else
                    HttpContext.Current.User = new GenericPrincipal(new ApiIdentity(username), new string[] { });
            }

        }
    }
}