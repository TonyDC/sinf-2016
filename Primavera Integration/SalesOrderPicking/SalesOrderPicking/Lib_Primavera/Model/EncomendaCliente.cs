using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class EncomendaCliente {

        private string encomendaID;
        private uint numeroDocumento;
        private string filial;
        private string serie;
        private string cliente;
        private DateTime? dataMinimaEncomenda;
        private List<LinhaEncomendaCliente> artigos;

        public string ID { get { return this.encomendaID; } }
        public List<LinhaEncomendaCliente> Artigos { get { return this.artigos; } }
        public uint NumeroDocumento { get { return this.numeroDocumento; } }
        public string Filial { get { return this.filial; } }
        public string Serie { get { return this.serie; } }
        public string Cliente { get { return this.cliente; } }
        public DateTime? DataMinimaEncomenda { get { return this.dataMinimaEncomenda; } } 

        public EncomendaCliente(string id, uint numeroDocumento, string cliente, string serie, string filial = null, List<LinhaEncomendaCliente> artigos = null) {
            this.encomendaID = id;
            this.numeroDocumento = numeroDocumento;
            this.filial = filial?? GeneralConstants.FILIAL_POR_OMISSAO;
            this.serie = serie;
            this.cliente = cliente;
            this.artigos = artigos;

            if (artigos != null) {
                // Buscar a data de entrega mais próxima
                DateTime minDate = DateTime.MaxValue;

                foreach (LinhaEncomendaCliente item in artigos) {
                    if (DateTime.Compare(item.DataEntrega, minDate) < 0)
                        minDate = item.DataEntrega;
                }

                dataMinimaEncomenda = minDate;

            } else
                dataMinimaEncomenda = null;
        }

        public bool ShouldSerializeArtigos() {
            return this.Artigos != null;
        }

        public bool ShouldSerializeDataMinimaEncomenda() {
            return this.dataMinimaEncomenda != null;
        }

    }
}