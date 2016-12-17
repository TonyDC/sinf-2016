using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class ReplenishmentLine {

        private string id;
        private string artigo;
        private int quantATransferir;
        private string localizacaoOrigem;
        private string localizacaoDestino;
        private string unidades;
        private string descricao;

        [JsonProperty("id")]
        public string IDLinha { get { return this.id; } }
        [JsonProperty("artigo")]
        public string Artigo { get { return this.artigo; } }
        [JsonProperty("quantidade")]
        public int QuantATransferir { get { return this.quantATransferir; } }
        [JsonProperty("origem")]
        public string LocalizacaoOrigem { get {return this.localizacaoOrigem; } }
        [JsonProperty("destino")]
        public string LocalizacaoDestino { get { return this.localizacaoDestino; } }
        [JsonProperty("unidade")]
        public string Unidades { get { return this.unidades; } }
        [JsonProperty("descricao_artigo")]
        public string Descricao { get { return this.descricao; } }

        public ReplenishmentLine(string id, string artigo, string descricao, int quantATransferir, string unidades, string localizacaoOrigem, string localizacaoDestino) {
            this.id = id;
            this.artigo = artigo;
            this.quantATransferir = quantATransferir;
            this.unidades = unidades;
            this.localizacaoOrigem = localizacaoOrigem;
            this.localizacaoDestino = localizacaoDestino;
            this.descricao = descricao;
        }

    }
}