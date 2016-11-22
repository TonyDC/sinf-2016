using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {
    
    public class LinhaEncomendaCliente {

        private string linhaID;
        private string artigoID;
        private string armazem;
        private string localizacao;
        private string lote;
        private double quantidade;
        private uint ordemNaEncomenda;

        public string LinhaID { get { return this.linhaID; } }
        public string ArtigoID { get { return this.artigoID; } }
        public string Armazem { get { return this.armazem; } }
        public string Localizacao { get { return this.localizacao; } }
        public string Lote { get { return this.lote; } }
        public double Quantidade { get { return this.quantidade; } }
        public uint OrdemNaEncomenda { get { return this.ordemNaEncomenda; } }

        public LinhaEncomendaCliente(string linhaID, string artigoID, string armazem, string localizacao, string lote, double quantidade, uint ordemNaEncomenda) {
            this.linhaID = linhaID;
            this.artigoID = artigoID;
            this.armazem = armazem;
            this.localizacao = localizacao;
            this.lote = lote;
            this.quantidade = quantidade;
            this.ordemNaEncomenda = ordemNaEncomenda;
        }
    }
}