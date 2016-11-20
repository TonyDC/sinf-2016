using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using SalesOrderPicking.Lib_Primavera;
using SalesOrderPicking.Lib_Primavera.Model;

namespace SalesOrderPicking.Controllers {

    public class TransferenciaController : ApiController {

        // GET: api/Transferencia
        public IEnumerable<string> Get() {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Transferencia/5
        public string Get(int id) {
            return "value";
        }

        // POST: api/Transferencia
        public IHttpActionResult Post(TransferenciaArmazem transferencia) {

            if (PriIntegration.GerarTransferenciaArmazem(transferencia))
                return Ok();
            else
                return BadRequest();
        }

        // PUT: api/Transferencia/5
        public void Put(int id, [FromBody]string value) {
        }

        // DELETE: api/Transferencia/5
        public void Delete(int id) {
        }
    }
}
