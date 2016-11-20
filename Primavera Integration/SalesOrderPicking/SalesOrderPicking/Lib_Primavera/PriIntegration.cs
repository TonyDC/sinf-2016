using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Interop.ErpBS900;
using Interop.StdPlatBS900;
using Interop.StdBE900;
using Interop.GcpBE900;
using ADODB;
using SalesOrderPicking.Lib_Primavera.Model;

namespace SalesOrderPicking.Lib_Primavera {

    public class PriIntegration {

        #region Artigo
        public static List<Artigo> ListaArtigos() {

            if (PriEngine.IniciaTransaccao()) {
                StdBELista objLista = PriEngine.Engine.Consulta("SELECT * FROM Artigo WITH (NOLOCK)");

                List<Artigo> listaArtigos = new List<Artigo>();

                while (!objLista.NoFim()) {
                    listaArtigos.Add(new Artigo(objLista.Valor("artigo"), objLista.Valor("descricao")));
                    objLista.Seguinte();
                }

                PriEngine.TerminaTransaccao();
                return listaArtigos;

            } else
                return null;

        }

        public static Artigo ObterArtigo(string codArtigo) {

            if (PriEngine.IniciaTransaccao()) {

                StdBELista objLista = PriEngine.Engine.Consulta("SELECT * FROM Artigo WITH (NOLOCK) WHERE Artigo = '" + codArtigo + "'");

                if (objLista.NumLinhas() < 1) {
                    PriEngine.TerminaTransaccao();
                    return null;
                }

                StdBELista objListaStocks = PriEngine.Engine.Consulta("SELECT * FROM ArtigoArmazem WITH (NOLOCK) WHERE Artigo = '" + codArtigo + "' ORDER BY StkActual DESC");
                List<StockArtigo> listaStockArtigo = new List<StockArtigo>();

                while (!objListaStocks.NoFim()) {
                    listaStockArtigo.Add(new StockArtigo(objListaStocks.Valor("Armazem"),
                                                            objListaStocks.Valor("Localizacao"),
                                                            objListaStocks.Valor("Lote"),
                                                            objListaStocks.Valor("StkActual").ToString()));

                    objListaStocks.Seguinte();
                }

                Artigo artigo = new Artigo(objLista.Valor("Artigo"), objLista.Valor("Descricao"), listaStockArtigo);

                objLista.Fim();
                PriEngine.TerminaTransaccao();
                return artigo;

            } else
                return null;
        }

        #endregion Artigo


        #region Encomendas

        public static List<EncomendaCliente> GetEncomendasClientes() {

            if (PriEngine.IniciaTransaccao()) {

                // Performed query: SELECT CabecDoc.Entidade, LinhasDoc.Artigo, CabecDoc.TipoDoc, CabecDoc.NumDoc,LinhasDoc.Descricao,CabecDoc.Data, CabecDoc.Serie, CabecDoc.Filial, LinhasDoc.NumLinha FROM LinhasDoc INNER JOIN CabecDoc ON (LinhasDoc.IdCabecDoc = CabecDoc.Id) WHERE TipoDoc = 'ECL'
                // StdBELista listaEncomendas = PriEngine.Engine.Comercial.Vendas.LstLinhasDocVendas("TipoDoc = 'ECL'");

                StdBELista queryEncomendas = PriEngine.Engine.Consulta("SELECT * FROM CabecDoc WITH (NOLOCK) WHERE TipoDoc = 'ECL' ORDER BY NumDoc");
                List<EncomendaCliente> listaEncomendas = new List<EncomendaCliente>();

                while (!queryEncomendas.NoFim()) {

                    string encomendaID = queryEncomendas.Valor("Id");

                    // Seleccionar apenas os atributos pretendidos
                    StdBELista queryEncomendasPorCliente = PriEngine.Engine.Consulta("SELECT * FROM LinhasDoc WITH (NOLOCK) WHERE IdCabecDoc = '" + encomendaID + "' AND Artigo IS NOT NULL");
                    List<LinhaEncomendaCliente> artigosEncomenda = new List<LinhaEncomendaCliente>();

                    while (!queryEncomendasPorCliente.NoFim()) {

                        string id = queryEncomendasPorCliente.Valor("Id"),
                            artigo = queryEncomendasPorCliente.Valor("Artigo"),
                            //unidadeVenda = 
                            quantidade = queryEncomendasPorCliente.Valor("Quantidade").ToString(),
                            numLinha = queryEncomendasPorCliente.Valor("NumLinha").ToString();

                        artigosEncomenda.Add(new LinhaEncomendaCliente(id, artigo, quantidade, numLinha));
                        queryEncomendasPorCliente.Seguinte();
                    }

                    listaEncomendas.Add(new EncomendaCliente(encomendaID, queryEncomendas.Valor("NumDoc").ToString(), queryEncomendas.Valor("Filial"), queryEncomendas.Valor("Serie"), artigosEncomenda));
                    queryEncomendas.Seguinte();
                }

                PriEngine.TerminaTransaccao();
                return listaEncomendas;

            } else
                return null;
        }

        #endregion Encomendas



        #region Cliente

