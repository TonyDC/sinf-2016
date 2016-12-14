using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Text;

using Interop.ErpBS900;
using Interop.StdPlatBS900;
using Interop.StdBE900;
using Interop.GcpBE900;
using ADODB;

using SalesOrderPicking.Lib_Primavera.Model;
using SalesOrderPicking.Lib_Primavera;
using SalesOrderPicking.Lib_Primavera.Model.Requests;
using System.Runtime.ExceptionServices;

namespace SalesOrderPicking.Lib_Primavera {

    public class PriIntegration {

        public static int MAX_CAP_FUNCIONARIO = 100;
        public static string ARMAZEM_PRIMARIO = "A1";

        #region Artigo

        public static List<Artigo> ListaArtigos() {

            List<Artigo> listaArtigos = new List<Artigo>();

            List<Dictionary<string, object>> rows = DBQuery.performQuery(PriEngine.DBConnString, "SELECT * FROM Artigo WITH (NOLOCK)");

            foreach (var item in rows) {
                object artigoID = item["Artigo"], descricao = item["Descricao"];

                listaArtigos.Add(new Artigo(artigoID as string, descricao as string));
            }

            return listaArtigos;
        }


        public static Artigo ObterArtigo(string codArtigo) {

            if (PriEngine.DBConnString == null)
                throw new InvalidOperationException("Conexão não inicializada (Connection string is null)");

            else if (codArtigo == null)
                throw new InvalidOperationException("Invalid object code");

            List<Dictionary<string, object>> artigo = DBQuery.performQuery(PriEngine.DBConnString, "SELECT * FROM Artigo WITH (NOLOCK) WHERE Artigo = @0@", codArtigo);
            if (artigo.Count != 1)
                return null;
            //throw new InvalidOperationException("Código de artigo inválido");

            Dictionary<string, object> linhaArtigo = artigo.ElementAt(0);
            object descricao = linhaArtigo["Descricao"], unidadeVenda = linhaArtigo["UnidadeVenda"];

            List<Dictionary<string, object>> stockArtigo = DBQuery.performQuery(PriEngine.DBConnString, "SELECT * FROM ArtigoArmazem WITH (NOLOCK) WHERE Artigo = @0@ ORDER BY StkActual DESC", codArtigo);

            List<StockArtigo> listaStockArtigo = new List<StockArtigo>();

            for (int i = 0; i < stockArtigo.Count; i++) {

                Dictionary<string, object> line = stockArtigo.ElementAt(i);
                object armazem = line["Armazem"], localizacao = line["Localizacao"], lote = line["Lote"], stock = line["StkActual"];

                listaStockArtigo.Add(new StockArtigo(armazem as string, localizacao as string, lote as string, Convert.ToDouble(stock)));
            }

            return new Artigo(codArtigo, descricao as string, unidadeVenda as string, listaStockArtigo);
        }

        #endregion Artigo


        #region Encomendas

        public static List<EncomendaCliente> GetEncomendasClientesPorOrdenacao(string f, string s, bool porDataMinima, bool dataDesc, bool porCliente, bool clienteDesc) {

            if (f == null || s == null)
                throw new InvalidOperationException("Bad arguments: 'filial', 'serie'");

            List<EncomendaCliente> listaArtigos = new List<EncomendaCliente>();
            List<Dictionary<string, object>> listaEncomendas = null;

            string orderQuerySubString = "";

            if (porDataMinima && !porCliente)
                orderQuerySubString = " ORDER BY DataMinima " + (dataDesc ? "DESC" : "");

            else if (!porDataMinima && porCliente)
                orderQuerySubString = " ORDER BY Entidade " + (clienteDesc ? "DESC" : "");

            else if (porDataMinima && porCliente)
                orderQuerySubString = " ORDER BY Entidade " + (clienteDesc ? "DESC" : "") + ", DataMinima " + (dataDesc ? "DESC" : "");


            listaEncomendas = DBQuery.performQuery(PriEngine.PickingDBConnString, GeneralConstants.QUERY_TESTE3 + orderQuerySubString, f, s);

            foreach (var item in listaEncomendas) {
                object encomendaID = item["Id"], filial = item["Filial"], serie = item["Serie"], numDoc = item["NumDoc"], cliente = item["EntidadeFac"];

                List<Dictionary<string, object>> linhasEncomenda = DBQuery.performQuery(PriEngine.DBConnString, "SELECT LinhasDoc.Id, LinhasDoc.Artigo, LinhasDoc.Quantidade AS Quantidade, COALESCE(quant_satisfeita, 0) AS QuantTrans, NumLinha, Armazem, Localizacao, Lote, DataEntrega FROM LinhasDoc WITH (NOLOCK) LEFT JOIN PICKING.dbo.LinhaEncomenda WITH (NOLOCK) ON (LinhaEncomenda.id_linha = LinhasDoc.Id AND sys.fn_varbintohexstr(LinhasDoc.VersaoUltAct) = LinhaEncomenda.versao_ult_act) WHERE LinhasDoc.IdCabecDoc = @0@ AND LinhasDoc.Artigo IS NOT NULL", encomendaID);

                List<LinhaEncomendaCliente> artigosEncomenda = new List<LinhaEncomendaCliente>();

                foreach (var linha in linhasEncomenda) {
                    object linhaID = linha["Id"], artigoID = linha["Artigo"], quantidade = linha["Quantidade"], quantidadeSatisfeita = linha["QuantTrans"], numLinha = linha["NumLinha"], armazem = linha["Armazem"], localizacao = linha["Localizacao"], lote = linha["Lote"], dataEntrega = linha["DataEntrega"];

                    artigosEncomenda.Add(new LinhaEncomendaCliente(linhaID.ToString(), artigoID as string, armazem as string, localizacao as string, lote as string, Convert.ToDouble(quantidade), Convert.ToDouble(quantidadeSatisfeita), Convert.ToUInt32(numLinha), (DateTime)dataEntrega));
                }

                listaArtigos.Add(new EncomendaCliente(encomendaID.ToString(), Convert.ToUInt32(numDoc), cliente as string, serie as string, filial as string, artigosEncomenda));
            }

            return listaArtigos;
        }

        // Vai buscar as encomendas que não estão satisfeitas na totalidade ou que não estão com um processo de picking pendente
        public static List<EncomendaCliente> GetEncomendasClientes(string f, string s, string clienteID = null, int? nDoc = null) {

            if (f == null || s == null)
                throw new InvalidOperationException("Bad arguments: 'filial', 'serie'");


            List<EncomendaCliente> listaArtigos = new List<EncomendaCliente>();
            List<Dictionary<string, object>> listaEncomendas = null;

            if (clienteID == null)
                if (nDoc == null)
                    listaEncomendas = DBQuery.performQuery(PriEngine.PickingDBConnString, GeneralConstants.QUERY_TESTE1 + " ORDER BY CabecDoc.NumDoc", f, s);
                else
                    listaEncomendas = DBQuery.performQuery(PriEngine.PickingDBConnString, GeneralConstants.QUERY_TESTE1 + " AND CabecDoc.NumDoc = @2@", f, s, nDoc);
            else
                if (nDoc == null)
                    listaEncomendas = DBQuery.performQuery(PriEngine.PickingDBConnString, GeneralConstants.QUERY_TESTE1 + " AND EntidadeFac = @2@ ORDER BY CabecDoc.NumDoc", f, s, clienteID);
                else
                    listaEncomendas = DBQuery.performQuery(PriEngine.PickingDBConnString, GeneralConstants.QUERY_TESTE1 + " AND EntidadeFac = @2@ AND CabecDoc.NumDoc = @3@ ORDER BY CabecDoc.NumDoc", f, s, clienteID, nDoc);


            foreach (var item in listaEncomendas) {
                object encomendaID = item["Id"], filial = item["Filial"], serie = item["Serie"], numDoc = item["NumDoc"], cliente = item["EntidadeFac"];

                List<Dictionary<string, object>> linhasEncomenda = DBQuery.performQuery(PriEngine.DBConnString, "SELECT LinhasDoc.Id, LinhasDoc.Artigo, LinhasDoc.Quantidade AS Quantidade, COALESCE(quant_satisfeita, 0) AS QuantTrans, NumLinha, Armazem, Localizacao, Lote, DataEntrega FROM LinhasDoc WITH (NOLOCK) LEFT JOIN PICKING.dbo.LinhaEncomenda WITH (NOLOCK) ON (LinhaEncomenda.id_linha = LinhasDoc.Id AND sys.fn_varbintohexstr(LinhasDoc.VersaoUltAct) = LinhaEncomenda.versao_ult_act) WHERE LinhasDoc.IdCabecDoc = @0@ AND LinhasDoc.Artigo IS NOT NULL", encomendaID);

                List<LinhaEncomendaCliente> artigosEncomenda = new List<LinhaEncomendaCliente>();

                foreach (var linha in linhasEncomenda) {
                    object linhaID = linha["Id"], artigoID = linha["Artigo"], quantidade = linha["Quantidade"], quantidadeSatisfeita = linha["QuantTrans"], numLinha = linha["NumLinha"], armazem = linha["Armazem"], localizacao = linha["Localizacao"], lote = linha["Lote"], dataEntrega = linha["DataEntrega"];

                    artigosEncomenda.Add(new LinhaEncomendaCliente(linhaID.ToString(), artigoID as string, armazem as string, localizacao as string, lote as string, Convert.ToDouble(quantidade), Convert.ToDouble(quantidadeSatisfeita), Convert.ToUInt32(numLinha), (DateTime)dataEntrega));
                }

                listaArtigos.Add(new EncomendaCliente(encomendaID.ToString(), Convert.ToUInt32(numDoc), cliente as string, serie as string, filial as string, artigosEncomenda));
            }

            return listaArtigos;
        }


