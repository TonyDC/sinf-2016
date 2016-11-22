using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class StockArtigo {

        private string armazem;
        private string localizacao;
        private string lote;
        private double stockActual;

        public string Armazem { get { return this.armazem; } }
        public string Localizacao { get { return this.localizacao; } }
        public string Lote { get { return this.lote; } }
        public double StockActual { get { return this.stockActual; } }

        public StockArtigo(string armazem, string localizacao, string lote, double stockActual) {
            this.armazem = armazem;
            this.localizacao = localizacao;
            this.lote = lote;
            this.stockActual = stockActual;
        }

    }
}