        public static List<Cliente> GetListaClientes() {

            if (PriEngine.IniciaTransaccao()) {

                List<Cliente> listaClientes = new List<Cliente>();
                StdBELista queryCliente = PriEngine.Engine.Consulta("SELECT * FROM Clientes WITH (NOLOCK)");

                while (!queryCliente.NoFim()) {
                    listaClientes.Add(new Cliente(queryCliente.Valor("Cliente"),
                                                    queryCliente.Valor("Nome")));

                    queryCliente.Seguinte();
                }

                PriEngine.TerminaTransaccao();
                return listaClientes;

            } else
                return null;
        }

        public static Cliente GetClienteInfo(string clienteID) {

            if (PriEngine.IniciaTransaccao()) {
                StdBELista queryCliente = PriEngine.Engine.Consulta("SELECT * FROM Clientes WITH (NOLOCK) WHERE Cliente = '" + clienteID + "'");

                if (queryCliente.NumLinhas() < 1) {
                    PriEngine.TerminaTransaccao();
                    return null;
                }

                Cliente cliente = new Cliente(queryCliente.Valor("Cliente"),
                                                    queryCliente.Valor("Nome"),
                                                    queryCliente.Valor("NomeFiscal"),
                                                    queryCliente.Valor("Fac_Mor"),
                                                    queryCliente.Valor("Fac_Local"),
                                                    queryCliente.Valor("Fac_Cp"),
                                                    queryCliente.Valor("Fac_Cploc"),
                                                    queryCliente.Valor("Fac_Tel"),
                                                    queryCliente.Valor("Pais"));

                queryCliente.Fim();
                PriEngine.TerminaTransaccao();
                return cliente;

            } else
                return null;
        }

        #endregion Cliente


        #region Armazem

        public static List<Armazem> GetArmazens() {

            if (PriEngine.IniciaTransaccao()) {
                List<Armazem> listaArmazens = new List<Armazem>();
                StdBELista queryArmazens = PriEngine.Engine.Consulta("SELECT * FROM Armazens WITH (NOLOCK)");

                while (!queryArmazens.NoFim()) {
                    listaArmazens.Add(new Armazem(queryArmazens.Valor("Armazem"),
                                                    queryArmazens.Valor("Descricao")));

                    queryArmazens.Seguinte();
                }

                PriEngine.TerminaTransaccao();
                return listaArmazens;

            } else
                return null;
        }


        public static bool GerarTransferenciaArmazem(string artigoID, string armazemOrigem, string armazemDestino, float quantidade) {
            return true;
        }


        // Determinar as varias localizacoes associadas ao armazem
        public static List<LocalizacaoArmazem> GetLocalizacoesArmazens(string armazemID) {

            if (PriEngine.IniciaTransaccao()) {
                List<LocalizacaoArmazem> listaArmazens = new List<LocalizacaoArmazem>();
                StdBELista queryArmazens = PriEngine.Engine.Consulta("SELECT * FROM ArmazemLocalizacoes");

                while (!queryArmazens.NoFim()) {
                    listaArmazens.Add(new LocalizacaoArmazem(queryArmazens.Valor("Id"),
                                                             queryArmazens.Valor("Localizacao"),
                                                             queryArmazens.Valor("Descricao"),
                                                             queryArmazens.Valor("NomeNivel")));

                    queryArmazens.Seguinte();
                }

                return listaArmazens;

            } else
                return null;

        }

        #endregion Armazem


        #region Testes

        public static bool GerarGuiaRemessa(PedidoTransformacaoECL encomenda) {

            if (PriEngine.IniciaTransaccao()) {

                // Carregar encomenda de cliente
                GcpBEDocumentoVenda objEncomenda = PriEngine.Engine.Comercial.Vendas.Edita(encomenda.Filial, GeneralConstants.ENCOMENDA_CLIENTE_DOCUMENTO, encomenda.Serie, Int16.Parse(encomenda.NumeroDocumento));

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


        /**
         * Request body: 
         * 
         * {
	            armazemOrigem: "A1",
	            serie: "2016",
	            artigos: [
		                    {
			                    artigo: "A0004",
			                    localizacaoOrigem: "A1.A.1.001",
			                    armazemDestino: "A1",
			                    localizacaoDestino: "A1.A.1.003",
			                    quantidade: "9"
		                    }
		                ]
            }
         * */
        public static bool GerarTransferenciaArmazem(TransferenciaArmazem lista) {

            if (PriEngine.IniciaTransaccao()) {

                System.Diagnostics.Debug.WriteLine(lista);

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

                    linhaDocStock.set_Artigo(item.ArtigoID);
                    linhaDocStock.set_LocalizacaoOrigem(item.LocalizacaoOrigem);
                    linhaDocStock.set_Localizacao(item.LocalizacaoDestino);
                    linhaDocStock.set_Armazem(item.ArmazemDestino);
                    linhaDocStock.set_Quantidade(Double.Parse(item.Quantidade));
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

        #endregion Testes

    }
}

// TODO try...catch em cada controlador
// TODO atencao ao SQL injection -> usar os controladores de base de dados nativos