        public static List<EncomendaCliente> GetEncomendasPassiveisDeTransformacao(string f, string s) {

            if (f == null || s == null)
                throw new InvalidOperationException("Bad arguments: 'filial', 'serie'");


            List<EncomendaCliente> listaArtigos = new List<EncomendaCliente>();
            List<Dictionary<string, object>> listaEncomendas = DBQuery.performQuery(PriEngine.PickingDBConnString, GeneralConstants.QUERY_TESTE2, f, s);

            foreach (var item in listaEncomendas) {
                object encomendaID = item["Id"], filial = item["Filial"], serie = item["Serie"], numDoc = item["NumDoc"], cliente = item["EntidadeFac"];

                List<Dictionary<string, object>> linhasEncomenda = DBQuery.performQuery(PriEngine.DBConnString, "SELECT LinhasDoc.Id, LinhasDoc.Artigo, LinhasDoc.Quantidade AS Quantidade, COALESCE(quant_satisfeita, 0) AS QuantTrans, NumLinha, Armazem, Localizacao, Lote, DataEntrega FROM LinhasDoc WITH (NOLOCK) LEFT JOIN PICKING.dbo.LinhaEncomenda WITH (NOLOCK) ON (LinhaEncomenda.id_linha = LinhasDoc.Id AND sys.fn_varbintohexstr(LinhasDoc.VersaoUltAct) = LinhaEncomenda.versao_ult_act) WHERE LinhasDoc.IdCabecDoc = @0@ AND LinhasDoc.Artigo IS NOT NULL", encomendaID);

                List<LinhaEncomendaCliente> artigosEncomenda = new List<LinhaEncomendaCliente>();

                foreach (var linha in linhasEncomenda) {
                    object linhaID = linha["Id"], artigoID = linha["Artigo"], quantidade = linha["Quantidade"], quantidadeSatisfeita = linha["QuantTrans"], numLinha = linha["NumLinha"], armazem = linha["Armazem"], localizacao = linha["Localizacao"], lote = linha["Lote"], dataEntrega = linha["DataEntrega"];

                    artigosEncomenda.Add(new LinhaEncomendaCliente(linhaID.ToString(), artigoID as string, armazem as string, localizacao as string, lote as string, Convert.ToDouble(quantidade), Convert.ToDouble(quantidadeSatisfeita), Convert.ToUInt32(numLinha), (DateTime)dataEntrega));
                }

                listaArtigos.Add(new EncomendaCliente(encomendaID.ToString(), Convert.ToUInt32(numDoc), cliente as string, serie as string, filial as string, artigosEncomenda));
            }

            return listaArtigos;
        }


        public static bool GerarGuiaRemessa(PedidoTransformacaoECL encomenda) {

            if (encomenda == null)
                throw new InvalidOperationException("Pedido de encomenda inválido (parâmetro a null)");
            
            // Verificar se todas as linhas da encomenda fora satisfeitas na totalidade
            List<Dictionary<string, object>> unfulfilledLines = DBQuery.performQuery(PriEngine.PickingDBConnString,
                "SELECT CabecDoc.id FROM PRIDEMOSINF.dbo.CabecDoc INNER JOIN PRIDEMOSINF.dbo.LinhasDoc ON (CabecDoc.id = LinhasDoc.IdCabecDoc) LEFT JOIN LinhaEncomenda ON (LinhasDoc.id = LinhaEncomenda.id_linha AND sys.fn_varbintohexstr(LinhasDoc.VersaoUltAct) = LinhaEncomenda.versao_ult_act) WHERE CabecDoc.TipoDoc = 'ECL' AND CabecDoc.Filial = @0@ AND CabecDoc.Serie = @1@ AND CabecDoc.NumDoc = @2@ AND (LinhaEncomenda.quant_satisfeita < LinhaEncomenda.quant_pedida OR LinhaEncomenda.id IS NULL) AND LinhasDoc.Quantidade > 0",
                encomenda.Filial, encomenda.Serie, (int)encomenda.NDoc);

            if (unfulfilledLines.Count > 0)
                throw new InvalidOperationException("Só podem ser geradas guias de remessa através da API caso cada linha da encomenda de cliente esteja totalmente satisfeita e ainda não tenha sido sujeita a transformação");
            
            if (!PriEngine.IniciaTransaccao())
                return false;

            GcpBEDocumentoVenda objEncomenda = null;
            try {
                // Carregar encomenda de cliente
                objEncomenda = PriEngine.Engine.Comercial.Vendas.Edita(encomenda.Filial, GeneralConstants.ENCOMENDA_CLIENTE_DOCUMENTO, encomenda.Serie, (int)encomenda.NDoc);

                // A saída de stock é feita a partir do armazém de expedição
                objEncomenda.set_EmModoEdicao(true);
                GcpBELinhasDocumentoVenda linhasObjEncomenda = objEncomenda.get_Linhas();
                for (int i = 1; i <= linhasObjEncomenda.NumItens; i++) {
                    GcpBELinhaDocumentoVenda linha = linhasObjEncomenda[i];

                    linha.set_Armazem("EXPED");
                    linha.set_Localizacao("EXPED");
                }

                PriEngine.Engine.Comercial.Vendas.Actualiza(objEncomenda);
                objEncomenda.set_EmModoEdicao(false);
                PriEngine.TerminaTransaccao();

            } catch (Exception) {
                PriEngine.DesfazTransaccao();
                throw;
            }

            // ------------------------------------------------------------------------------------

            if (objEncomenda == null || !PriEngine.IniciaTransaccao())
                return false;

            try {
                objEncomenda = PriEngine.Engine.Comercial.Vendas.Edita(encomenda.Filial, GeneralConstants.ENCOMENDA_CLIENTE_DOCUMENTO, encomenda.Serie, (int)encomenda.NDoc);
                if (objEncomenda == null) {
                    PriEngine.TerminaTransaccao();
                    throw new InvalidOperationException("Não existe uma encomenda de cliente com os dados fornecidos.");
                }

                GcpBEDocumentoVenda objGuiaRemessa = new GcpBEDocumentoVenda();

                objGuiaRemessa.set_TipoEntidade(objEncomenda.get_TipoEntidade());
                objGuiaRemessa.set_Entidade(objEncomenda.get_Entidade());
                objGuiaRemessa.set_Tipodoc(GeneralConstants.GUIA_REMESSA_DOCUMENTO);
                objGuiaRemessa.set_Serie(objEncomenda.get_Serie());

                PriEngine.Engine.Comercial.Vendas.PreencheDadosRelacionados(objGuiaRemessa);

                Array arrayDocs = new GcpBEDocumentoVenda[] { objEncomenda };

                // Limitação da API do Primavera: a encomenda de cliente tem de ser transformada na sua totalidade
                PriEngine.Engine.Comercial.Vendas.TransformaDocumentoEX2(ref arrayDocs, ref objGuiaRemessa, false);

                PriEngine.TerminaTransaccao();

            } catch (Exception) {
                PriEngine.DesfazTransaccao();
                throw;
            }

            return true;
        }

        #endregion Encomendas



        #region Cliente

        public static List<Cliente> GetListaClientes() {

            List<Cliente> listaClientes = new List<Cliente>();
            List<Dictionary<string, object>> queryRows = DBQuery.performQuery(PriEngine.DBConnString, "SELECT * FROM Clientes WITH (NOLOCK)");

            foreach (var item in queryRows) {
                object id = item["Cliente"], nome = item["Nome"];

                listaClientes.Add(new Cliente(id as string, nome as string));
            }

            return listaClientes;
        }


        public static Cliente GetClienteInfo(string clienteID) {

            if (clienteID == null)
                throw new InvalidOperationException("ID de cliente inválido");

            List<Dictionary<string, object>> queryRows = DBQuery.performQuery(PriEngine.DBConnString, "SELECT * FROM Clientes WITH (NOLOCK) WHERE Cliente = @0@", clienteID);

            if (queryRows.Count != 1)
                throw new InvalidOperationException("Cliente inexistente");

            Dictionary<string, object> clientRow = queryRows.ElementAt(0);

            object id = clientRow["Cliente"], nome = clientRow["Nome"], nomeFiscal = clientRow["NomeFiscal"], morada = clientRow["Fac_Mor"], local = clientRow["Fac_Local"], codPostal = clientRow["Fac_Cp"], locCodPostal = clientRow["Fac_Cploc"], telefone = clientRow["Fac_Tel"], pais = clientRow["Pais"];

            return new Cliente(id as string, nome as string, nomeFiscal as string, morada as string, local as string, codPostal as string, locCodPostal as string, telefone as string, pais as string);
        }

