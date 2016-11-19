using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {
    
    public class LinhaEncomendaCliente {

        private string linhaID;
        private string artigoID;
        private string quantidade;
        private string ordemNaEncomenda;

        public string LinhaID { get { return this.linhaID; } }
        public string ArtigoID { get { return this.artigoID; } }
        public string Quantidade { get { return this.quantidade; } }
        public string OrdemNaEncomenda { get { return this.ordemNaEncomenda; } }

        public LinhaEncomendaCliente(string linhaID, string artigoID, string quantidade, string ordemNaEncomenda) {
            this.linhaID = linhaID;
            this.artigoID = artigoID;
            this.quantidade = quantidade;
            this.ordemNaEncomenda = ordemNaEncomenda;
        }
    }
}