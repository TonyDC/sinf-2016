using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class LocalizacaoArmazem {

        private string id;
        private string localizacao;
        private string descricao;
        private string nomeNivel;

        public string ID { get { return this.id; } }
        public string Localizacao { get { return this.localizacao; } }
        public string Descricao { get { return this.descricao; } }
        public string NomeNivel { get { return this.nomeNivel; } }

        public LocalizacaoArmazem(string id, string localizacao, string descricao, string nomeNivel) {
            this.id = id;
            this.localizacao = localizacao;
            this.descricao = descricao;
            this.nomeNivel = nomeNivel;
        }

    }
}