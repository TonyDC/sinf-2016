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
        private string cliente;
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

        public string Cliente {
            get {
                return this.cliente;
            }
        }

        public EncomendaCliente(string id, string numeroDocumento, string cliente, string serie, string filial = null, List<LinhaEncomendaCliente> artigos = null) {
            this.encomendaID = id;
            this.numeroDocumento = numeroDocumento;
            this.filial = filial?? GeneralConstants.FILIAL_POR_OMISSAO;
            this.serie = serie;
            this.cliente = cliente;
            this.artigos = artigos;
        }

        public bool ShouldSerializeArtigos() {
            return this.Artigos != null;
        }

    }
}