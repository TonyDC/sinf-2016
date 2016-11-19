using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class Artigo {

        private string codArtigo;
        private string descArtigo;
        private List<StockArtigo> listaStock;

        public string CodArtigo {
            get {
                return codArtigo;
            }
        }

        public string DescArtigo {
            get {
                return descArtigo;
            }
        }

        public List<StockArtigo> ListaStock { get { return this.listaStock; } }

        public Artigo(string cod, string descricao, List<StockArtigo> listaStock = null) {
            this.codArtigo = cod;
            this.descArtigo = descricao;
            this.listaStock = listaStock;
        }

        public bool ShouldSerializeListaStock() {
            return (this.ListaStock != null);
        }

    }
}