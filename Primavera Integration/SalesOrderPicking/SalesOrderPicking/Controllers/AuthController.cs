using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using SalesOrderPicking.Auth;

namespace SalesOrderPicking.Controllers {

    public class AuthController : ApiController {

        [Route("api/auth/register/worker")]
        public IHttpActionResult PostNewWorker(Dictionary<string, string> body) {

            if (!body.ContainsKey("username") || !body.ContainsKey("password"))
                return BadRequest("In order to register a user, one must provide 'username' and 'password");

            int userID;
            try {
                userID = RegisterWorker(body["username"], body["password"]);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            return Ok(userID);
        }


        [Route("api/auth/register/manager")]
        public IHttpActionResult PostNewWorker(Dictionary<string, string> body) {

            if (!body.ContainsKey("username") || !body.ContainsKey("password"))
                return BadRequest("In order to register a user, one must provide 'username' and 'password");

            int userID;
            try {
                userID = RegisterManager(body["username"], body["password"]);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            return Ok(userID);
        }


        [Route("api/auth/login/worker")]
        public IHttpActionResult PostNewWorker(Dictionary<string, string> body) {

            if (!body.ContainsKey("username") || !body.ContainsKey("password"))
                return BadRequest("In order to register a user, one must provide 'username' and 'password");

            bool loggedIn = false;
            try {
                loggedIn = LoginWorker(body["username"], body["password"]);

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
        public IHttpActionResult PostNewWorker(Dictionary<string, string> body) {

            if (!body.ContainsKey("username") || !body.ContainsKey("password"))
                return BadRequest("In order to register a user, one must provide 'username' and 'password");

            bool loggedIn = false;
            try {
                loggedIn = LoginManager(body["username"], body["password"]);

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