        #endregion Cliente


        #region Armazem

        public static List<Armazem> GetArmazens() {

            List<Armazem> listaArmazens = new List<Armazem>();
            List<Dictionary<string, object>> queryRows = DBQuery.performQuery(PriEngine.DBConnString, "SELECT * FROM Armazens WITH (NOLOCK)");

            foreach (var item in queryRows) {
                object id = item["Armazem"], descricao = item["Descricao"];

                listaArmazens.Add(new Armazem(id as string, descricao as string));
            }

            return listaArmazens;
        }

        // Determinar as varias localizacoes associadas ao armazem
        public static List<LocalizacaoArmazem> GetLocalizacoesArmazens(string armazemID) {

            if (armazemID == null)
                throw new InvalidOperationException("Identificador de armazém inválido");

            List<LocalizacaoArmazem> listaLocalizacoesArmazem = new List<LocalizacaoArmazem>();
            List<Dictionary<string, object>> queryRows = DBQuery.performQuery(PriEngine.DBConnString, "SELECT * FROM ArmazemLocalizacoes WITH (NOLOCK) WHERE Armazem = @0@", armazemID);

            foreach (var item in queryRows) {
                object id = item["Id"], localizacao = item["Localizacao"], descricao = item["Descricao"], nomeNivel = item["NomeNivel"];

                listaLocalizacoesArmazem.Add(new LocalizacaoArmazem(id.ToString(), localizacao as string, descricao as string, nomeNivel as string));
            }

            return listaLocalizacoesArmazem;
        }


        public static bool GerarTransferenciaArmazem(TransferenciaArmazem lista) {

            if (!PriEngine.IniciaTransaccao())
                return false;

            try {
                GcpBEDocumentoStock docStock = new GcpBEDocumentoStock();

                docStock.set_Tipodoc(GeneralConstants.TRANSFERENCIA_ARMAZEM_DOCUMENTO);
                docStock.set_Serie(lista.Serie);
                docStock.set_ArmazemOrigem(lista.ArmazemOrigem);

                PriEngine.Engine.Comercial.Stocks.PreencheDadosRelacionados(docStock);

                /*
                 * Forma alternativa
                 * 
                foreach (var item in lista.Artigos) {
                   
                    PriEngine.Engine.Comercial.Stocks.AdicionaLinha(docStock, item.ArtigoID, "", Double.Parse(item.Quantidade), item.ArmazemDestino);
                }*/

                GcpBELinhasDocumentoStock linhasDocStock = new GcpBELinhasDocumentoStock();

                foreach (var item in lista.Artigos) {

                    GcpBELinhaDocumentoStock linhaDocStock = new GcpBELinhaDocumentoStock();

                    linhaDocStock.set_Artigo(item.Artigo);
                    linhaDocStock.set_LocalizacaoOrigem(item.LocalizacaoOrigem);
                    linhaDocStock.set_Localizacao(item.LocalizacaoDestino);
                    linhaDocStock.set_Armazem(item.ArmazemDestino);
                    linhaDocStock.set_Quantidade(item.Quantidade);
                    linhaDocStock.set_DataStock(DateTime.Today);

                    linhasDocStock.Insere(linhaDocStock);
                }

                docStock.set_Linhas(linhasDocStock);

                PriEngine.Engine.Comercial.Stocks.Actualiza(docStock);
                PriEngine.TerminaTransaccao();

            } catch (Exception) {
                PriEngine.DesfazTransaccao();
                throw;
            }

            return true;
        }

        #endregion Armazem

        #region Administracao

        public static List<string> GetSeries() {

            List<Dictionary<string, object>> queryRows = DBQuery.performQuery(PriEngine.DBConnString, "SELECT Serie FROM SeriesVendas WITH (NOLOCK) WHERE TipoDoc = 'ECL'");
            List<string> result = new List<string>();

            foreach (var line in queryRows) {
                object serie = line["Serie"];
                result.Add(serie as string);
            }

            return result;
        }

        public static List<string> GetFiliais() {

            List<Dictionary<string, object>> queryRows = DBQuery.performQuery(PriEngine.DBConnString, "SELECT DISTINCT Filial FROM CabecDoc WHERE TipoDoc = 'ECL'");
            List<string> result = new List<string>();

            foreach (var line in queryRows) {
                object serie = line["Filial"];
                result.Add(serie as string);
            }

            return result;
        }

        #endregion Administracao



        #region Testes

        public static bool RegistarAvisos(List<string> avisos) {

            if (avisos.Count < 1)
                return false;

            StringBuilder queryString = new StringBuilder("INSERT INTO Avisos(mensagem) VALUES ('");
            queryString.Append(avisos.ElementAt(0));
            queryString.Append("')");

            for (int i = 1; i < avisos.Count; i++) {
                queryString.Append(", ('");
                queryString.Append(avisos.ElementAt(i));
                queryString.Append("')");
            }

            DBQuery.performQuery(PriEngine.PickingDBConnString, queryString.ToString());

            return true;
        }




        public static Dictionary<string, int> GetStockActual(string armazem) {
            // TOD refinar numa só query
            List<Dictionary<string, object>> rows = DBQuery.performQuery(PriEngine.DBConnString, "SELECT sum(StkActual) AS Stock, Artigo FROM ArtigoArmazem WHERE Armazem = @0@ GROUP BY Artigo", armazem);
            List<Dictionary<string, object>> reservedStockRows = DBQuery.performQuery(PriEngine.PickingDBConnString, "SELECT * FROM QuantidadeReserva WHERE armazem = @0@", armazem);
            Dictionary<string, int> stock = new Dictionary<string, int>();
            Dictionary<string, int> reservedStock = new Dictionary<string, int>();

            foreach (var item in reservedStockRows) {
                string artigo = item["artigo"] as string;
                int quantidade = Convert.ToInt32(item["quant_reservada"]);

                if (artigo == null)
                    continue;

                else if (reservedStock.ContainsKey(artigo))
                    reservedStock[artigo] += quantidade;

                else
                    reservedStock.Add(artigo, quantidade);
            }


            foreach (var item in rows) {
                string artigo = item["Artigo"] as string;
                int quantidade = Convert.ToInt32(item["Stock"]);

                if (artigo == null)
                    continue;

                if (reservedStock.ContainsKey(artigo)) {
                    quantidade -= reservedStock[artigo];
                    if (quantidade < 0)
                        quantidade = 0;
                }

                if (stock.ContainsKey(artigo))
                    stock[artigo] += quantidade;

                else
                    stock.Add(artigo, quantidade);
            }

            return stock;

            /*
            List<Dictionary<string, object>> rows = Utilities.performQuery(PriEngine.PickingDBConnString, 
                "SELECT StockNoArmazem.Stock - COALESCE(StockReservado.Stock, 0) AS Total, StockNoArmazem.Artigo FROM (SELECT CASE WHEN SUM(StkActual) < 0 THEN 0 ELSE sum(StkActual) END AS Stock, Artigo FROM PRIDEMOSINF.dbo.ArtigoArmazem WHERE Armazem = '@0@' GROUP BY Artigo) AS StockNoArmazem LEFT JOIN (SELECT sum(quant_reservada) AS Stock, Artigo FROM QuantidadeReserva INNER JOIN PRIDEMOSINF.dbo.ArmazemLocalizacoes ON (QuantidadeReserva.localizacao = ArmazemLocalizacoes.Localizacao) WHERE ArmazemLocalizacoes.Armazem = '@0@' GROUP BY Artigo) as StockReservado ON (StockNoArmazem.Artigo = StockReservado.Artigo)", 
                armazem);

            Dictionary<string, int> stock = new Dictionary<string, int>();
            foreach (var tuple in rows) {
                stock.Add(tuple["Artigo"].ToString(), Convert.ToInt32(tuple["Total"]));
            }
            
            return stock;
             * */
        }
        /*
        public static Dictionary<string, int> GetStockActualPorArtigo(string artigo) {


        }
        */



