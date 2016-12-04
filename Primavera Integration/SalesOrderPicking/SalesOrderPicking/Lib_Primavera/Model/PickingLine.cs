using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class PickingLine {

        private string artigo;
        private int quantPedida;
        private string unidades;

        public string Artigo { get { return this.artigo; } }
        public int QuantPedida { get { return this.quantPedida; } }
        public string Unidades { get { return this.unidades; } }

        public PickingLine(string artigo, int quantPedida, string unidades) {
            this.artigo = artigo;
            this.quantPedida = quantPedida;
            this.unidades = unidades;
        }
    }
}