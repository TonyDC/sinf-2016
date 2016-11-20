using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class TransferenciaArmazem {

        private string armazemOrigem;
        private string serie;
        private List<TransferenciaArtigo> artigos;

        public string ArmazemOrigem { get { return this.armazemOrigem; } }
        public string Serie { get { return this.serie; } }
        public List<TransferenciaArtigo> Artigos { get { return this.artigos; } }

        public TransferenciaArmazem(string armazemOrigem, string serie, List<TransferenciaArtigo> artigos) {
            this.armazemOrigem = armazemOrigem;
            this.serie = serie;
            this.artigos = artigos;
        }

        public override string ToString() {
            StringBuilder str = new StringBuilder();

            str.Append("Armazém Origem: ").AppendLine(armazemOrigem);
            str.Append("Série: ").AppendLine(serie);
            str.AppendLine("Artigos: ");

            foreach (var item in artigos) {
                str.AppendLine(item.ToString());
            }

            return str.ToString();
        }

    }

}