        public static bool GerarPickingOrders(string filial, string serie, List<uint> encomendas) {

            if (filial == null || serie == null || encomendas.Count < 1)
                throw new InvalidOperationException("Bad arguments");

            List<string> listaAvisos = new List<string>();

            // Verificar a existência dos documentos
            StringBuilder conditionQueryString = new StringBuilder("(NumDoc = ");
            conditionQueryString.Append(encomendas.ElementAt(0));
            for (int i = 1; i < encomendas.Count; i++) {
                conditionQueryString.Append(" OR NumDoc = ");
                conditionQueryString.Append(encomendas.ElementAt(i));
            }
            conditionQueryString.Append(")");

            // --------------------------------------------------------------------------------------------------------------
            // Verificar se o armazém principal tem stock suficiente
            Dictionary<string, int> stockActual = GetStockActual(ARMAZEM_PRIMARIO);
            Dictionary<string, int> stockReserva = new Dictionary<string, int>();
            DBQuery dbQuery = new DBQuery(PriEngine.PickingDBConnString);

            try {
                List<Dictionary<string, object>> linhasEncomendas = dbQuery.performQueryWithTransaction(
                        "SELECT LinhasDoc.Id, sys.fn_varbintohexstr(LinhasDoc.VersaoUltAct) as VersaoUltAct, LinhasDoc.Unidade, LinhasDoc.Artigo, LinhasDoc.Quantidade, LinhasDoc.DataEntrega, CabecDoc.Serie, CabecDoc.NumDoc FROM PRIDEMOSINF.dbo.CabecDoc INNER JOIN PRIDEMOSINF.dbo.LinhasDoc ON (CabecDoc.id = LinhasDoc.idCabecDoc) WHERE TipoDoc = 'ECL' AND Quantidade > 0 AND filial = @0@ AND serie = @1@ AND " + conditionQueryString.ToString(),
                        filial, serie);

                if (linhasEncomendas.Count < 1)
                    throw new InvalidOperationException("Não foi possível encontrar as encomendas indicadas");


                foreach (var tuple in linhasEncomendas) {

                    // Verificar se já existe uma linha
                    List<Dictionary<string, object>> linhaEncomenda = dbQuery.performQueryWithTransaction("SELECT * FROM LinhaEncomenda WHERE id_linha = @0@ AND versao_ult_act = @1@", tuple["Id"].ToString(), tuple["VersaoUltAct"].ToString());

                    if (linhaEncomenda.Count > 0) {
                        List<Dictionary<string, object>> linhaPendente = dbQuery.performQueryWithTransaction("SELECT LinhaEncomenda.id FROM LinhaEncomenda INNER JOIN LinhaPicking ON (LinhaEncomenda.id = LinhaPicking.id_linha_encomenda) LEFT JOIN PickingWave ON (LinhaPicking.id_picking = PickingWave.id) WHERE id_linha = @0@ AND versao_ult_act = @1@ AND (id_picking IS NULL OR em_progresso = 1)", tuple["Id"].ToString(), tuple["VersaoUltAct"].ToString());
                        if (linhaPendente.Count > 0)
                            continue;       // Está a decorrer uma picking order a satisfazer esta linha

                        Dictionary<string, object> linha = linhaEncomenda.ElementAt(0);
                        int quantidadePedida = Convert.ToInt32(linha["quant_pedida"]),
                               quantidadeSatisfeita = Convert.ToInt32(linha["quant_satisfeita"]);

                        // A linha da encomenda já foi satisfeita
                        if (quantidadePedida <= quantidadeSatisfeita)
                            continue;


                        string artigo = linha["artigo"] as string;
                        if (!stockActual.ContainsKey(artigo) || stockActual[artigo] <= 0) {
                            listaAvisos.Add("O armazém principal (" + ARMAZEM_PRIMARIO + ") não tem o artigo " + artigo + ", presente no documento nº" + tuple["NumDoc"] + " com a série " + serie);
                            continue;

                        }

                        // Criar uma nova linha picking por forma a satisfazer a diferença
                        int diferenca = quantidadePedida - quantidadeSatisfeita;
                        if (diferenca > stockActual[artigo]) {
                            listaAvisos.Add("Documento (série, número) " + serie + ", " + tuple["NumDoc"] + ": O armazém principal (" + ARMAZEM_PRIMARIO + ") não tem quantidade suficiente para satisfazer o artigo " + artigo + ". Foi gerada uma picking wave capaz de satisfazer a quantidade existente (" + stockActual[artigo] + ").");
                            diferenca = stockActual[artigo];
                        }

                        stockActual[artigo] -= diferenca;

                        if (!stockReserva.ContainsKey(artigo))
                            stockReserva[artigo] = diferenca;
                        else
                            stockReserva[artigo] += diferenca;


                        // A quantidade a satisfazer não pode ultrapassar a capacidade máxima do funcionário
                        while (diferenca > MAX_CAP_FUNCIONARIO) {
                            diferenca -= MAX_CAP_FUNCIONARIO;
                            dbQuery.performQueryWithTransaction("INSERT INTO LinhaPicking(quant_a_satisfazer, artigo, id_linha_encomenda) VALUES(@0@, @1@, @2@)",
                                MAX_CAP_FUNCIONARIO, artigo, linha["id"].ToString());
                        }
                        dbQuery.performQueryWithTransaction("INSERT INTO LinhaPicking(quant_a_satisfazer, artigo, id_linha_encomenda) VALUES(@0@, @1@, @2@)",
                                diferenca, artigo, linha["id"].ToString());

                    } else {

                        int diferenca = Convert.ToInt32(tuple["Quantidade"]);
                        string artigo = tuple["Artigo"] as string;

                        if (!stockActual.ContainsKey(artigo) || stockActual[artigo] <= 0) {
                            listaAvisos.Add("O armazém principal (" + ARMAZEM_PRIMARIO + ") não tem o artigo " + artigo + ", presente no documento nº" + tuple["NumDoc"] + " com a série " + serie);
                            continue;

                        } else if (diferenca > stockActual[artigo]) {
                            listaAvisos.Add("Documento (série, número) " + serie + ", " + tuple["NumDoc"] + ": O armazém principal (" + ARMAZEM_PRIMARIO + ") não tem quantidade suficiente para satisfazer o artigo " + artigo + ". Foi gerada uma picking wave capaz de satisfazer a quantidade existente (" + stockActual[artigo] + ").");
                            diferenca = stockActual[artigo];
                        }

                        stockActual[artigo] -= diferenca;

                        if (!stockReserva.ContainsKey(artigo))
                            stockReserva[artigo] = diferenca;
                        else
                            stockReserva[artigo] += diferenca;

                        DateTime t = (DateTime)tuple["DataEntrega"];

                        // Criar uma nova linha de encomenda
                        List<Dictionary<string, object>> insertResult = dbQuery.performQueryWithTransaction(
                            "INSERT INTO LinhaEncomenda(id_linha, versao_ult_act, artigo, quant_pedida, unidade, data_entrega, serie) OUTPUT INSERTED.id VALUES(@0@, @1@, @2@, @3@, @4@, CONVERT(DATETIME, @5@, 103), @6@)",
                            tuple["Id"].ToString(), tuple["VersaoUltAct"].ToString(), artigo, tuple["Quantidade"], tuple["Unidade"] as string, tuple["DataEntrega"] != null ? tuple["DataEntrega"] : null, tuple["Serie"] as string);

                        if (insertResult.Count < 1)
                            throw new InvalidOperationException("Error in inserting into LinhaEncomenda");


                        while (diferenca > MAX_CAP_FUNCIONARIO) {
                            diferenca -= MAX_CAP_FUNCIONARIO;
                            dbQuery.performQueryWithTransaction(
                                "INSERT INTO LinhaPicking(quant_a_satisfazer, artigo, id_linha_encomenda) VALUES(@0@, @1@, @2@)",
                                MAX_CAP_FUNCIONARIO, artigo, insertResult.ElementAt(0)["id"].ToString());
                        }
                        dbQuery.performQueryWithTransaction(
                                "INSERT INTO LinhaPicking(quant_a_satisfazer, artigo, id_linha_encomenda) VALUES(@0@, @1@, @2@)",
                                diferenca, artigo, insertResult.ElementAt(0)["id"].ToString());
                    }
                }

                // Guardar as quantias reservadas
                foreach (KeyValuePair<string, int> item in stockReserva) {

                    // Verificar se já existe
                    List<Dictionary<string, object>> reservedStockRows = dbQuery.performQueryWithTransaction(
                        "SELECT * FROM QuantidadeReserva WHERE artigo = @0@ AND armazem = @1@", item.Key, ARMAZEM_PRIMARIO);
                    if (reservedStockRows.Count > 0)
                        dbQuery.performQueryWithTransaction(
                       "UPDATE QuantidadeReserva SET quant_reservada = @0@ WHERE id = @1@", (item.Value + Convert.ToInt32(reservedStockRows.ElementAt(0)["quant_reservada"])), reservedStockRows.ElementAt(0)["id"].ToString());
                    else
                        dbQuery.performQueryWithTransaction(
                      "INSERT INTO QuantidadeReserva(artigo, quant_reservada, armazem) VALUES (@0@, @1@, @2@)", item.Key, item.Value, ARMAZEM_PRIMARIO);

                }

                // Registar os avisos
                RegistarAvisos(listaAvisos);

            } catch (Exception e) {
                dbQuery.Rollback();
                ExceptionDispatchInfo.Capture(e).Throw();
                throw;
            }

            dbQuery.Commit();

            return true;
        }






