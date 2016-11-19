using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SalesOrderPicking.Lib_Primavera;
using SalesOrderPicking.Lib_Primavera.Model;

namespace SalesOrderPicking.Controllers {

    public class ClienteController : ApiController {

        // GET: api/Cliente
        public IHttpActionResult Get() {

            List<Cliente> clientes = PriIntegration.GetListaClientes();

            if (clientes == null)
                return NotFound();

            else
                return Ok(clientes);
        }

        // GET: api/Cliente/5
        public IHttpActionResult Get(string id) {
            Cliente cliente = PriIntegration.GetClienteInfo(id);

            if (cliente == null)
                return NotFound();

            else
                return Ok(cliente);
        }

        // POST: api/Cliente
        public void Post([FromBody]string value) {
        }

        // PUT: api/Cliente/5
        public void Put(int id, [FromBody]string value) {
        }

        // DELETE: api/Cliente/5
        public void Delete(int id) {
        }
    }
}
