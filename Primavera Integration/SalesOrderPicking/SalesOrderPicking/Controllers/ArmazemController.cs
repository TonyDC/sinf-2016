using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using SalesOrderPicking.Lib_Primavera;
using SalesOrderPicking.Lib_Primavera.Model;

namespace SalesOrderPicking.Controllers {

    public class ArmazemController : ApiController {

        // GET: api/armazem
        public IHttpActionResult Get() {
            List<Armazem> listaArmazens = PriIntegration.GetArmazens();

            if (listaArmazens == null)
                return NotFound();
            else
                return Ok(listaArmazens);
        }

        // GET: api/armazem/{armazem-id}
        public IHttpActionResult Get(string id) {
            List<LocalizacaoArmazem> listaLocalizacoes = PriIntegration.GetLocalizacoesArmazens(id);

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
         *              artigo: string,
         *              localizacaoOrigem: string,
         *              localizacaoDestino: string,
         *              armazemDestino: string,
         *              quantidade: string
         */              
        public IHttpActionResult Post(TransferenciaArmazem transferencia) {

            if (PriIntegration.GerarTransferenciaArmazem(transferencia))
                return Ok();
            else
                return BadRequest();
        }
    }
}
