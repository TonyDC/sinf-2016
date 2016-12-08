﻿using System;
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

namespace SalesOrderPicking.Lib_Primavera {

    public class PriIntegration {

        public const int MAX_CAP_FUNCIONARIO = 2;
        // TODO GeneralConstants + substituir ocorrências
        public const string ARMAZEM_PRIMARIO = "A1";

        #region Artigo

        public static List<Artigo> ListaArtigos() {

            List<Artigo> listaArtigos = new List<Artigo>();

            List<Dictionary<string, object>> rows = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM Artigo WITH (NOLOCK)");

            foreach (var item in rows) {
                object artigoID = item["Artigo"], descricao = item["Descricao"];

                listaArtigos.Add(new Artigo(artigoID as string, descricao as string));
            }

            return listaArtigos;
        }


        public static Artigo ObterArtigo(string codArtigo) {

            if (PriEngine.DBConnString == null)
                throw new InvalidOperationException("Connexão não inicializada (Connection string is null)");

            else if (codArtigo == null)
                throw new InvalidOperationException("Invalid object code");


            List<Dictionary<string, object>> artigo = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM Artigo WITH (NOLOCK) WHERE Artigo = '@0@'", codArtigo);
            if (artigo.Count != 1)
                throw new InvalidOperationException("Código de artigo inválido");

            Dictionary<string, object> linhaArtigo = artigo.ElementAt(0);
            object descricao = linhaArtigo["Descricao"], unidadeVenda = linhaArtigo["UnidadeVenda"];

            List<Dictionary<string, object>> stockArtigo = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM ArtigoArmazem WITH (NOLOCK) WHERE Artigo = '@0@' ORDER BY StkActual DESC", codArtigo);

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

        public static List<EncomendaCliente> GetEncomendasClientes(string f, string s, string clienteID = null, string nDoc = null) {

            uint nDocUInt;
            if (f == null || s == null || (nDoc != null && !uint.TryParse(nDoc, out nDocUInt)))
                throw new InvalidOperationException("Bad arguments: 'f', 's' or 'nDoc'");


            List<EncomendaCliente> listaArtigos = new List<EncomendaCliente>();
            List<Dictionary<string, object>> listaEncomendas = null;

            if (clienteID == null)
                if (nDoc == null)
                    listaEncomendas = Utilities.performQuery(PriEngine.DBConnString, GeneralConstants.QUERY_ENCOMENDAS_PENDENTES + " AND Filial = '@0@' AND Serie = '@1@' ORDER BY NumDoc", f, s);
                else
                    listaEncomendas = Utilities.performQuery(PriEngine.DBConnString, GeneralConstants.QUERY_ENCOMENDAS_PENDENTES + " AND NumDoc = @0@ AND Filial = '@1@' AND Serie = '@2@'", nDoc, f, s);
            else
                if (nDoc == null)
                    listaEncomendas = Utilities.performQuery(PriEngine.DBConnString, GeneralConstants.QUERY_ENCOMENDAS_PENDENTES + " AND EntidadeFac = '@0@' AND Filial = '@1@' AND Serie = '@2@' ORDER BY NumDoc", clienteID, f, s);
                else
                    listaEncomendas = Utilities.performQuery(PriEngine.DBConnString, GeneralConstants.QUERY_ENCOMENDAS_PENDENTES + " AND EntidadeFac = '@0@' AND Filial = '@1@' AND Serie = '@2@' AND NumDoc = @3@ ORDER BY NumDoc", clienteID, f, s, nDoc);


            foreach (var item in listaEncomendas) {
                object encomendaID = item["Id"], filial = item["Filial"], serie = item["Serie"], numDoc = item["NumDoc"], cliente = item["EntidadeFac"];

                List<Dictionary<string, object>> linhasEncomenda = Utilities.performQuery(PriEngine.DBConnString, "SELECT Id, Artigo, LinhasDoc.Quantidade AS Quantidade, QuantTrans, NumLinha, Armazem, Localizacao, Lote, DataEntrega FROM LinhasDoc WITH (NOLOCK) INNER JOIN LinhasDocStatus WITH (NOLOCK) ON (LinhasDocStatus.IdLinhasDoc = LinhasDoc.Id) WHERE LinhasDoc.IdCabecDoc = '@0@' AND LinhasDoc.Artigo IS NOT NULL", encomendaID.ToString());

                List<LinhaEncomendaCliente> artigosEncomenda = new List<LinhaEncomendaCliente>();

                foreach (var linha in linhasEncomenda) {
                    object linhaID = linha["Id"], artigoID = linha["Artigo"], quantidade = linha["Quantidade"], quantidadeSatisfeita = linha["QuantTrans"], numLinha = linha["NumLinha"], armazem = linha["Armazem"], localizacao = linha["Localizacao"], lote = linha["Lote"], dataEntrega = linha["DataEntrega"];

                    artigosEncomenda.Add(new LinhaEncomendaCliente(linhaID.ToString(), artigoID as string, armazem as string, localizacao as string, lote as string, Convert.ToDouble(quantidade), Convert.ToDouble(quantidadeSatisfeita), Convert.ToUInt32(numLinha), (DateTime)dataEntrega));
                }

                listaArtigos.Add(new EncomendaCliente(encomendaID.ToString(), Convert.ToUInt32(numDoc), cliente as string, serie as string, filial as string, artigosEncomenda));
            }

            return listaArtigos;
        }
        // TODO verificar se a quantidade satisfeita é igual à quantidade encomendada
        public static bool GerarGuiaRemessa(PedidoTransformacaoECL encomenda) {

            if (PriEngine.IniciaTransaccao()) {

                if (encomenda == null)
                    throw new InvalidOperationException("Pedido de encomenda inválido (parâmetro a null)");

                // Carregar encomenda de cliente
                GcpBEDocumentoVenda objEncomenda = PriEngine.Engine.Comercial.Vendas.Edita(encomenda.Filial, GeneralConstants.ENCOMENDA_CLIENTE_DOCUMENTO, encomenda.Serie, (int)encomenda.NDoc);

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

                // ------------------------------------------------------------------------------------

                if (!PriEngine.IniciaTransaccao())
                    return false;

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
                return true;

            } else {
                return false;
            }
        }

        #endregion Encomendas



        #region Cliente

        public static List<Cliente> GetListaClientes() {

            List<Cliente> listaClientes = new List<Cliente>();
            List<Dictionary<string, object>> queryRows = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM Clientes WITH (NOLOCK)");

            foreach (var item in queryRows) {
                object id = item["Cliente"], nome = item["Nome"];

                listaClientes.Add(new Cliente(id as string, nome as string));
            }

            return listaClientes;
        }


        public static Cliente GetClienteInfo(string clienteID) {

            if (clienteID == null)
                throw new InvalidOperationException("ID de cliente inválido");


            List<Dictionary<string, object>> queryRows = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM Clientes WITH (NOLOCK) WHERE Cliente = '@0@'", clienteID);

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
            List<Dictionary<string, object>> queryRows = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM Armazens WITH (NOLOCK)");

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
            List<Dictionary<string, object>> queryRows = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM ArmazemLocalizacoes WITH (NOLOCK) WHERE Armazem = '@0@'", armazemID);

            foreach (var item in queryRows) {
                object id = item["Id"], localizacao = item["Localizacao"], descricao = item["Descricao"], nomeNivel = item["NomeNivel"];

                listaLocalizacoesArmazem.Add(new LocalizacaoArmazem(id.ToString(), localizacao as string, descricao as string, nomeNivel as string));
            }

            return listaLocalizacoesArmazem;
        }


        public static bool GerarTransferenciaArmazem(TransferenciaArmazem lista) {

            if (PriEngine.IniciaTransaccao()) {

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

                return true;

            } else
                return false;
        }

        #endregion Armazem

        #region Administracao

        public static List<string> GetSeries() {

            List<Dictionary<string, object>> queryRows = Utilities.performQuery(PriEngine.DBConnString, "SELECT Serie FROM SeriesVendas WITH (NOLOCK) WHERE TipoDoc = 'ECL'");
            List<string> result = new List<string>();

            foreach (var line in queryRows) {
                object serie = line["Serie"];
                result.Add(serie as string);
            }

            return result;
        }

        #endregion Administracao



        #region Testes

        public static Dictionary<string, int> GetStockActual(string armazem) {

            List<Dictionary<string, object>> rows = Utilities.performQuery(PriEngine.DBConnString, "SELECT sum(StkActual) AS Stock, Artigo FROM ArtigoArmazem WHERE Armazem = '@0@' GROUP BY Artigo", armazem);
            List<Dictionary<string, object>> reservedStockRows = Utilities.performQuery(PriEngine.PickingDBConnString, "SELECT * FROM QuantidadeReserva WHERE armazem = '@0@'", armazem);
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
        }
        /*
        public static uint GetCapacidadeFuncionario() {

            List<Dictionary<string, object>> definitionsRows = Utilities.performQuery(PriEngine.DBConnString, "SELECT capMaxFuncionario FROM Definicoes");
            if (definitionsRows.Count != 1)
                throw new InvalidOperationException("Bad definitions table");

            Dictionary<string, object> definitions = definitionsRows.ElementAt(0);
            object capMax;
            if (!definitions.TryGetValue("capMaxFuncionario", out capMax))
                throw new InvalidOperationException("There is no such column as 'capMaxFuncionario'");

            return Convert.ToUInt32(capMax);
        }
        */
        public static bool GerarPickingOrders(string filial, string serie, List<uint> encomendas) {

            if (filial == null || serie == null || encomendas.Count < 1)
                throw new InvalidOperationException("Bad arguments");

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
            Dictionary<string, int> stockActual = GetStockActual("A1");
            Dictionary<string, int> stockReserva = new Dictionary<string, int>();

            List<Dictionary<string, object>> linhasEncomendas = Utilities.performQuery(PriEngine.DBConnString,
                "SELECT LinhasDoc.Id, sys.fn_varbintohexstr(LinhasDoc.VersaoUltAct) as VersaoUltAct, LinhasDoc.Unidade, LinhasDoc.Artigo, LinhasDoc.Quantidade FROM CabecDoc INNER JOIN LinhasDoc ON (CabecDoc.id = LinhasDoc.idCabecDoc) WHERE TipoDoc = 'ECL' AND Quantidade > 0 AND filial = '@0@' AND serie = '@1@' AND @2@",
                filial, serie, conditionQueryString.ToString());
            int capMaxFuncionario = MAX_CAP_FUNCIONARIO;

            foreach (var tuple in linhasEncomendas) {

                // Verificar se já existe uma linha
                List<Dictionary<string, object>> linhaEncomenda = Utilities.performQuery(PriEngine.PickingDBConnString, "SELECT * FROM LinhaEncomenda WHERE id_linha = '@0@' AND versao_ult_act = '@1@'", tuple["Id"].ToString(), tuple["VersaoUltAct"].ToString());

                if (linhaEncomenda.Count > 0) {
                    List<Dictionary<string, object>> linhaPendente = Utilities.performQuery(PriEngine.PickingDBConnString, "SELECT LinhaEncomenda.id FROM LinhaEncomenda INNER JOIN LinhaPicking ON (LinhaEncomenda.id = LinhaPicking.id_linha_encomenda) LEFT JOIN PickingWave ON (LinhaPicking.id_picking = PickingWave.id) WHERE id_linha = '@0@' AND versao_ult_act = '@1@' AND (id_picking IS NULL OR em_progresso = 1)", tuple["Id"].ToString(), tuple["VersaoUltAct"].ToString());
                    if (linhaPendente.Count > 0)
                        continue;       // Está a decorrer uma picking order a satisfazer esta linha

                    Dictionary<string, object> linha = linhaEncomenda.ElementAt(0);
                    int quantidadePedida = Convert.ToInt32(linha["quant_pedida"]),
                           quantidadeSatisfeita = Convert.ToInt32(linha["quant_satisfeita"]);

                    // A linha da encomenda já foi satisfeita
                    if (quantidadePedida <= quantidadeSatisfeita)
                        continue;

                    else {
                        string artigo = linha["artigo"] as string;
                        if (!stockActual.ContainsKey(artigo))
                            throw new InvalidOperationException("The main warehouse does not have the product " + artigo + ", which is present in document nº" + tuple["NumDoc"]);

                        // Criar uma nova linha picking por forma a satisfazer a diferença
                        // TODO gerar uma reposição de stock
                        int diferenca = quantidadePedida - quantidadeSatisfeita;
                        if (diferenca > stockActual[artigo])
                            throw new InvalidOperationException("Insuficient stock for " + artigo);

                        else {
                            stockActual[artigo] -= diferenca;

                            if (!stockReserva.ContainsKey(artigo))
                                stockReserva[artigo] = diferenca;
                            else
                                stockReserva[artigo] += diferenca;
                        }

                        // A quantidade a satisfazer não pode ultrapassar a capacidade máxima do funcionário
                        while (diferenca > capMaxFuncionario) {
                            diferenca -= capMaxFuncionario;
                            Utilities.performQuery(PriEngine.PickingDBConnString, "INSERT INTO LinhaPicking(quant_a_satisfazer, artigo, id_linha_encomenda) VALUES(@0@, '@1@', '@2@')",
                                capMaxFuncionario.ToString(), artigo, linha["id"].ToString());
                        }
                        Utilities.performQuery(PriEngine.PickingDBConnString, "INSERT INTO LinhaPicking(quant_a_satisfazer, artigo, id_linha_encomenda) VALUES(@0@, '@1@', '@2@')",
                                diferenca.ToString(), artigo, linha["id"].ToString());

                    }

                } else {

                    int diferenca = Convert.ToInt32(tuple["Quantidade"]);
                    string artigo = tuple["Artigo"] as string;
                    if (!stockActual.ContainsKey(artigo))
                        throw new InvalidOperationException("The main warehouse does not have the product " + artigo + ", which is present in nº" + tuple["NumDoc"]);

                    else if (diferenca > stockActual[artigo])
                        // TODO gerar uma reposição de stock
                        throw new InvalidOperationException("Insuficient stock for " + artigo);

                    else {
                        stockActual[artigo] -= diferenca;

                        if (!stockReserva.ContainsKey(artigo))
                            stockReserva[artigo] = diferenca;
                        else
                            stockReserva[artigo] += diferenca;
                    }


                    // Criar uma nova linha de encomenda
                    List<Dictionary<string, object>> insertResult = Utilities.performQuery(PriEngine.PickingDBConnString,
                        "INSERT INTO LinhaEncomenda(id_linha, versao_ult_act, artigo, quant_pedida, unidade) OUTPUT INSERTED.id VALUES('@0@', '@1@', '@2@', @3@, '@4@')",
                        tuple["Id"].ToString(), tuple["VersaoUltAct"].ToString(), artigo, tuple["Quantidade"].ToString(), tuple["Unidade"] as string);

                    if (insertResult.Count < 1)
                        throw new InvalidOperationException("Error in inserting into LinhaEncomenda");


                    while (diferenca > capMaxFuncionario) {
                        diferenca -= capMaxFuncionario;
                        Utilities.performQuery(PriEngine.PickingDBConnString,
                            "INSERT INTO LinhaPicking(quant_a_satisfazer, artigo, id_linha_encomenda) VALUES(@0@, '@1@', '@2@')",
                            capMaxFuncionario.ToString(), artigo, insertResult.ElementAt(0)["id"].ToString());
                    }
                    Utilities.performQuery(PriEngine.PickingDBConnString,
                            "INSERT INTO LinhaPicking(quant_a_satisfazer, artigo, id_linha_encomenda) VALUES(@0@, '@1@', '@2@')",
                            diferenca.ToString(), artigo, insertResult.ElementAt(0)["id"].ToString());
                }
            }

            // Guardar as quantias reservadas
            foreach (KeyValuePair<string, int> item in stockReserva) {

                // Verificar se já existe
                List<Dictionary<string, object>> reservedStockRows = Utilities.performQuery(PriEngine.PickingDBConnString,
                    "SELECT * FROM QuantidadeReserva WHERE artigo = '@0@' AND armazem = '@1@'", item.Key, "A1");
                if (reservedStockRows.Count > 0)
                    Utilities.performQuery(PriEngine.PickingDBConnString,
                   "UPDATE QuantidadeReserva SET quant_reservada = @0@ WHERE id = '@1@'", (item.Value + Convert.ToInt32(reservedStockRows.ElementAt(0)["quant_reservada"])).ToString(), reservedStockRows.ElementAt(0)["id"].ToString());
                else
                    Utilities.performQuery(PriEngine.PickingDBConnString,
                  "INSERT INTO QuantidadeReserva(artigo, quant_reservada, armazem) VALUES ('@0@', @1@, '@2@')", item.Key, item.Value.ToString(), "A1");

            }


            // futuramente: para cada encomenda, verificar se já foi satisfeita pelas picking orders associadas a ela (note-se que cada linha está associada a uma encomenda); se sim, se ela foi satisfeita na totalidade

            // verificar se as encomendas seleccionadas podem ser satisfeitas com o stock existente actualmente no armazém

            // Para cada encomenda, obter as linhas da mesma; não considerar aquelas em que a quantidade já satisfeita é igual à quantidade total


            // para cada linha, averiguar a localização que tenha esse artigo e em quantidade suficiente; criar uma linha para a picking order
            // abordagem inicial: a localizacao com o maior stock desse produto
            // abordagem ideal: ter, em atenção, as rotas (através da ordenação alfabética das localizações)


            // criar uma picking order
            // Ter em atenção a capacidade máxima de cada funcionário (parâmetro configurável através de GetCapacidadeFuncionario)
            // abordagem inicial: uma picking order por cada encomenda
            // abordagem ideal: uma encomenda pode ser satisfeita por várias picking orders -> agrupar, da melhor forma, as várias picking lines
            // para cada agrupamento de linhas, verificar quais são os diferentes armazens utilizados para esse conjunto, e registar isso para a picking order




            // Adicionar essas linhas à picking order



            // ----------------------------------------
            // Para encomendas que já foram satisfeitas parcialmente, determinar a diferença entre o satisfeito e o total encomendado
            // Pressupostos: a quantidade já satisfeita está disponível através do campo QuantSatisfeita

            return true;
        }


        public static Wave<PickingLine> GetProximaPickingWave(int IDfuncionario) {

            // Verificar se o funcionário existe no sistema
            List<Dictionary<string, object>> workerRows = Utilities.performQuery(PriEngine.PickingDBConnString, "SELECT * FROM Funcionario WHERE id = @0@", IDfuncionario.ToString());
            if (workerRows.Count != 1)
                throw new InvalidOperationException("There is no such worker");

            // O funcionário não pode ter picking a decorrer
            List<Dictionary<string, object>> pendingPickingWaveRows = Utilities.performQuery(PriEngine.PickingDBConnString,
                "SELECT Funcionario.id FROM Funcionario INNER JOIN PickingWave ON (Funcionario.id = PickingWave.id_funcionario) WHERE id_funcionario = @0@ AND em_progresso = 1", IDfuncionario.ToString());
            if (pendingPickingWaveRows.Count != 0)
                throw new InvalidOperationException("The worker has pendign picking waves");

            // Buscar as picking lines que ainda não foram atribuídas
            List<Dictionary<string, object>> pickingLineRows = Utilities.performQuery(PriEngine.PickingDBConnString, "SELECT * FROM LinhaPicking WHERE id_picking IS NULL AND quant_a_satisfazer <= @0@", MAX_CAP_FUNCIONARIO.ToString());
            if (pickingLineRows.Count < 1)
                return null;

            Random random = new Random();
            int currentWorkerCapacity = 0, index = 0;
            List<string> consideredLines = new List<string>();
            while (currentWorkerCapacity >= 0 && currentWorkerCapacity <= MAX_CAP_FUNCIONARIO && index < pickingLineRows.Count) {
                Dictionary<string, object> line = pickingLineRows.ElementAt(index);

                int quantidadeASatisfazer = Convert.ToInt32(line["quant_a_satisfazer"]);
                if (currentWorkerCapacity + quantidadeASatisfazer > MAX_CAP_FUNCIONARIO) {
                    index++;
                    continue;
                }

                // Determinar a localização (estratégia: localização aleatória)
                // O produto deve estar disponível no piso de picking (piso 1), no armazém primário (A1)
                List<Dictionary<string, object>> localizacoesRows = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM ArtigoArmazem WITH (NOLOCK) WHERE Artigo = '@0@' AND Localizacao LIKE '@1@.[A-Z].1.[0-9][0-9][0-9]'", line["artigo"].ToString(), ARMAZEM_PRIMARIO);
                if (localizacoesRows.Count < 1)
                    throw new InvalidOperationException("The product " + line["Artigo"].ToString() + " is not available in the main warehouse");
                
                string localizacao = localizacoesRows.ElementAt(random.Next(0, localizacoesRows.Count - 1))["Localizacao"] as string;
                if (localizacao == null)
                    throw new InvalidOperationException("Bad location name: " + localizacao);

                // Aceitar a localizacao
                Utilities.performQuery(PriEngine.PickingDBConnString,
                    "UPDATE LinhaPicking SET localizacao = '@0@' WHERE id = '@1@'", localizacao, line["id"].ToString());

                currentWorkerCapacity += quantidadeASatisfazer;
                consideredLines.Add(line["id"].ToString());

                index++;
            }

            // TODO: pode acontecer caso a quantidadeASatisfazer for sempre superior à capacidade máxima do funcionário
            if (consideredLines.Count < 1)
                throw new InvalidOperationException("No lines considered");

            // Criar picking wave
            List<Dictionary<string, object>> pickingWaveResult = Utilities.performQuery(PriEngine.PickingDBConnString,
                    "INSERT INTO PickingWave(id_funcionario) OUTPUT INSERTED.id VALUES(@0@)", IDfuncionario.ToString());
            if (pickingWaveResult.Count < 1)
                throw new InvalidOperationException("A new picking wave could not be created");

            // Agrupar as linhas seleccionadas
            StringBuilder queryString = new StringBuilder();
            queryString.Append("(LinhaPicking.id = '" + consideredLines.ElementAt(0) + "'");
            for (int i = 1; i < consideredLines.Count; i++) {
                queryString.Append(" OR LinhaPicking.id = '" + consideredLines.ElementAt(i) + "'");
            }
            queryString.Append(")");
            Utilities.performQuery(PriEngine.PickingDBConnString,
                    "UPDATE LinhaPicking SET id_picking = '@0@' WHERE @1@",
                    pickingWaveResult.ElementAt(0)["id"].ToString(), queryString.ToString());

            // Estabelecer a rota: Ordenação alfabética das localizações
            List<Dictionary<string, object>> pickingLinesRows = Utilities.performQuery(PriEngine.PickingDBConnString,
                    "SELECT LinhaPicking.id, localizacao, LinhaEncomenda.artigo, quant_a_satisfazer, unidade FROM LinhaPicking INNER JOIN LinhaEncomenda ON(LinhaPicking.id_linha_encomenda = LinhaEncomenda.id) WHERE @0@ ORDER BY localizacao",
                    queryString.ToString());

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

            // Adicionar à picking order
            return new Wave<PickingLine>(pickingWaveResult.ElementAt(0)["id"].ToString(), Convert.ToInt32(workerRows.ElementAt(0)["id"]), pickingOrderContent);
        }


        // ATENÇÃO: NÃO CONTABILIZAR AS LINHAS CUJA QUANTIDADE É 0
        // Argumento: numero da picking wave; linhas de pares id_da_linha_da_picking_wave - quantidade_de_facto_satisfeita
        public static bool TerminarPickingOrder(int funcionarioID, string pickingWaveID, List<Pair<string, int>> linhas, string serie) {

            if (linhas.Count < 1)
                throw new InvalidOperationException("A picking wave must have, at least, one line");

            // Verificar se este funcionario tem a picking wave atribuída a ele + verificar se a picking wave existe + Verificar se o funcionário existe no sistema + verificar se a picking wave está pendente
            List<Dictionary<string, object>> pickingWavesRows = Utilities.performQuery(PriEngine.PickingDBConnString,
                "SELECT * FROM PickingWave WHERE id = '@0@' AND id_funcionario = '@1@' AND em_progresso = 1",
                pickingWaveID, funcionarioID.ToString());
            if (pickingWavesRows.Count != 1)
                throw new InvalidOperationException("There is no such picking wave");
            /*
            // Verificar se as linhas estão pendentes
            List<Dictionary<string, object>> pickingLineRows = Utilities.performQuery(PriEngine.PickingDBConnString,
                "SELECT * FROM LinhaPicking WHERE id_picking = '@0@' AND em_progresso = 1",
                pickingWaveID, funcionarioID.ToString());
            if (pickingLineRows.Count < 1)
                throw new InvalidOperationException("The picking wave is concluded");*/

            // Verificar se todas as linhas estão presentes
            StringBuilder conditionQuery = new StringBuilder();
            conditionQuery.Append("(id != '" + linhas.ElementAt(0).First + "'");
            foreach (var item in linhas) {
                conditionQuery.Append(" AND id != '" + item.First + "'");
            }
            conditionQuery.Append(")");
            List<Dictionary<string, object>> nonIncludedRows = Utilities.performQuery(PriEngine.PickingDBConnString,
                "SELECT * FROM LinhaPicking WHERE id_picking = '@0@' AND @1@",
                pickingWaveID, conditionQuery.ToString());
            if (nonIncludedRows.Count > 0)
                throw new InvalidOperationException("The request must include all lines");

           
            // Marcar a quantidade satisfeita na linha
            foreach (var linha in linhas) {
                // TODO verificar se a quantidade satisfeita e inferior a pedida e inferior a que existe actualmente
                List<Dictionary<string, object>> pickingLineRow = Utilities.performQuery(PriEngine.PickingDBConnString,
                   "SELECT quant_a_satisfazer FROM LinhaPicking INNER JOIN PickingWave ON (LinhaPicking.id_picking = PickingWave.id) WHERE LinhaPicking.id = '@0@' AND LinhaPicking.id_picking = '@1@' AND em_progresso = 1",
                   linha.First, pickingWaveID);

                if (pickingLineRow.Count < 1)
                    continue;

                else if (Convert.ToInt32(pickingLineRow.ElementAt(0)["quant_a_satisfazer"]) < Convert.ToInt32(linha.Second))
                    throw new InvalidOperationException("A quantidade satisfeita é superior à pedida");


                List<Dictionary<string, object>> updatedRow = Utilities.performQuery(PriEngine.PickingDBConnString,
                    "UPDATE LinhaPicking SET quant_recolhida = @1@ OUTPUT INSERTED.id_linha_encomenda WHERE id = '@0@' AND id_picking = '@2@'",
                    linha.First, linha.Second.ToString(), pickingWaveID);

                if (updatedRow.Count < 1)       // A linha não pertence à picking order
                    continue;

                Utilities.performQuery(PriEngine.PickingDBConnString,
                    "UPDATE LinhaEncomenda SET quant_satisfeita = quant_satisfeita + @1@ WHERE id = '@0@'",
                    updatedRow.ElementAt(0)["id_linha_encomenda"].ToString(), linha.Second.ToString());
            }

            // Marcar a picking order como completa
            Utilities.performQuery(PriEngine.PickingDBConnString,
                    "UPDATE PickingWave SET em_progresso = 0, data_conclusao = GETDATE() WHERE id = '@0@'",
                    pickingWaveID);

            // Realizar tranferencia de armazem para EXPED
            // Retirar da quantidade reservada
            List<Dictionary<string, object>> affectedRows = Utilities.performQuery(PriEngine.PickingDBConnString,
               "SELECT artigo, quant_a_satisfazer, quant_recolhida, localizacao FROM LinhaPicking WHERE id_picking = '@0@'",
               pickingWaveID);

            List<Triple<string, string, int>> toReplenish = new List<Triple<string, string, int>>();
            foreach (var item in affectedRows) {
                List<TransferenciaArtigo> lista = new List<TransferenciaArtigo>();
                lista.Add(new TransferenciaArtigo(item["artigo"] as string, item["localizacao"] as string, GeneralConstants.ARMAZEM_EXPEDICAO, GeneralConstants.ARMAZEM_EXPEDICAO, Convert.ToDouble(item["quant_recolhida"])));         // TODO testar

                GerarTransferenciaArmazem(new TransferenciaArmazem("A1", serie, lista));
                Utilities.performQuery(PriEngine.PickingDBConnString,
                    "UPDATE QuantidadeReserva SET quant_reservada = quant_reservada - @0@ WHERE artigo = '@1@' AND armazem = 'A1'",
                    item["quant_a_satisfazer"].ToString(), item["artigo"] as string);

                toReplenish.Add(new Triple<string, string, int> { First = item["artigo"] as string, Second = item["localizacao"] as string, Third = Convert.ToInt32(item["quant_a_satisfazer"]) });
            }

            GerarReplenishment(toReplenish);

            return true;
        }

        public static bool AreReplenishmentPending() {
            List<Dictionary<string, object>> rows = Utilities.performQuery(PriEngine.PickingDBConnString,
                "SELECT * FROM LinhaReplenishment WHERE id_replenishment IS NULL");

            return rows.Count > 0;
        }

        // O replenishment occore no armazém primário 'A1'
        public static bool GerarReplenishment(List<Triple<string, string, int>> listaReposicao) {

            if(listaReposicao == null || listaReposicao.Count < 1)
                throw new InvalidOperationException("Invalid argument");

            StringBuilder queryString = new StringBuilder("INSERT INTO LinhaReplenishment(artigo, localizacao_destino, quant_a_satisfazer) VALUES('");
            Triple<string, string, int> firstLine = listaReposicao.ElementAt(0);
            queryString.Append(firstLine.First);
            queryString.Append("', '");
            queryString.Append(firstLine.Second);
            queryString.Append("', ");
            queryString.Append(firstLine.Third);
            queryString.Append(")");

            for (int i = 1; i < listaReposicao.Count; i++) {
                Triple<string, string, int> itemARepor = listaReposicao.ElementAt(i);
                string artigo = itemARepor.First;
                string localizacaoDestino = itemARepor.Second;
                int quantidadeARepor = itemARepor.Third;

                queryString.Append(", ('");
                queryString.Append(artigo);

                queryString.Append("', '");
                queryString.Append(localizacaoDestino);

                queryString.Append("', ");
                queryString.Append(quantidadeARepor);
                queryString.Append(")");
            }

            Utilities.performQuery(PriEngine.PickingDBConnString,
                queryString.ToString());

            return true;
        }

        // TODO: lançar uma warning a alertar para o facto de o armazem não ter stock de reposição
        // TODO: reservar o stock
        public static Wave<ReplenishmentLine> GetProximaReplenishmentOrder(int funcionarioID) {

            // Verificar se o funcionário existe no sistema
            List<Dictionary<string, object>> workerRows = Utilities.performQuery(PriEngine.PickingDBConnString, "SELECT * FROM Funcionario WHERE id = @0@", funcionarioID.ToString());
            if (workerRows.Count != 1)
                throw new InvalidOperationException("There is no such worker");

            // Verificar se o funcionário tem orders pendentes
            List<Dictionary<string, object>> pendingRows = Utilities.performQuery(PriEngine.PickingDBConnString, 
                "SELECT PickingWave.id FROM PickingWave INNER JOIN Funcionario ON (PickingWave.id_funcionario = Funcionario.id) WHERE PickingWave.em_progresso = 1 AND Funcionario.id = @0@" +
                "UNION" +
                "SELECT ReplenishmentWave.id FROM ReplenishmentWave INNER JOIN Funcionario ON (ReplenishmentWave.id_funcionario = Funcionario.id) WHERE ReplenishmentWave.em_progresso = 1 AND Funcionario.id = @0@", 
                funcionarioID.ToString());
            if(pendingRows.Count > 1)
                throw new InvalidOperationException("The worker has pending waves");

            List<Dictionary<string, object>> rowsToAssign = Utilities.performQuery(PriEngine.PickingDBConnString,
                "SELECT * FROM LinhaReplenishment WHERE id_replenishment IS NULL", funcionarioID.ToString());           // Abordagem gananciosa? ORDER BY quant_a_repor DESC?


            List<string> consideredLines = new List<string>();
            Random random = new Random();
            int currentWorkerCapacity = 0,
                index = 0;
            while (currentWorkerCapacity <= MAX_CAP_FUNCIONARIO && index < rowsToAssign.Count) {
                Dictionary<string, object> linha = rowsToAssign.ElementAt(index);
                string artigo = linha["artigo"].ToString(),
                        id = linha["id"].ToString();
                int quantidade = Convert.ToInt32(linha["quant_a_satisfazer"]);

                if (currentWorkerCapacity + quantidade > MAX_CAP_FUNCIONARIO) {
                    index++;
                    continue;
                }

                // Determinar a localização (estratégia: localização aleatória)
                // O produto deve estar disponível nos pisos de replenishment (pisos 2-9), no armazém primário (A1)
                List<Dictionary<string, object>> localizacoesRows = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM ArtigoArmazem WITH (NOLOCK) WHERE Artigo = '@0@' AND Localizacao LIKE 'A1.[A-Z].[2-9].[0-9][0-9][0-9]'", artigo);
                if (localizacoesRows.Count < 1)
                    throw new InvalidOperationException("The product " + linha["Artigo"].ToString() + " is not available in the main warehouse");
                string localizacao = localizacoesRows.ElementAt(random.Next(0, localizacoesRows.Count - 1))["Localizacao"] as string;
                if (localizacao == null)
                    throw new InvalidOperationException("Bad location name: " + localizacao);

                // Aceitar a localizacao
                Utilities.performQuery(PriEngine.PickingDBConnString,
                    "UPDATE LinhaReplenishment SET localizacao_origem = '@0@' WHERE id = '@1@'", localizacao, id);


                currentWorkerCapacity += quantidade;
                consideredLines.Add(id);

                index++;
            }


            // TODO: pode acontecer caso a quantidadeASatisfazer for sempre superior à capacidade máxima do funcionário
            if (consideredLines.Count < 1)
                throw new InvalidOperationException("No lines considered");

            // Criar replenishment wave
            List<Dictionary<string, object>> replenishmentWaveResult = Utilities.performQuery(PriEngine.PickingDBConnString,
                    "INSERT INTO ReplenishmentWave(id_funcionario) OUTPUT INSERTED.id VALUES(@0@)", funcionarioID.ToString());
            if (replenishmentWaveResult.Count < 1)
                throw new InvalidOperationException("A new picking wave could not be created");

            // Agrupar as linhas seleccionadas
            StringBuilder queryString = new StringBuilder();
            queryString.Append("(LinhaReplenishment.id = '" + consideredLines.ElementAt(0) + "'");
            for (int i = 1; i < consideredLines.Count; i++) {
                queryString.Append(" OR LinhaReplenishment.id = '" + consideredLines.ElementAt(i) + "'");
            }
            queryString.Append(")");
            Utilities.performQuery(PriEngine.PickingDBConnString,
                    "UPDATE LinhaReplenishment SET id_replenishment = '@0@' WHERE @1@",
                    replenishmentWaveResult.ElementAt(0)["id"].ToString(), queryString.ToString());


            // Estabelecer a rota: Ordenação alfabética das localizações
            // Pressupõe-se que a localização de reposição e a posição final apenas variam em piso
            List<Dictionary<string, object>> replenishmentLinesRows = Utilities.performQuery(PriEngine.PickingDBConnString,
                    "SELECT * FROM LinhaReplenishment WHERE @0@ ORDER BY localizacaoDestino",                                   // Assim, o piso é fixado, sendo, por conseguinte, como uma picking wave
                    queryString.ToString());

            Dictionary<string, List<ReplenishmentLine>> replenishmentWaveContent = new Dictionary<string, List<ReplenishmentLine>>();

            foreach (var line in replenishmentLinesRows) {
                List<ReplenishmentLine> pl;
                string location = line["localizacao_origem"] as string;
                if (!replenishmentWaveContent.TryGetValue(location, out pl)) {
                    pl = new List<ReplenishmentLine>();
                    pl.Add(new ReplenishmentLine(line["artigo"] as string, Convert.ToInt32(line["quant_a_satisfazer"]), line["unidade"] as string, location, line["localizacao_destino"] as string));
                    replenishmentWaveContent.Add(location, pl);

                } else
                    pl.Add(new ReplenishmentLine(line["artigo"] as string, Convert.ToInt32(line["quant_a_satisfazer"]), line["unidade"] as string, location, line["localizacao_destino"] as string));
            }

            // Adicionar à picking order
            return new Wave<ReplenishmentLine>(replenishmentWaveResult.ElementAt(0)["id"].ToString(), Convert.ToInt32(workerRows.ElementAt(0)["id"]), replenishmentWaveContent);
        }
        
        // Argumento: linhas de pares id_da_linha - quantidade_de_facto_mudada
        public static bool terminarReplenishmentOrder(int funcionarioID, string replenishmentWaveID, List<Pair<string, int>> linhas, string serie) {

            if (linhas.Count < 1)
                throw new InvalidOperationException("A replenishment wave must have, at least, one line");

            // Verificar se este funcionario tem a replenishment wave atribuída a ele + verificar se a picking wave existe + Verificar se o funcionário existe no sistema + verificar se a picking wave está pendente
            List<Dictionary<string, object>> replenishmentWavesRows = Utilities.performQuery(PriEngine.PickingDBConnString,
                "SELECT * FROM ReplenishmentWave WHERE id = '@0@' AND id_funcionario = '@1@' AND em_progresso = 1",
                replenishmentWaveID, funcionarioID.ToString());
            if (replenishmentWavesRows.Count != 1)
                throw new InvalidOperationException("There is no such replenishment wave");
            /*
            // Verificar se as linhas estão pendentes
            List<Dictionary<string, object>> pickingLineRows = Utilities.performQuery(PriEngine.PickingDBConnString,
                "SELECT * FROM LinhaPicking WHERE id_picking = '@0@' AND em_progresso = 1",
                pickingWaveID, funcionarioID.ToString());
            if (pickingLineRows.Count < 1)
                throw new InvalidOperationException("The picking wave is concluded");*/

            // Verificar se todas as linhas estão presentes
            StringBuilder conditionQuery = new StringBuilder();
            conditionQuery.Append("(id != '" + linhas.ElementAt(0).First + "'");
            foreach (var item in linhas) {
                conditionQuery.Append(" AND id != '" + item.First + "'");
            }
            conditionQuery.Append(")");
            List<Dictionary<string, object>> nonIncludedRows = Utilities.performQuery(PriEngine.PickingDBConnString,
                "SELECT * FROM LinhaReplenishment WHERE id_replenishment = '@0@' AND @1@",
                replenishmentWaveID, conditionQuery.ToString());
            if (nonIncludedRows.Count > 0)
                throw new InvalidOperationException("The request must include all lines");


            // Marcar a quantidade satisfeita na linha
            foreach (var linha in linhas) {
                // TODO verificar se a quantidade satisfeita e inferior a pedida e inferior a que existe actualmente
                Utilities.performQuery(PriEngine.PickingDBConnString,
                    "UPDATE LinhaReplenishment SET quant_recolhida = @1@ OUTPUT INSERTED.id WHERE id = '@0@' AND id_replenishment = '@2@'",
                    linha.First, linha.Second.ToString(), replenishmentWaveID);
            }

            // Marcar a replenishment wave como completa
            Utilities.performQuery(PriEngine.PickingDBConnString,
                    "UPDATE ReplenishmentWave SET em_progresso = 0, data_conclusao = GETDATE() WHERE id = '@0@'",
                    replenishmentWaveID);

            // Realizar tranferencia de armazem
            // Retirar da quantidade reservada
            List<Dictionary<string, object>> afectedRows = Utilities.performQuery(PriEngine.PickingDBConnString,
               "SELECT artigo, quant_a_satisfazer, quant_recolhida, localizacaoOrigem, localizacaoDestino FROM LinhaReplenishment WHERE id_replenishment = '@0@'",
               replenishmentWaveID);

            foreach (var item in afectedRows) {
                List<TransferenciaArtigo> lista = new List<TransferenciaArtigo>();
                lista.Add(new TransferenciaArtigo(item["artigo"] as string, item["localizacaoOrigem"] as string, item["localizacaoDestino"] as string, GeneralConstants.ARMAZEM_EXPEDICAO, Convert.ToDouble(item["quant_recolhida"])));

                GerarTransferenciaArmazem(new TransferenciaArmazem("A1", serie, lista));
                /*
                Utilities.performQuery(PriEngine.PickingDBConnString,
                    "UPDATE QuantidadeReserva SET quant_reservada = quant_reservada - @0@ WHERE artigo = '@1@' AND armazem = 'A1'",
                    item["quant_a_satisfazer"].ToString(), item["artigo"] as string);
                 * */
            }

            return true;
        }

        

        // Obter a wave que está pendente e activa para uma dado funcionário

        /*
        // Não funciona!
        public static void testFunction(string filial, string serie, uint nDoc, string artigo, double quantSatisfeita) {

            if (PriEngine.IniciaTransaccao()) {

                // Carregar encomenda de cliente
                GcpBEDocumentoVenda objEncomenda = PriEngine.Engine.Comercial.Vendas.Edita(filial, GeneralConstants.ENCOMENDA_CLIENTE_DOCUMENTO, serie, (int)nDoc);

                // A saída de stock é feita a partir do armazém de expedição
                objEncomenda.set_EmModoEdicao(true);
                GcpBELinhasDocumentoVenda linhasObjEncomenda = objEncomenda.get_Linhas();
                for (int i = 1; i <= linhasObjEncomenda.NumItens; i++) {
                    GcpBELinhaDocumentoVenda linha = linhasObjEncomenda[i];
                    System.Diagnostics.Debug.WriteLine(linha.get_Artigo() != artigo);
                    if (linha.get_Artigo() != artigo)
                        continue;

                    linha.set_Armazem("EXPED");
                    linha.set_Localizacao("EXPED");
                    linha.set_QuantSatisfeita(quantSatisfeita);
                }

                PriEngine.Engine.Comercial.Vendas.Actualiza(objEncomenda);
                objEncomenda.set_EmModoEdicao(false);
                PriEngine.TerminaTransaccao();

            } else
                return;

        }
        */

        #endregion Testes

    }
}

// TODO try...catch em cada controlador