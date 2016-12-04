using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class Resposta {

        private string mensagem;

        public string Mensagem { get { return this.mensagem; } }

        public Resposta(string mensagem) {
            this.mensagem = mensagem;
        }
    }
}