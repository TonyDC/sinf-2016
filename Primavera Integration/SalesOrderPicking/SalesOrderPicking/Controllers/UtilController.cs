using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using SalesOrderPicking.Lib_Primavera;
using SalesOrderPicking.Lib_Primavera.Model;
using SalesOrderPicking.Lib_Primavera.Model.Requests;

namespace SalesOrderPicking.Controllers {

    public class UtilController : ApiController {

        [Route("api/util/series")]
        public IHttpActionResult GetSeries() {
            List<string> result = null;
            try {
                result = PriIntegration.GetSeries();

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception) {
                return InternalServerError();
            }


            if (result == null)
                return NotFound();

            else
                return Ok(result);
        }

        [Route("api/util/filiais")]
        public IHttpActionResult GetFiliais() {
            List<string> result = null;
            try {
                result = PriIntegration.GetFiliais();

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception) {
                return InternalServerError();
            }


            if (result == null)
                return NotFound();

            else
                return Ok(result);
        }

        [Route("api/definitions/capacidade")]
        public IHttpActionResult GetCapacidadeMaximaFuncionario() {
            int result = -1;
            try {
                result = PriIntegration.GetCapacidadeMaximaFuncionario();

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception) {
                return InternalServerError();
            }


            if (result < 0)
                return InternalServerError();

            else {
                Dictionary<string, int> response = new Dictionary<string, int>();
                response.Add("capacidade", result);

                return Ok(response);
            }

        }

        // POST: api/definitions/capacidade
        /*
         * {
         *      "capacidade": uint
         * }
         */
        [Route("api/definitions/capacidade")]
        public IHttpActionResult PostCapacidadeMaximaFuncionario(Dictionary<string, int> request) {

            if (!request.ContainsKey("capacidade"))
                return BadRequest("A resposta deve conter um objecto com a propriedade 'capacidade'");

            int novaCapacidade = request["capacidade"];

            try {
                PriIntegration.SetCapacidadeMaximaFuncionario(novaCapacidade);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception) {
                return InternalServerError();
            }


            return Ok();
        }


        [Route("api/definitions/armazem-principal")]
        public IHttpActionResult GetArmazemPrincipal() {
            string result = null;
            try {
                result = PriIntegration.GetArmazemPrincipal();

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception) {
                return InternalServerError();
            }


            if (result == null)
                return InternalServerError();

            else {
                Dictionary<string, string> response = new Dictionary<string, string>();
                response.Add("armazem-principal", result);

                return Ok(response);
            }

        }

        // POST: api/definitions/armazem-principal
        /*
         * {
         *      "armazem": string
         * }
         */
        [Route("api/definitions/armazem-principal")]
        public IHttpActionResult PostArmazemPrincipal(Dictionary<string, string> request) {

            if (!request.ContainsKey("armazem"))
                return BadRequest("A resposta deve conter um objecto com a propriedade 'armazem'");

            string novoArmazemPrincipal = request["armazem"];

            try {
                PriIntegration.SetArmazemPrincipal(novoArmazemPrincipal);

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception) {
                return InternalServerError();
            }


            return Ok();
        }


        [Route("api/avisos/existe")]
        public IHttpActionResult GetQuantidadeAvisos() {
            int result = -1;
            try {
                result = PriIntegration.GetNumeroAvisosPorLer();

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception) {
                return InternalServerError();
            }

            if (result < 0)
                return InternalServerError();

            Dictionary<string, int> resultBody = new Dictionary<string, int>();
            resultBody.Add("quantidade", result);

            return Ok(resultBody);
        }


        [Route("api/avisos")]
        public IHttpActionResult GetAvisos() {
            List<string> result = null;
            try {
                result = PriIntegration.GetAvisosPorLer();

            } catch (InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception) {
                return InternalServerError();
            }

            if (result == null)
                return InternalServerError();

            else
                return Ok(result);
        }

    }
}