        public static Wave<PickingLine> GetProximaPickingWave(int IDfuncionario) {

            // Verificar se o funcionário existe no sistema
            List<Dictionary<string, object>> workerRows = DBQuery.performQuery(PriEngine.PickingDBConnString, "SELECT * FROM Funcionario WHERE id = @0@", IDfuncionario.ToString());
            if (workerRows.Count != 1)
                throw new InvalidOperationException("There is no such worker");

            // O funcionário não pode ter waves a decorrer
            if (AreWavesPendentes(IDfuncionario))
                throw new InvalidOperationException("The worker has pending waves");

            DBQuery dbQuery = new DBQuery(PriEngine.PickingDBConnString);

            // Buscar as picking lines que ainda não foram atribuídas (dar prioridade àquelas cuja data de entrega está próxima)
            List<Dictionary<string, object>> pickingLineRows = dbQuery.performQueryWithTransaction("SELECT LinhaPicking.* FROM LinhaPicking INNER JOIN LinhaEncomenda ON (LinhaPicking.id_linha_encomenda = LinhaEncomenda.id) WHERE id_picking IS NULL AND quant_a_satisfazer <= @0@ ORDER BY data_entrega DESC", MAX_CAP_FUNCIONARIO);
            if (pickingLineRows.Count < 1)
                return null;

            try {
                List<string> listaAvisos = new List<string>();

                int currentWorkerCapacity = 0, index = 0;
                List<string> consideredLines = new List<string>();
                while (currentWorkerCapacity >= 0 && currentWorkerCapacity < MAX_CAP_FUNCIONARIO && index < pickingLineRows.Count) {
                    Dictionary<string, object> line = pickingLineRows.ElementAt(index);

                    int quantidadeASatisfazer = Convert.ToInt32(line["quant_a_satisfazer"]);
                    bool notificado = (bool)line["notificado_aviso"];

                    if (currentWorkerCapacity + quantidadeASatisfazer > MAX_CAP_FUNCIONARIO) {
                        index++;
                        continue;
                    }

                    // Determinar a localização (estratégia: ir à localização com a maior quantidade)
                    // O produto deve estar disponível no piso de picking (piso 1), no armazém primário
                    List<Dictionary<string, object>> localizacoesRows = dbQuery.performQueryWithTransaction(
                        "SELECT * FROM PRIDEMOSINF.dbo.ArtigoArmazem WITH (NOLOCK) WHERE Artigo = @0@ AND Localizacao LIKE @1@ ORDER BY StkActual DESC",
                        line["artigo"].ToString(), ARMAZEM_PRIMARIO + ".[A-Z].1.[0-9][0-9][0-9]");
                    if (localizacoesRows.Count < 1) {
                        if (!notificado) {
                            listaAvisos.Add("O armazém principal (" + ARMAZEM_PRIMARIO + ") não tem o artigo " + line["artigo"].ToString() + " na zona de picking (piso 1).");
                            dbQuery.performQueryWithTransaction(
                                "UPDATE LinhaPicking SET notificado_aviso = 1 WHERE id = @0@", line["id"].ToString());
                        }
                        index++;
                        continue;
                    }

                    string localizacao = localizacoesRows.ElementAt(0)["Localizacao"] as string;
                    if (localizacao == null)
                        throw new InvalidOperationException("Bad location name: 'null'");

                    // Aceitar a localizacao
                    dbQuery.performQueryWithTransaction(
                        "UPDATE LinhaPicking SET localizacao = @0@ WHERE id = @1@", localizacao, line["id"].ToString());

                    currentWorkerCapacity += quantidadeASatisfazer;
                    consideredLines.Add(line["id"].ToString());

                    index++;
                }

                // Registar os avisos
                RegistarAvisos(listaAvisos);

                // Para precaver!
                if (consideredLines.Count < 1) {
                    dbQuery.Commit();
                    return null;
                }
                //throw new InvalidOperationException("No lines considered");

                // Criar picking wave
                List<Dictionary<string, object>> pickingWaveResult = dbQuery.performQueryWithTransaction(
                        "INSERT INTO PickingWave(id_funcionario) OUTPUT INSERTED.id VALUES(@0@)", IDfuncionario);
                if (pickingWaveResult.Count < 1)
                    throw new InvalidOperationException("A new picking wave could not be created");

                // Agrupar as linhas seleccionadas
                StringBuilder queryString = new StringBuilder();
                queryString.Append("(LinhaPicking.id = '" + consideredLines.ElementAt(0) + "'");
                for (int i = 1; i < consideredLines.Count; i++) {
                    queryString.Append(" OR LinhaPicking.id = '" + consideredLines.ElementAt(i) + "'");
                }
                queryString.Append(")");
                dbQuery.performQueryWithTransaction(
                        "UPDATE LinhaPicking SET id_picking = @0@ WHERE " + queryString.ToString(),
                        pickingWaveResult.ElementAt(0)["id"].ToString());

                // Estabelecer a rota: Ordenação alfabética das localizações (S-shape heuristic)
                List<Dictionary<string, object>> pickingLinesRows = dbQuery.performQueryWithTransaction(
                        "SELECT LinhaPicking.id, localizacao, LinhaEncomenda.artigo, quant_a_satisfazer, unidade FROM LinhaPicking INNER JOIN LinhaEncomenda ON(LinhaPicking.id_linha_encomenda = LinhaEncomenda.id) WHERE " + queryString.ToString() + " ORDER BY localizacao");

                // Agrupar as linhas por localizacao
                Dictionary<string, List<PickingLine>> pickingOrderContent = new Dictionary<string, List<PickingLine>>();

                foreach (var line in pickingLinesRows) {
                    List<PickingLine> pl;
                    string location = line["localizacao"] as string;
                    if (!pickingOrderContent.TryGetValue(location, out pl)) {
                        pl = new List<PickingLine>();
                        pl.Add(new PickingLine(line["id"].ToString(), line["artigo"] as string, Convert.ToInt32(line["quant_a_satisfazer"]), line["unidade"] as string));
                        pickingOrderContent.Add(location, pl);

                    } else
                        pl.Add(new PickingLine(line["id"].ToString(), line["artigo"] as string, Convert.ToInt32(line["quant_a_satisfazer"]), line["unidade"] as string));
                }

                dbQuery.Commit();

                // Adicionar à picking order
                return new Wave<PickingLine>(pickingWaveResult.ElementAt(0)["id"].ToString(), Convert.ToInt32(workerRows.ElementAt(0)["id"]), pickingOrderContent);

            } catch (Exception e) {
                dbQuery.Rollback();
                ExceptionDispatchInfo.Capture(e).Throw();
                throw;
            }
        }







