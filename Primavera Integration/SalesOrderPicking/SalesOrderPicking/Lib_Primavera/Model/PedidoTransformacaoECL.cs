using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class PedidoTransformacaoECL {

        private uint nDoc;
        private string filial;
        private string serie;

        [Required]
        public uint NDoc { get { return this.nDoc; } }
        [Required]
        public string Filial { get { return this.filial; } }
        [Required]
        public string Serie { get { return this.serie; } }

        public PedidoTransformacaoECL(uint nDoc, string serie, string filial = GeneralConstants.FILIAL_POR_OMISSAO) {
            this.nDoc = nDoc;
            this.filial = filial;
            this.serie = serie;
        }
    }

}