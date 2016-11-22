using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class Artigo {

        private string codArtigo;
        private string descArtigo;
        private string unidadeVenda;
        private List<StockArtigo> listaStock;

        public string CodArtigo { get { return codArtigo; } }
        public string DescArtigo { get { return descArtigo; } }
        public string UnidadeVenda { get { return this.unidadeVenda; } }
        public List<StockArtigo> ListaStock { get { return this.listaStock; } }

        public Artigo(string cod, string descricao, string unidadeVenda = null, List<StockArtigo> listaStock = null) {
            this.codArtigo = cod;
            this.descArtigo = descricao;
            this.unidadeVenda = unidadeVenda;
            this.listaStock = listaStock;
        }

        public bool ShouldSerializeListaStock() {
            return (this.ListaStock != null);
        }

        public bool ShouldSerializeUnidadeVenda() {
            return (this.UnidadeVenda != null);
        }
    }
}