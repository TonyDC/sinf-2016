using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class TransferenciaArtigo {

        private string artigoID;
        private string localizacaoOrigem;
        private string armazemDestino;
        private string localizacaoDestino;
        private double quantidade;

        [Required]
        public string Artigo { get { return this.artigoID; } }

        [Required]
        public string LocalizacaoOrigem { get { return this.localizacaoOrigem; } }

        [Required]
        public string LocalizacaoDestino { get { return this.localizacaoDestino; } }

        [Required]
        public string ArmazemDestino { get { return this.armazemDestino; } }

        [Required]
        [Range(0, double.MaxValue)]
        public double Quantidade { get { return this.quantidade; } }

        public TransferenciaArtigo(string artigo, string localizacaoOrigem, string localizacaoDestino, string armazemDestino, double quantidade) {
            this.artigoID = artigo;
            this.localizacaoOrigem = localizacaoOrigem;
            this.localizacaoDestino = localizacaoDestino;
            this.armazemDestino = armazemDestino;
            this.quantidade = quantidade;
        }

        public override string ToString() {
            StringBuilder str = new StringBuilder();

            str.Append("Artigo: ").AppendLine(artigoID);
            str.Append("Localização Origem: ").AppendLine(localizacaoOrigem);
            str.Append("Armazém Destino: ").AppendLine(armazemDestino);
            str.Append("Localização Destino: ").AppendLine(localizacaoDestino);
            str.Append("Quantidade: ").AppendLine(quantidade.ToString());

            return str.ToString();
        }
    }

}