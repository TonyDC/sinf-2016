using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using SalesOrderPicking.Lib_Primavera;
using SalesOrderPicking.Lib_Primavera.Model;
using SalesOrderPicking.Auth;

namespace SalesOrderPicking.Controllers {

    public class ArmazemController : ApiController {

        // GET: api/armazem
        public IHttpActionResult Get() {
            List<Armazem> listaArmazens = null;
            try {
                listaArmazens = PriIntegration.GetArmazens();

            } catch (Exception) {
                return InternalServerError();
            }

            if (listaArmazens == null)
                return NotFound();
            else
                return Ok(listaArmazens);
        }

        // GET: api/armazem/{armazem-id}
        public IHttpActionResult Get(string id) {
            List<LocalizacaoArmazem> listaLocalizacoes = null;
            try {
                listaLocalizacoes = PriIntegration.GetLocalizacoesArmazens(id);

            } catch(InvalidOperationException e) {
                return BadRequest(e.Message);

            } catch (Exception) {
                return InternalServerError();
            }

            if (listaLocalizacoes == null)
                return NotFound();
            else
                return Ok(listaLocalizacoes);
        }

        // POST: api/armazem
        /*
         * Body: Object
         *          armazemOrigem: string,
         *          serie: string,
         *          artigos: Array
         *              Object
         *                  artigo: string,
         *                  localizacaoOrigem: string,
         *                  localizacaoDestino: string,
         *                  armazemDestino: string,
         *                  quantidade: uint
         */
        public IHttpActionResult Post(TransferenciaArmazem transferencia) {
     
            try {
                if (PriIntegration.GerarTransferenciaArmazem(transferencia))
                    return Ok();
                else
                    return BadRequest();

            } catch (Exception) {
                return InternalServerError();
            }
        }
    }
}
