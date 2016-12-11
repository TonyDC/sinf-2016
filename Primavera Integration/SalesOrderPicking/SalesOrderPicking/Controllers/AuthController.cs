using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SalesOrderPicking.Controllers {

    public class AuthController : ApiController {

        [Route("api/auth/register/worker")]
        public IHttpActionResult PostNewWorker(Dictionary<string, string> body) {

            if (!body.ContainsKey("username") || !body.ContainsKey("password"))
                return BadRequest("In order to register a user, one must provide 'username' and 'password");

            int userID;
            try {
                userID = SalesOrderPicking.Auth.Auth.RegisterWorker(body["username"], body["password"]);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            Dictionary<string, object> response = new Dictionary<string, object>();
            response.Add("user", userID);

            return Ok(response);
        }


        [Route("api/auth/register/manager")]
        public IHttpActionResult PostNewManager(Dictionary<string, string> body) {

            if (!body.ContainsKey("username") || !body.ContainsKey("password"))
                return BadRequest("In order to register a user, one must provide 'username' and 'password");

            int userID;
            try {
                userID = SalesOrderPicking.Auth.Auth.RegisterManager(body["username"], body["password"]);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            Dictionary<string, object> response = new Dictionary<string, object>();
            response.Add("user", userID);

            return Ok(response);
        }


        [Route("api/auth/login/worker")]
        public IHttpActionResult LoginWorker(Dictionary<string, string> body) {

            if (!body.ContainsKey("username") || !body.ContainsKey("password"))
                return BadRequest("In order to register a user, one must provide 'username' and 'password");

            bool loggedIn = false;
            try {
                loggedIn = SalesOrderPicking.Auth.Auth.LoginWorker(body["username"], body["password"]);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            if (!loggedIn)
                return Unauthorized();
            else
                return Ok();
        }


        [Route("api/auth/login/manager")]
        public IHttpActionResult LoginManager(Dictionary<string, string> body) {

            if (!body.ContainsKey("username") || !body.ContainsKey("password"))
                return BadRequest("In order to register a user, one must provide 'username' and 'password");

            bool loggedIn = false;
            try {
                loggedIn = SalesOrderPicking.Auth.Auth.LoginManager(body["username"], body["password"]);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            if (!loggedIn)
                return Unauthorized();
            else
                return Ok();
        }

    }

}
