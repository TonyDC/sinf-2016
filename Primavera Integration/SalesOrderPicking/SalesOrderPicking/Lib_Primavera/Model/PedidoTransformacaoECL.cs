using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class PedidoTransformacaoECL {

        private uint numeroDocumento;
        private string filial;
        private string serie;

        [Required]
        public uint NumeroDocumento { get { return this.numeroDocumento; } }
        [Required]
        public string Filial { get { return this.filial; } }
        [Required]
        public string Serie { get { return this.serie; } }

        public PedidoTransformacaoECL(uint nDoc, string serie, string filial = GeneralConstants.FILIAL_POR_OMISSAO) {
            this.numeroDocumento = nDoc;
            this.filial = filial;
            this.serie = serie;
        }
    }

}