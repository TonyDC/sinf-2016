using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class PedidoTransformacaoECL {

        private string numeroDocumento;
        private string filial;
        private string serie;

        public string NumeroDocumento { get { return this.numeroDocumento; } }
        public string Filial { get { return this.filial; } }
        public string Serie { get { return this.serie; } }

        public PedidoTransformacaoECL(string nDoc, string serie, string filial = GeneralConstants.FILIAL_POR_OMISSAO) {
            this.numeroDocumento = nDoc;
            this.filial = filial;
            this.serie = serie;
        }
    }

}