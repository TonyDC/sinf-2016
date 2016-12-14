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

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

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

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception) {
                return InternalServerError();
            }

            if (cliente == null)
                return NotFound();
            else
                return Ok(cliente);
        }

        // GET: api/cliente/{cliente-id}/encomenda/{filial}/{serie}
        [Route("api/cliente/{id}/encomenda/{filial}/{serie}")]
        public IHttpActionResult GetEncomendaByCliente(string id, string filial, string serie) {
            List<EncomendaCliente> encomendasCliente = null;
            try {
                encomendasCliente = PriIntegration.GetEncomendasClientes(filial, serie, id);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception) {
                return InternalServerError();
            }

            if (encomendasCliente == null || encomendasCliente.Count < 1)
                return NotFound();
            else
                return Ok(encomendasCliente);
        }

        // GET: api/cliente/{cliente-id}/encomenda/{filial}/{serie}/{n}
        [Route("api/cliente/{id}/encomenda/{filial}/{serie}/{n:int:min(1)}")]
        public IHttpActionResult GetEncomendaByClienteAndNumber(string id, string filial, string serie, int n) {
            List<EncomendaCliente> encomendas = null;
            try {
                encomendas = PriIntegration.GetEncomendasClientes(filial, serie, id, n);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            if (encomendas == null || encomendas.Count < 1)
                return NotFound();
            else
                return Ok(encomendas);
        }

        // GET: api/encomenda/{filial}/{serie}
        [Route("api/encomenda/{filial}/{serie}")]
        public IHttpActionResult GetEncomendas(string filial, string serie) {
            List<EncomendaCliente> encomendas = null;
            try {
                encomendas = PriIntegration.GetEncomendasClientes(filial, serie);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            if (encomendas == null || encomendas.Count < 1)
                return NotFound();
            else
                return Ok(encomendas);
        }

        // GET: api/encomenda/{filial}/{serie}
        [Route("api/encomenda/{filial}/{serie}/order/date/{desc:bool=false}")]
        public IHttpActionResult GetEncomendasOrdenadasPorDataMinima(string filial, string serie, bool desc) {
            List<EncomendaCliente> encomendas = null;
            try {
                encomendas = PriIntegration.GetEncomendasClientesPorOrdenacao(filial, serie, true, desc, false, false);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            if (encomendas == null || encomendas.Count < 1)
                return NotFound();
            else
                return Ok(encomendas);
        }


        // GET: api/encomenda/{filial}/{serie}
        [Route("api/encomenda/{filial}/{serie}/order/cliente/{desc:bool=false}")]
        public IHttpActionResult GetEncomendasOrdenadasPorCliente(string filial, string serie, bool desc) {
            List<EncomendaCliente> encomendas = null;
            try {
                encomendas = PriIntegration.GetEncomendasClientesPorOrdenacao(filial, serie, false, false, true, desc);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            if (encomendas == null || encomendas.Count < 1)
                return NotFound();
            else
                return Ok(encomendas);
        }

        // GET: api/encomenda/{filial}/{serie}
        [Route("api/encomenda/{filial}/{serie}/order/date/{descDate:bool}/cliente/{descCliente:bool}")]
        public IHttpActionResult GetEncomendasOrdenadasPorDataECliente(string filial, string serie, bool descDate, bool descCliente) {
            List<EncomendaCliente> encomendas = null;
            try {
                encomendas = PriIntegration.GetEncomendasClientesPorOrdenacao(filial, serie, true, descDate, true, descCliente);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            if (encomendas == null || encomendas.Count < 1)
                return NotFound();
            else
                return Ok(encomendas);
        }


        // GET: api/encomenda/{filial}/{serie}/{n}
        [Route("api/encomenda/{filial}/{serie}/{n:int:min(1)}")]
        public IHttpActionResult GetEncomendaByNumDoc(string filial, string serie, int n) {
            List<EncomendaCliente> encomendas = null;
            try {
                encomendas = PriIntegration.GetEncomendasClientes(filial, serie, null, n);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            if (encomendas == null || encomendas.Count < 1)
                return NotFound();
            else
                return Ok(encomendas);
        }

        [Route("api/encomenda-pronto/{filial}/{serie}")]
        public IHttpActionResult GetEncomendasProntas(string filial, string serie) {
            List<EncomendaCliente> encomendas = null;
            try {
                encomendas = PriIntegration.GetEncomendasPassiveisDeTransformacao(filial, serie);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            if (encomendas == null || encomendas.Count < 1)
                return NotFound();
            else
                return Ok(encomendas);
        }

        // POST: api/encomendas
        /*
         * {
         *      "nDoc": uint,
         *      "serie": string,
         *      "filial": string
         * }
         */
        [Route("api/encomenda")]
        public IHttpActionResult PostEncomendas(PedidoTransformacaoECL encomenda) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try {
                if (PriIntegration.GerarGuiaRemessa(encomenda))
                    return Ok();
                else
                    return BadRequest();

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }
 
        }
    }
}