        // Argumento: numero da picking wave; linhas de pares id_da_linha_da_picking_wave - quantidade_de_facto_satisfeita
        public static bool TerminarPickingOrder(int funcionarioID, string pickingWaveID, List<LinhaWave> linhas) {

            if (linhas.Count < 1)
                throw new InvalidOperationException("A picking wave must have, at least, one line");

            // Verificar se este funcionario tem a picking wave atribuída a ele + verificar se a picking wave existe + Verificar se o funcionário existe no sistema + verificar se a picking wave está pendente
            List<Dictionary<string, object>> pickingWavesRows = DBQuery.performQuery(PriEngine.PickingDBConnString,
                "SELECT * FROM PickingWave WHERE id = @0@ AND id_funcionario = @1@ AND em_progresso = 1",
                pickingWaveID, funcionarioID);
            if (pickingWavesRows.Count != 1)
                throw new InvalidOperationException("There is no such picking wave");

            // Verificar se todas as linhas estão presentes
            StringBuilder conditionQuery = new StringBuilder();
            conditionQuery.Append("(id != '" + linhas.ElementAt(0).First + "'");
            foreach (var item in linhas) {
                conditionQuery.Append(" AND id != '" + item.First + "'");
            }
            conditionQuery.Append(")");
            List<Dictionary<string, object>> nonIncludedRows = DBQuery.performQuery(PriEngine.PickingDBConnString,
                "SELECT * FROM LinhaPicking WHERE id_picking = @0@ AND " + conditionQuery.ToString(),
                pickingWaveID);
            if (nonIncludedRows.Count > 0)
                throw new InvalidOperationException("The request must include all lines");

            DBQuery dbQuery = new DBQuery(PriEngine.PickingDBConnString);
            List<string> listaAvisos = new List<string>();

            try {
                // Marcar a quantidade satisfeita na linha
                foreach (var linha in linhas) {
                    // TODO pode ser optimizado
                    List<Dictionary<string, object>> pickingLineRow = dbQuery.performQueryWithTransaction(
                       "SELECT LinhaPicking.artigo, quant_a_satisfazer, LinhaEncomenda.data_entrega, LinhaPicking.id_linha_encomenda FROM LinhaPicking INNER JOIN PickingWave ON (LinhaPicking.id_picking = PickingWave.id) INNER JOIN LinhaEncomenda ON (LinhaPicking.id_linha_encomenda = LinhaEncomenda.id) WHERE LinhaPicking.id = @0@ AND LinhaPicking.id_picking = @1@ AND em_progresso = 1",
                       linha.First, pickingWaveID);

                    if (pickingLineRow.Count < 1)
                        continue;

                    else if (Convert.ToInt32(pickingLineRow.ElementAt(0)["quant_a_satisfazer"]) < linha.Second)
                        throw new InvalidOperationException((pickingLineRow.ElementAt(0)["artigo"] as string) + ": a quantidade satisfeita é superior à pedida.");

                    else if (linha.Second < 0)
                        throw new InvalidOperationException("A quantidade satisfeita não pode ser inferior a 0.");


                    List<Dictionary<string, object>> updatedRow = dbQuery.performQueryWithTransaction(
                        "UPDATE LinhaPicking SET quant_recolhida = @1@ OUTPUT INSERTED.id_linha_encomenda WHERE id = @0@ AND id_picking = @2@",
                        linha.First, linha.Second, pickingWaveID);

                    if (updatedRow.Count < 1)       // A linha não pertence à picking order
                        continue;

                    dbQuery.performQueryWithTransaction(
                        "UPDATE LinhaEncomenda SET quant_satisfeita = quant_satisfeita + @1@ WHERE id = @0@",
                        updatedRow.ElementAt(0)["id_linha_encomenda"].ToString(), linha.Second.ToString());

                    // Verificar se o picking foi cumprido dentro da data limite de entrega
                    DateTime dataEntrega = (DateTime)pickingLineRow.ElementAt(0)["data_entrega"];
                    if (dataEntrega == null)
                        continue;

                    else if (dataEntrega.CompareTo(DateTime.Today) < 0) {
                        List<Dictionary<string, object>> clientOrderRows = dbQuery.performQueryWithTransaction(
                            "SELECT CabecDoc.Entidade, CabecDoc.NumDoc, CabecDoc.Serie FROM LinhaEncomenda INNER JOIN PRIDEMOSINF.dbo.LinhasDoc ON (LinhasDoc.id = LinhaEncomenda.id_linha) INNER JOIN PRIDEMOSINF.dbo.CabecDoc ON (LinhasDoc.idCabecDoc = CabecDoc.id) WHERE LinhaEncomenda.id = @0@",
                            pickingLineRow.ElementAt(0)["id_linha_encomenda"].ToString());

                        if (clientOrderRows.Count > 0) {
                            Dictionary<string, object> linhaClientOrder = clientOrderRows.ElementAt(0);
                            listaAvisos.Add("Encomenda nº " + (linhaClientOrder["NumDoc"].ToString()) + ", série: " + (linhaClientOrder["Serie"] as string) + ", cliente: " + (linhaClientOrder["Entidade"] as string) + " -> O artigo " + pickingLineRow.ElementAt(0)["artigo"].ToString() + " vai ser entregue ao cliente fora do prazo de entrega estabelecido");
                        }
                    }
                }

                // Marcar a picking order como completa
                dbQuery.performQueryWithTransaction(
                        "UPDATE PickingWave SET em_progresso = 0, data_conclusao = GETDATE() WHERE id = @0@",
                        pickingWaveID);

                // Realizar tranferencia de armazem para EXPED
                // Retirar da quantidade reservada
                List<Dictionary<string, object>> affectedRows = dbQuery.performQueryWithTransaction(
                   "SELECT LinhaPicking.artigo, LinhaPicking.quant_a_satisfazer, quant_recolhida, localizacao, unidade, serie FROM LinhaPicking INNER JOIN LinhaEncomenda ON (LinhaPicking.id_linha_encomenda = LinhaEncomenda.id) WHERE id_picking = @0@",
                   pickingWaveID);

                List<ReplenishmentReservation> toReplenish = new List<ReplenishmentReservation>();
                foreach (var item in affectedRows) {

                    dbQuery.performQueryWithTransaction(
                        "UPDATE QuantidadeReserva SET quant_reservada = quant_reservada - @0@ WHERE artigo = @1@ AND armazem = @2@",
                        item["quant_a_satisfazer"].ToString(), item["artigo"] as string, ARMAZEM_PRIMARIO);

                    toReplenish.Add(new ReplenishmentReservation { Artigo = item["artigo"] as string, Localizacao = item["localizacao"] as string, Quantidade = Convert.ToInt32(item["quant_a_satisfazer"]), Unidade = item["unidade"] as string, Serie = item["serie"] as string });

                    double quantidadeATransferir = Convert.ToDouble(item["quant_recolhida"]);
                    if (quantidadeATransferir < 1)          // Caso não tenha havido quantidade a mover, nãp gerar a transferência de armazém
                        continue;

                    List<TransferenciaArtigo> lista = new List<TransferenciaArtigo>();
                    lista.Add(new TransferenciaArtigo(item["artigo"] as string, item["localizacao"] as string, GeneralConstants.ARMAZEM_EXPEDICAO, GeneralConstants.ARMAZEM_EXPEDICAO, quantidadeATransferir));

                    GerarTransferenciaArmazem(new TransferenciaArmazem(ARMAZEM_PRIMARIO, item["serie"] as string, lista));
                }

                RegistarAvisos(listaAvisos);
                GerarReplenishment(toReplenish);

                dbQuery.Commit();

            } catch (Exception) {
                dbQuery.Rollback();
                throw;
            }

            return true;
        }






        // O replenishment occore no armazém primário
        public static bool GerarReplenishment(List<ReplenishmentReservation> listaReposicao) {

            if (listaReposicao == null || listaReposicao.Count < 1)
                throw new InvalidOperationException("Invalid argument");

            // Verificar se a quantidade a repôr não ultrapassa a capacidade do funcionário
            foreach (var tuple in listaReposicao) {
                if (tuple.Quantidade < 1)
                    continue;

                while (tuple.Quantidade > MAX_CAP_FUNCIONARIO) {
                    tuple.Quantidade -= MAX_CAP_FUNCIONARIO;
                    listaReposicao.Add(new ReplenishmentReservation { Artigo = tuple.Artigo, Localizacao = tuple.Localizacao, Quantidade = MAX_CAP_FUNCIONARIO, Unidade = tuple.Unidade, Serie = tuple.Serie });
                }
            }

            StringBuilder queryString = new StringBuilder("INSERT INTO LinhaReplenishment(artigo, localizacao_destino, quant_a_satisfazer, unidade, serie) VALUES('");
            ReplenishmentReservation firstLine = listaReposicao.ElementAt(0);
            queryString.Append(firstLine.Artigo);
            queryString.Append("', '");
            queryString.Append(firstLine.Localizacao);
            queryString.Append("', ");
            queryString.Append(firstLine.Quantidade);
            queryString.Append(", '");
            queryString.Append(firstLine.Unidade);
            queryString.Append("', '");
            queryString.Append(firstLine.Serie);
            queryString.Append("')");

            for (int i = 1; i < listaReposicao.Count; i++) {
                ReplenishmentReservation itemARepor = listaReposicao.ElementAt(i);
                string artigo = itemARepor.Artigo;
                string localizacaoDestino = itemARepor.Localizacao;
                int quantidadeARepor = itemARepor.Quantidade;
                string unidade = itemARepor.Unidade;
                string serie = itemARepor.Serie;

                queryString.Append(", ('");
                queryString.Append(artigo);

                queryString.Append("', '");
                queryString.Append(localizacaoDestino);

                queryString.Append("', ");
                queryString.Append(quantidadeARepor);

                queryString.Append(", '");
                queryString.Append(unidade);

                queryString.Append("', '");
                queryString.Append(serie);
                queryString.Append("')");
            }

            DBQuery.performQuery(PriEngine.PickingDBConnString,
                queryString.ToString());

            return true;
        }












