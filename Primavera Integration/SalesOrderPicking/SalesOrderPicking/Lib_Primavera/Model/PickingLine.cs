using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class PickingLine {

        private string idLinha;
        private string artigo;
        private string descricaoArtigo;
        private int quantPedida;
        private string unidades;

        [JsonProperty("id")]
        public string IDLinha { get { return this.idLinha; } }
        [JsonProperty("artigo")]
        public string Artigo { get { return this.artigo; } }
        [JsonProperty("descricao_artigo")]
        public string DescricaoArtigo { get { return this.descricaoArtigo; } }
        [JsonProperty("quantidade")]
        public int QuantPedida { get { return this.quantPedida; } }
        [JsonProperty("unidade")]
        public string Unidades { get { return this.unidades; } }

        public PickingLine(string idLinha, string artigo, string descricaoArtigo, int quantPedida, string unidades) {
            this.idLinha = idLinha;
            this.artigo = artigo;
            this.descricaoArtigo = descricaoArtigo;
            this.quantPedida = quantPedida;
            this.unidades = unidades;
        }
    }
}