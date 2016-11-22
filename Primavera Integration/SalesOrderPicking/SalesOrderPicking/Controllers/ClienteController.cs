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

        // GET: api/cliente
        public IHttpActionResult Get() {
            List<Cliente> clientes = null;
            try {
                clientes = PriIntegration.GetListaClientes();

            } catch (Exception) {
                return InternalServerError();
            }

            if (clientes == null)
                return NotFound();
            else
                return Ok(clientes);
        }

        // GET: api/cliente/{cliente-id}
        public IHttpActionResult Get(string id) {
            Cliente cliente = null;
            try {
                cliente = PriIntegration.GetClienteInfo(id);

            } catch (Exception) {
                return InternalServerError();
            }

            if (cliente == null)
                return NotFound();
            else
                return Ok(cliente);
        }

        // GET: api/cliente/{cliente-id}/encomendas
        [Route("api/cliente/{id}/encomendas")]
        public IHttpActionResult GetEncomenda(string id) {
            List<EncomendaCliente> encomendasCliente = null;
            try {
                encomendasCliente = PriIntegration.GetEncomendasClientes(id);

            } catch (Exception) {
                return InternalServerError();
            }

            if (encomendasCliente == null)
                return NotFound();
            else
                return Ok(encomendasCliente);
        }

        // GET: api/clientes/encomendas
        [Route("api/clientes/encomendas")]
        public IHttpActionResult GetEncomendas() {
            List<EncomendaCliente> encomendas = null;
            try {
                PriIntegration.GetEncomendasClientes();

            } catch (Exception) {
                return InternalServerError();
            }

            if (encomendas == null)
                return NotFound();
            else
                return Ok(encomendas);
        }

        // POST: api/clientes/encomendas
        /*
         * Body: Object
         *          nDoc: string,
         *          serie: string,
         *          filial: string
         */
        [Route("api/clientes/encomendas")]
        public IHttpActionResult PostEncomendas(PedidoTransformacaoECL encomenda) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try {
                if (PriIntegration.GerarGuiaRemessa(encomenda))
                    return Ok();
                else
                    return BadRequest();

            } catch (Exception) {
                return InternalServerError();
            }
 
        }
    }
}
