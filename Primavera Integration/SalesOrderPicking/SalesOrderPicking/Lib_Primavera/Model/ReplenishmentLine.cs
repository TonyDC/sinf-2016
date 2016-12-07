using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class ReplenishmentLine {

        private string artigo;
        private int quantATransferir;
        private string localizacaoOrigem;
        private string localizacaoDestino;
        private string unidades;

        public string Artigo { get { return this.artigo; } }
        public int QuantATransferir { get { return this.quantATransferir; } }
        public string LocalizacaoOrigem { get {return this.localizacaoOrigem; }}
        public string LocalizacaoDestino { get { return this.localizacaoDestino; } }
        public string Unidades { get { return this.unidades; } }

        public ReplenishmentLine(string artigo, int quantATransferir, string unidades, string localizacaoOrigem, string localizacaoDestino) {
            this.artigo = artigo;
            this.quantATransferir = quantATransferir;
            this.unidades = unidades;
            this.localizacaoOrigem = localizacaoOrigem;
            this.localizacaoDestino = localizacaoDestino;
        }

    }
}