        // TODO: reservar o stock
        public static Wave<ReplenishmentLine> GetProximaReplenishmentOrder(int funcionarioID) {

            // Verificar se o funcionário existe no sistema
            List<Dictionary<string, object>> workerRows = DBQuery.performQuery(PriEngine.PickingDBConnString, "SELECT * FROM Funcionario WHERE id = @0@", funcionarioID);
            if (workerRows.Count != 1)
                throw new InvalidOperationException("There is no such worker");

            // Verificar se o funcionário tem orders pendentes
            if (AreWavesPendentes(funcionarioID))
                throw new InvalidOperationException("The worker has pending waves");

            List<string> listaAvisos = new List<string>();

            List<Dictionary<string, object>> rowsToAssign = DBQuery.performQuery(PriEngine.PickingDBConnString,
                "SELECT * FROM LinhaReplenishment WHERE id_replenishment IS NULL ORDER BY quant_a_satisfazer DESC");           // Abordagem gananciosa

            if (rowsToAssign.Count < 1)
                return null;

            DBQuery dbQuery = new DBQuery(PriEngine.PickingDBConnString);

            try {
                List<string> consideredLines = new List<string>();

                int currentWorkerCapacity = 0,
                    index = 0;

                while (currentWorkerCapacity < MAX_CAP_FUNCIONARIO && index < rowsToAssign.Count) {
                    Dictionary<string, object> linha = rowsToAssign.ElementAt(index);
                    string artigo = linha["artigo"].ToString(),
                            id = linha["id"].ToString();
                    int quantidade = Convert.ToInt32(linha["quant_a_satisfazer"]);
                    bool notificado = (bool)linha["notificado_aviso"];

                    if (currentWorkerCapacity + quantidade > MAX_CAP_FUNCIONARIO) {
                        index++;
                        continue;
                    }
                   
                    // Determinar a localização (estratégia: localização com maior quantidade)
                    // O produto deve estar disponível nos pisos de replenishment (pisos 2-9), no armazém primário
                    List<Dictionary<string, object>> localizacoesRows = dbQuery.performQueryWithTransaction("SELECT * FROM PRIDEMOSINF.dbo.ArtigoArmazem WITH (NOLOCK) WHERE Artigo = @0@ AND Localizacao LIKE '" + ARMAZEM_PRIMARIO + ".[A-Z].[2-9].[0-9][0-9][0-9]' AND StkActual > 0 ORDER BY StkActual DESC", artigo);
                    if (localizacoesRows.Count < 1) {
                        if (!notificado) {
                            listaAvisos.Add("O artigo " + artigo + " não está disponível nas áreas de reposição do armazém");
                            dbQuery.performQueryWithTransaction(
                                "UPDATE LinhaReplenishment SET notificado_aviso = 1 WHERE id = @0@", id);
                        }
                        index++;
                        continue;
                    }

                    string localizacao = localizacoesRows.ElementAt(0)["Localizacao"] as string;                // Localização com a maior quantidade
                    if (localizacao == null)
                        throw new InvalidOperationException("Bad location name: 'null'");

                    // Aceitar a localizacao
                    dbQuery.performQueryWithTransaction(
                        "UPDATE LinhaReplenishment SET localizacao_origem = @0@ WHERE id = @1@", localizacao, id);


                    currentWorkerCapacity += quantidade;
                    consideredLines.Add(id);

                    index++;
                }

                RegistarAvisos(listaAvisos);

                // Para precaver!
                if (consideredLines.Count < 1) {
                    dbQuery.Commit();
                    return null;
                }

                // Criar replenishment wave
                List<Dictionary<string, object>> replenishmentWaveResult = dbQuery.performQueryWithTransaction(
                        "INSERT INTO ReplenishmentWave(id_funcionario) OUTPUT INSERTED.id VALUES(@0@)", funcionarioID);
                if (replenishmentWaveResult.Count < 1)
                    throw new InvalidOperationException("A new picking wave could not be created");

                // Agrupar as linhas seleccionadas
                StringBuilder queryString = new StringBuilder();
                queryString.Append("(LinhaReplenishment.id = '" + consideredLines.ElementAt(0) + "'");
                for (int i = 1; i < consideredLines.Count; i++) {
                    queryString.Append(" OR LinhaReplenishment.id = '" + consideredLines.ElementAt(i) + "'");
                }
                queryString.Append(")");
                dbQuery.performQueryWithTransaction(
                        "UPDATE LinhaReplenishment SET id_replenishment = @0@ WHERE " + queryString.ToString(),
                        replenishmentWaveResult.ElementAt(0)["id"].ToString());


                // Estabelecer a rota: Ordenação alfabética das localizações
                // TODO Pressupõe-se que a localização de reposição e a posição final apenas variam em piso
                List<Dictionary<string, object>> replenishmentLinesRows = dbQuery.performQueryWithTransaction(
                        "SELECT * FROM LinhaReplenishment WHERE " + queryString.ToString() + " ORDER BY localizacao_destino");                                   // Assim, o piso é fixado, sendo, por conseguinte, como uma picking wave


                Dictionary<string, List<ReplenishmentLine>> replenishmentWaveContent = new Dictionary<string, List<ReplenishmentLine>>();

                foreach (var line in replenishmentLinesRows) {
                    List<ReplenishmentLine> pl;
                    string location = line["localizacao_origem"] as string;
                    string id = line["id"].ToString();
                    string artigo = line["artigo"] as string;
                    int quantidade = Convert.ToInt32(line["quant_a_satisfazer"]);
                    string unidade = line["unidade"] as string;
                    string destino = line["localizacao_destino"] as string;

                    if (!replenishmentWaveContent.TryGetValue(location, out pl)) {
                        pl = new List<ReplenishmentLine>();
                        //pl.Add(new ReplenishmentLine(line["artigo"] as string, Convert.ToInt32(line["quant_a_satisfazer"]), line["unidade"] as string, location, line["localizacao_destino"] as string));
                        pl.Add(new ReplenishmentLine(id, artigo, quantidade, unidade, location, destino));
                        replenishmentWaveContent.Add(location, pl);

                    } else
                        //pl.Add(new ReplenishmentLine(line["artigo"] as string, Convert.ToInt32(line["quant_a_satisfazer"]), line["unidade"] as string, location, line["localizacao_destino"] as string));
                        pl.Add(new ReplenishmentLine(id, artigo, quantidade, unidade, location, destino));
                }

                dbQuery.Commit();

                // Adicionar à picking order
                return new Wave<ReplenishmentLine>(replenishmentWaveResult.ElementAt(0)["id"].ToString(), Convert.ToInt32(workerRows.ElementAt(0)["id"]), replenishmentWaveContent);

            } catch (Exception) {
                dbQuery.Rollback();
                throw;
            }
        }





        // Argumento: linhas de pares id_da_linha - quantidade_de_facto_mudada
        public static bool TerminarReplenishmentOrder(int funcionarioID, string replenishmentWaveID, List<LinhaWave> linhas) {

            if (linhas.Count < 1)
                throw new InvalidOperationException("A replenishment wave must have, at least, one line");

            // Verificar se este funcionario tem a replenishment wave atribuída a ele + verificar se a picking wave existe + Verificar se o funcionário existe no sistema + verificar se a picking wave está pendente
            List<Dictionary<string, object>> replenishmentWavesRows = DBQuery.performQuery(PriEngine.PickingDBConnString,
                "SELECT * FROM ReplenishmentWave WHERE id = @0@ AND id_funcionario = @1@ AND em_progresso = 1",
                replenishmentWaveID, funcionarioID);
            if (replenishmentWavesRows.Count != 1)
                throw new InvalidOperationException("There is no such replenishment wave");


            // Verificar se todas as linhas estão presentes
            StringBuilder conditionQuery = new StringBuilder();
            conditionQuery.Append("(id != '" + linhas.ElementAt(0).First + "'");
            foreach (var item in linhas) {
                conditionQuery.Append(" AND id != '" + item.First + "'");
            }
            conditionQuery.Append(")");
            List<Dictionary<string, object>> nonIncludedRows = DBQuery.performQuery(PriEngine.PickingDBConnString,
                "SELECT * FROM LinhaReplenishment WHERE id_replenishment = @0@ AND " + conditionQuery.ToString(),
                replenishmentWaveID);
            if (nonIncludedRows.Count > 0)
                throw new InvalidOperationException("The request must include all lines");

            DBQuery dbQuery = new DBQuery(PriEngine.PickingDBConnString);

            try {
                // Marcar a quantidade satisfeita na linha
                foreach (var linha in linhas) {
                    // Verificar se a quantidade satisfeita é inferior à pedida
                    List<Dictionary<string, object>> replenishmentLineRow = dbQuery.performQueryWithTransaction(
                       "SELECT quant_a_satisfazer, artigo FROM LinhaReplenishment INNER JOIN ReplenishmentWave ON (LinhaReplenishment.id_replenishment = ReplenishmentWave.id) WHERE LinhaReplenishment.id = @0@ AND LinhaReplenishment.id_replenishment = @1@ AND em_progresso = 1",
                       linha.First, replenishmentWaveID);

                    if (replenishmentLineRow.Count < 1)
                        continue;

                    else if (Convert.ToInt32(replenishmentLineRow.ElementAt(0)["quant_a_satisfazer"]) < linha.Second)
                        throw new InvalidOperationException((replenishmentLineRow.ElementAt(0)["artigo"] as string) + ": a quantidade satisfeita é superior à pedida.");

                    else if (linha.Second < 0)
                        throw new InvalidOperationException("A quantidade satisfeita não pode ser inferior a 0.");


                    dbQuery.performQueryWithTransaction(
                        "UPDATE LinhaReplenishment SET quant_recolhida = @1@ OUTPUT INSERTED.id WHERE id = @0@ AND id_replenishment = @2@",
                        linha.First, linha.Second, replenishmentWaveID);
                }

                // Marcar a replenishment wave como completa
                dbQuery.performQueryWithTransaction(
                        "UPDATE ReplenishmentWave SET em_progresso = 0, data_conclusao = GETDATE() WHERE id = @0@",
                        replenishmentWaveID);

                // Realizar tranferencia de armazem
                // Retirar da quantidade reservada
                List<Dictionary<string, object>> affectedRows = dbQuery.performQueryWithTransaction(
                   "SELECT artigo, quant_a_satisfazer, quant_recolhida, localizacao_origem, localizacao_destino, serie FROM LinhaReplenishment WHERE id_replenishment = @0@",
                   replenishmentWaveID);

                foreach (var item in affectedRows) {
                    double quantidadeATransferir = Convert.ToDouble(item["quant_recolhida"]);
                    if (quantidadeATransferir < 1)
                        continue;

                    List<TransferenciaArtigo> lista = new List<TransferenciaArtigo>();
                    lista.Add(new TransferenciaArtigo(item["artigo"] as string, item["localizacao_origem"] as string, item["localizacao_destino"] as string, ARMAZEM_PRIMARIO, quantidadeATransferir));

                    GerarTransferenciaArmazem(new TransferenciaArmazem(ARMAZEM_PRIMARIO, item["serie"] as string, lista));
                }

                dbQuery.Commit();

            } catch (Exception) {
                dbQuery.Rollback();
                throw;
            }

            return true;
        }







