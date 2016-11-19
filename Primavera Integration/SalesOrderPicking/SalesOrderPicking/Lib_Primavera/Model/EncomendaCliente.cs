using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class EncomendaCliente {

        private string encomendaID;
        private string numeroDocumento;
        private string filial;
        private string serie;
        private List<LinhaEncomendaCliente> artigos;

        public string ID {
            get {
                return this.encomendaID;
            }
        }

        public List<LinhaEncomendaCliente> Artigos {
            get {
                return this.artigos;
            }
        }

        public string NumeroDocumento {
            get {
                return this.numeroDocumento;
            }
        }

        public string Filial {
            get {
                return this.filial;
            }
        }

        public string Serie {
            get {
                return this.serie;
            }
        }

        public EncomendaCliente(string id, string numeroDocumento, string filial, string serie, List<LinhaEncomendaCliente> artigos = null) {
            this.encomendaID = id;
            this.numeroDocumento = numeroDocumento;
            this.filial = filial == null ? "000" : filial;
            this.serie = serie;
            this.artigos = artigos;
        }

        public bool ShouldSerializeArtigos() {
            return this.Artigos != null;
        }

    }
}