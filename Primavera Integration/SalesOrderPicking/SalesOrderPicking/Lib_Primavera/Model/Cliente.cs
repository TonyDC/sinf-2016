using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class Cliente {

        private string clienteID;
        private string nome;
        private string nomeFiscal;
        private string moradaFactura;
        private string localFactura;
        private string codigoPostalFactura;
        private string codigoPostalLocalFactura;
        private string telefoneFactura;
        private string pais;

        public string ID { get { return this.clienteID; } }
        public string Nome { get { return this.nome; } }
        public string NomeFiscal { get { return this.nomeFiscal; } }
        public string MoradaFactura { get { return this.moradaFactura; } }
        public string LocalFactura { get { return this.localFactura; } }
        public string CodigoPostalFactura { get { return this.codigoPostalFactura; } }
        public string CodigoPostalLocalFactura { get { return this.codigoPostalLocalFactura; } }
        public string TelefoneFactura { get { return this.telefoneFactura; } }
        public string Pais { get { return this.pais; } }

        public Cliente(string clienteID, string nome, string nomeFiscal = null, string moradaFactura = null, string localFactura = null, string codigoPostalFactura = null, string codigoPostalLocalFactura = null, string telefoneFactura = null, string pais = null) {
            this.clienteID = clienteID;
            this.nome = nome;
            this.nomeFiscal = nomeFiscal;
            this.moradaFactura = moradaFactura;
            this.localFactura = localFactura;
            this.codigoPostalFactura = codigoPostalFactura;
            this.codigoPostalLocalFactura = codigoPostalLocalFactura;
            this.telefoneFactura = telefoneFactura;
            this.pais = pais;
        }

        public bool ShouldSerializeNomeFiscal() {
            return (this.NomeFiscal != null);
        }

        public bool ShouldSerializeMoradaFactura() {
            return (this.MoradaFactura != null);
        }

        public bool ShouldSerializeLocalFactura() {
            return (this.LocalFactura != null);
        }

        public bool ShouldSerializeCodigoPostalFactura() {
            return (this.CodigoPostalFactura != null);
        }

        public bool ShouldSerializeCodigoPostalLocalFactura() {
            return (this.CodigoPostalLocalFactura != null);
        }

        public bool ShouldSerializeTelefoneFactura() {
            return (this.TelefoneFactura != null);
        }

        public bool ShouldSerializePais() {
            return (this.Pais != null);
        }
    }
}