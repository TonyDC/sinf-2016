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

        public const string ARMAZEM_EXPEDICAO = "EXPED";

        public const string QUERY_TESTE1 = "SELECT CabecDoc.* FROM PRIDEMOSINF.dbo.CabecDoc INNER JOIN (" +
                                                "SELECT DISTINCT NumDoc FROM PRIDEMOSINF.dbo.CabecDoc INNER JOIN PRIDEMOSINF.dbo.LinhasDoc ON (CabecDoc.id = LinhasDoc.IdCabecDoc) LEFT JOIN LinhaEncomenda ON (LinhaEncomenda.id_linha = LinhasDoc.id AND LinhaEncomenda.versao_ult_act = sys.fn_varbintohexstr(LinhasDoc.VersaoUltAct)) LEFT JOIN LinhaPicking ON (LinhaEncomenda.id = LinhaPicking.id_linha_encomenda) LEFT JOIN PickingWave ON (LinhaPicking.id_picking = PickingWave.id) WHERE TipoDoc = 'ECL' AND CabecDoc.Serie = @1@ AND Filial = @0@ AND LinhasDoc.Quantidade > 0 AND (LinhaEncomenda.id IS NULL OR LinhaEncomenda.quant_pedida > LinhaEncomenda.quant_satisfeita) AND NumDoc NOT IN (" +
                                                    "SELECT DISTINCT NumDoc FROM PRIDEMOSINF.dbo.CabecDoc INNER JOIN PRIDEMOSINF.dbo.LinhasDoc ON (CabecDoc.id = LinhasDoc.IdCabecDoc) LEFT JOIN LinhaEncomenda ON (LinhaEncomenda.id_linha = LinhasDoc.id) LEFT JOIN LinhaPicking ON (LinhaEncomenda.id = LinhaPicking.id_linha_encomenda) LEFT JOIN PickingWave ON (LinhaPicking.id_picking = PickingWave.id) WHERE LinhaEncomenda.id IS NOT NULL AND (PickingWave.em_progresso = 1 OR LinhaPicking.id_picking IS NULL) AND TipoDoc = 'ECL' AND LinhaEncomenda.Serie = @1@ AND Filial = @0@" +
                                                ")) AS DocumentosValidos ON (CabecDoc.NumDoc = DocumentosValidos.NumDoc) WHERE CabecDoc.TipoDoc = 'ECL' AND CabecDoc.Filial = @0@ AND CabecDoc.Serie = @1@";

        public const string QUERY_TESTE2 = "SELECT CabecDoc.* FROM PRIDEMOSINF.dbo.CabecDoc INNER JOIN (" +
                                                "SELECT DISTINCT NumDoc FROM PRIDEMOSINF.dbo.CabecDoc INNER JOIN PRIDEMOSINF.dbo.LinhasDoc ON (CabecDoc.id = LinhasDoc.IdCabecDoc) INNER JOIN PRIDEMOSINF.dbo.LinhasDocStatus ON (LinhasDoc.Id = LinhasDocStatus.IdLinhasDoc) LEFT JOIN LinhaEncomenda ON (LinhaEncomenda.id_linha = LinhasDoc.id) LEFT JOIN LinhaPicking ON (LinhaEncomenda.id = LinhaPicking.id_linha_encomenda) LEFT JOIN PickingWave ON (LinhaPicking.id_picking = PickingWave.id) WHERE LinhasDoc.Quantidade > 0 AND LinhaEncomenda.id IS NOT NULL AND PickingWave.em_progresso = 0 AND TipoDoc = 'ECL' AND LinhaEncomenda.Serie = @1@ AND Filial = @0@ AND EstadoTrans = 'P' AND NumDoc NOT IN (" +
                                                "SELECT DISTINCT NumDoc FROM PRIDEMOSINF.dbo.CabecDoc INNER JOIN PRIDEMOSINF.dbo.LinhasDoc ON (CabecDoc.id = LinhasDoc.IdCabecDoc) LEFT JOIN LinhaEncomenda ON (LinhaEncomenda.id_linha = LinhasDoc.id) LEFT JOIN LinhaPicking ON (LinhaEncomenda.id = LinhaPicking.id_linha_encomenda) LEFT JOIN PickingWave ON (LinhaPicking.id_picking = PickingWave.id) WHERE LinhaEncomenda.id IS NOT NULL AND (PickingWave.em_progresso = 1 OR LinhaPicking.id_picking IS NULL) AND TipoDoc = 'ECL' AND LinhaEncomenda.Serie = @1@ AND Filial = @0@" +
                                            ")) AS Numeracao ON (Numeracao.NumDoc = CabecDoc.NumDoc) WHERE Filial = @0@ AND Serie = @1@ AND TipoDoc = 'ECL'";

        public const string QUERY_TESTE3 = "SELECT CabecDoc.*, DataMinima FROM PRIDEMOSINF.dbo.CabecDoc INNER JOIN (" +
                                                "SELECT IdCabecDoc, min(DataEntrega) AS DataMinima FROM PRIDEMOSINF.dbo.LinhasDoc INNER JOIN PRIDEMOSINF.dbo.CabecDoc ON (LinhasDoc.IdCabecDoc = CabecDoc.Id) WHERE TipoDoc = 'ECL' AND Serie = @1@ AND Filial = @0@ GROUP BY IdCabecDoc" +
                                            ") AS TabelaDataMinima ON (CabecDoc.Id = TabelaDataMinima.IdCabecDoc)";
    }
}