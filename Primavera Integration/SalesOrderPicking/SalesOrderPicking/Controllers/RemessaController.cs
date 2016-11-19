using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SalesOrderPicking.Lib_Primavera;
using SalesOrderPicking.Lib_Primavera.Model;

namespace SalesOrderPicking.Controllers {

    public class RemessaController : ApiController {

        // GET: api/Remessa
        public IEnumerable<string> Get() {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Remessa/5
        public string Get(int id) {
            return "value";
        }

        // POST: api/Remessa
        public IHttpActionResult Post(EncomendaCliente value) {

            if (PriIntegration.GerarGuiaRemessa(value))
                return Ok();
            else
                return BadRequest();

        }

        // PUT: api/Remessa/5
        public void Put(int id, [FromBody]string value) {
        }

        // DELETE: api/Remessa/5
        public void Delete(int id) {
        }
    }
}
