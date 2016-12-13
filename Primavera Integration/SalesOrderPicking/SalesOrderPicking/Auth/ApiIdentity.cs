using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using System.Web.Providers.Entities;


namespace SalesOrderPicking.Auth {

    public class ApiIdentity : IIdentity {

        public string User {
            get;
            private set;
        }

        public ApiIdentity(string username) {
            if (username == null) throw new ArgumentNullException("Bad username");
            this.User = username;
        }

        public string Name {
            get {
                return this.User;
            }
        }

        public string AuthenticationType {
            get {
                return "Basic";
            }
        }

        public bool IsAuthenticated {
            get {
                return true;
            }
        }
    }
}