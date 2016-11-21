using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using SalesOrderPicking.Lib_Primavera;
using SalesOrderPicking.Lib_Primavera.Model;

namespace SalesOrderPicking.Controllers {

    public class ArtigoController : ApiController {

        // GET api/artigo
        public IEnumerable<Artigo> Get() {
            IEnumerable<Artigo> listaFinal = PriIntegration.ListaArtigos();

            if (listaFinal == null)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            else
                return PriIntegration.ListaArtigos();
        }

        // GET api/artigo/{artigo-id}
        public IHttpActionResult Get(string id) {
            Artigo artigo = PriIntegration.ObterArtigo(id);

            if (artigo == null)
                return NotFound();
            else
                return Ok(artigo);
        }
    }
}
