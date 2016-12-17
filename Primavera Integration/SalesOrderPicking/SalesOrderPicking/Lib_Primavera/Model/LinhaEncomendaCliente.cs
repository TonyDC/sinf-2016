using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {
    
    public class LinhaEncomendaCliente {

        private string linhaID;
        private string artigoID;
        private string descricaoArtigo;
        private string armazem;
        private string localizacao;
        private string lote;
        private double quantidade;
        private double quantidadeSatisfeita;
        private uint ordemNaEncomenda;
        private DateTime dataEntrega;

        public string LinhaID { get { return this.linhaID; } }
        public string ArtigoID { get { return this.artigoID; } }
        public string DescricaoArtigo { get { return this.descricaoArtigo; } }
        public string Armazem { get { return this.armazem; } }
        public string Localizacao { get { return this.localizacao; } }
        public string Lote { get { return this.lote; } }
        public double Quantidade { get { return this.quantidade; } }
        public double QuantidadeSatisfeita { get { return this.quantidadeSatisfeita; } }
        public uint OrdemNaEncomenda { get { return this.ordemNaEncomenda; } }
        public DateTime DataEntrega { get { return this.dataEntrega; } }

        public LinhaEncomendaCliente(string linhaID, string artigoID, string descricaoArtigo, string armazem, string localizacao, string lote, double quantidade, double quantidadeSatisfeita, uint ordemNaEncomenda, DateTime dataEntrega) {
            this.linhaID = linhaID;
            this.artigoID = artigoID;
            this.descricaoArtigo = descricaoArtigo;
            this.armazem = armazem;
            this.localizacao = localizacao;
            this.lote = lote;
            this.quantidade = quantidade;
            this.quantidadeSatisfeita = quantidadeSatisfeita;
            this.ordemNaEncomenda = ordemNaEncomenda;
            this.dataEntrega = dataEntrega;
        }
    }
}