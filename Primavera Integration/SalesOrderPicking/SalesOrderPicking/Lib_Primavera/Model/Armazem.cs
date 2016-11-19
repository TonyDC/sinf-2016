using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class Armazem {

        private string id;
        private string descricao;

        public string ID { get { return this.id; } }
        public string Descricao { get { return this.descricao; } }

        public Armazem(string id, string descricao) {
            this.id = id;
            this.descricao = descricao;
        }
    }
}