        public static bool AreWavesPendentes(int funcionarioID) {

            List<Dictionary<string, object>> pendingRows = DBQuery.performQuery(PriEngine.PickingDBConnString,
                "SELECT PickingWave.id FROM PickingWave INNER JOIN Funcionario ON (PickingWave.id_funcionario = Funcionario.id) WHERE PickingWave.em_progresso = 1 AND Funcionario.id = @0@" +
                " UNION " +
                "SELECT ReplenishmentWave.id FROM ReplenishmentWave INNER JOIN Funcionario ON (ReplenishmentWave.id_funcionario = Funcionario.id) WHERE ReplenishmentWave.em_progresso = 1 AND Funcionario.id = @0@",
                funcionarioID);

            return pendingRows.Count != 0;
        }



        public static dynamic GetWaveActual(int funcionarioID) {

            if (!AreWavesPendentes(funcionarioID))
                return null;

            Wave<ReplenishmentLine> replenishmentWave = GetReplenishmentWaveActual(funcionarioID);
            if (replenishmentWave != null)
                return replenishmentWave;

            else
                return GetPickingWaveActual(funcionarioID);
        }




        public static Wave<ReplenishmentLine> GetReplenishmentWaveActual(int funcionarioID) {

            List<Dictionary<string, object>> replenishmentWaveIDRows = DBQuery.performQuery(PriEngine.PickingDBConnString,
                    "SELECT * FROM ReplenishmentWave WHERE id_funcionario = @0@ AND em_progresso = 1",
                    funcionarioID);

            if (replenishmentWaveIDRows.Count < 1)
                return null;

            string replenishmentWaveID = replenishmentWaveIDRows.ElementAt(0)["id"].ToString();

            List<Dictionary<string, object>> replenishmentLinesRows = DBQuery.performQuery(PriEngine.PickingDBConnString,
                    "SELECT * FROM LinhaReplenishment WHERE id_replenishment = @0@ ORDER BY localizacao_destino",
                    replenishmentWaveID);

            Dictionary<string, List<ReplenishmentLine>> replenishmentWaveContent = new Dictionary<string, List<ReplenishmentLine>>();

            foreach (var line in replenishmentLinesRows) {
                List<ReplenishmentLine> pl;
                string location = line["localizacao_origem"] as string;
                string id = line["id"].ToString();
                string artigo = line["artigo"] as string;
                int quantidade = Convert.ToInt32(line["quant_a_satisfazer"]);
                string unidade = line["unidade"] as string;
                string destino = line["localizacao_destino"] as string;

                if (!replenishmentWaveContent.TryGetValue(location, out pl)) {
                    pl = new List<ReplenishmentLine>();
                    pl.Add(new ReplenishmentLine(id, artigo, quantidade, unidade, location, destino));
                    replenishmentWaveContent.Add(location, pl);

                } else
                    pl.Add(new ReplenishmentLine(id, artigo, quantidade, unidade, location, destino));
            }

            return new Wave<ReplenishmentLine>(replenishmentWaveID, funcionarioID, replenishmentWaveContent);
        }





        public static Wave<PickingLine> GetPickingWaveActual(int funcionarioID) {

            List<Dictionary<string, object>> pickingWaveIDRows = DBQuery.performQuery(PriEngine.PickingDBConnString,
                    "SELECT * FROM PickingWave WHERE id_funcionario = @0@ AND em_progresso = 1",
                    funcionarioID);

            if (pickingWaveIDRows.Count < 1)
                return null;

            string pickingWaveID = pickingWaveIDRows.ElementAt(0)["id"].ToString();

            // Estabelecer a rota: Ordenação alfabética das localizações (S-shape heuristic)
            List<Dictionary<string, object>> pickingLinesRows = DBQuery.performQuery(PriEngine.PickingDBConnString,
                    "SELECT LinhaPicking.id, localizacao, LinhaEncomenda.artigo, quant_a_satisfazer, unidade FROM LinhaPicking INNER JOIN LinhaEncomenda ON(LinhaPicking.id_linha_encomenda = LinhaEncomenda.id) WHERE id_picking = @0@ ORDER BY localizacao",
                    pickingWaveID);

            // Agrupar as linhas por localizacao
            Dictionary<string, List<PickingLine>> pickingOrderContent = new Dictionary<string, List<PickingLine>>();

            foreach (var line in pickingLinesRows) {
                List<PickingLine> pl;
                string location = line["localizacao"] as string;
                if (!pickingOrderContent.TryGetValue(location, out pl)) {
                    pl = new List<PickingLine>();
                    pl.Add(new PickingLine(line["id"].ToString(), line["artigo"] as string, Convert.ToInt32(line["quant_a_satisfazer"]), line["unidade"] as string));
                    pickingOrderContent.Add(location, pl);

                } else
                    pl.Add(new PickingLine(line["id"].ToString(), line["artigo"] as string, Convert.ToInt32(line["quant_a_satisfazer"]), line["unidade"] as string));
            }

            return new Wave<PickingLine>(pickingWaveID, funcionarioID, pickingOrderContent);
        }


        public static int GetCapacidadeMaximaFuncionario() {

            List<Dictionary<string, object>> definitionsRows = DBQuery.performQuery(PriEngine.PickingDBConnString, "SELECT * FROM Definicoes WHERE chave = 'cap_max_funcionario'");
            if (definitionsRows.Count < 1)
                throw new InvalidOperationException("Não existe nenhuma entrada na tabela de definições relativamente à capacidade máxima do funcionário");

            string capMaxFuncionario = (string)definitionsRows.ElementAt(0)["valor"];
            return Convert.ToInt32(capMaxFuncionario);
        }


        public static void SetCapacidadeMaximaFuncionario(int cap) {
            if (cap < 1)
                throw new InvalidOperationException("Capacidade inválida");

            DBQuery.performQuery(PriEngine.PickingDBConnString,
                "UPDATE Definicoes SET valor = @0@ WHERE chave = 'cap_max_funcionario'", cap.ToString());

            PriIntegration.MAX_CAP_FUNCIONARIO = cap;
        }



        public static string GetArmazemPrincipal() {
            List<Dictionary<string, object>> definitionsRows = DBQuery.performQuery(PriEngine.PickingDBConnString, "SELECT * FROM Definicoes WHERE chave = 'armazem_principal'");
            if (definitionsRows.Count < 1)
                throw new InvalidOperationException("Não existe nenhuma entrada na tabela de definições relativamente ao armazém principal");

            string armazemPrincipal = (string)definitionsRows.ElementAt(0)["valor"];
            return armazemPrincipal;
        }

        public static void SetArmazemPrincipal(string armazem) {
            if (armazem == null)
                throw new InvalidOperationException("Bad argument");

            List<Dictionary<string, object>> armazensRows = DBQuery.performQuery(PriEngine.DBConnString, "SELECT * FROM Armazens WHERE Armazem = @0@", armazem);
            if (armazensRows.Count < 1)
                throw new InvalidOperationException("O armazém não está registado no Primavera");

            DBQuery.performQuery(PriEngine.PickingDBConnString,
                "UPDATE Definicoes SET valor = @0@ WHERE chave = 'armazem_principal'", armazem);

            PriIntegration.ARMAZEM_PRIMARIO = armazem;
        }

        public static void InitializeIntegration() {
            PriIntegration.ARMAZEM_PRIMARIO = GetArmazemPrincipal();
            PriIntegration.MAX_CAP_FUNCIONARIO = GetCapacidadeMaximaFuncionario();
        }


        public static int GetNumeroAvisosPorLer() {
            List<Dictionary<string, object>> avisosRows = DBQuery.performQuery(PriEngine.PickingDBConnString, "SELECT * FROM Avisos WHERE visto = 0");
            return avisosRows.Count;
        }

        public static List<string> GetAvisosPorLer() {
            List<Dictionary<string, object>> avisosRows = DBQuery.performQuery(PriEngine.PickingDBConnString, "SELECT * FROM Avisos WHERE visto = 0");

            if(avisosRows.Count > 0)
                DBQuery.performQuery(PriEngine.PickingDBConnString, "UPDATE Avisos SET visto = 1 WHERE visto = 0");

            List<string> result = new List<string>();

            foreach (var item in avisosRows) {
                result.Add(item["mensagem"] as string);
            }

            return result;
        }

        #endregion Testes

    }
}