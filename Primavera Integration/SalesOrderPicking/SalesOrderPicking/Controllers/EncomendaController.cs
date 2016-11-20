using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SalesOrderPicking.Lib_Primavera;
using SalesOrderPicking.Lib_Primavera.Model;

namespace SalesOrderPicking.Controllers
{
    public class EncomendaController : ApiController
    {
        // GET: api/Encomenda
        public IHttpActionResult Get()
        {
            List<EncomendaCliente> encomendas = PriIntegration.GetEncomendasClientes();

            if (encomendas == null)
                return NotFound();
            else
                return Ok(encomendas);
        }

        // GET: api/Encomenda/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Encomenda
        public IHttpActionResult Post(PedidoTransformacaoECL encomenda) {

            if (PriIntegration.GerarGuiaRemessa(encomenda))
                return Ok();
            else
                return BadRequest();

        }

        // PUT: api/Encomenda/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Encomenda/5
        public void Delete(int id)
        {
        }
    }
}
