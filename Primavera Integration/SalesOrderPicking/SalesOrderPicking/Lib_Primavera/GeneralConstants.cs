using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera {
    
    public static class GeneralConstants {

        public const string GUIA_REMESSA_DOCUMENTO = "GR";
        public const string ENCOMENDA_CLIENTE_DOCUMENTO = "ECL";
        public const string TRANSFERENCIA_ARMAZEM_DOCUMENTO = "TRA";

        public const string FILIAL_POR_OMISSAO = "000";

        public const string QUERY_ENCOMENDAS_PENDENTES = "SELECT * FROM (SELECT distinct numDoc AS Doc FROM LinhASDocStatus INNER JOIN LinhASDoc ON (LinhASDocStatus.IdLinhASDoc = LinhASDoc.Id) INNER JOIN CabecDoc ON (LinhASDoc.IdCabecDoc = cabecDoc.id) WHERE LinhASDocStatus.EstadoTrans = 'P') AS DocPendentes INNER JOIN CabecDoc ON (DocPendentes.Doc = CabecDoc.NumDoc) WHERE CabecDoc.tipoDoc = 'ECL'";
        public const string QUERY_TODAS_ENCOMENDAS = "SELECT * FROM CabecDoc WITH (NOLOCK) WHERE TipoDoc = 'ECL'";

        public const double EPSILON = 1e-3;
    }
}