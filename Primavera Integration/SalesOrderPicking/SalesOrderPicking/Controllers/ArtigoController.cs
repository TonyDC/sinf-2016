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
        public IHttpActionResult Get() {
            IEnumerable<Artigo> listaFinal = null;
            try {
                listaFinal = PriIntegration.ListaArtigos();

            } catch (Exception) {
                return InternalServerError();
            }

            if (listaFinal == null)
                return BadRequest();
            else
                return Ok(PriIntegration.ListaArtigos());
        }

        // GET api/artigo/{artigo-id}
        public IHttpActionResult Get(string id) {
            Artigo artigo = null;
            try {
                artigo = PriIntegration.ObterArtigo(id);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            if (artigo == null)
                return NotFound();
            else
                return Ok(artigo);
        }
    }
}
