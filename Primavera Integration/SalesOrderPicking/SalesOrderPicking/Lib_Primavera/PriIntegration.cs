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

namespace SalesOrderPicking.Lib_Primavera {

    public class PriIntegration {

        #region Artigo

        public static List<Artigo> ListaArtigos() {

            List<Artigo> listaArtigos = new List<Artigo>();

            List<Dictionary<string, object>> rows = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM Artigo WITH (NOLOCK)");

            foreach (var item in rows) {
                object artigoID, descricao;

                item.TryGetValue("Artigo", out artigoID);
                item.TryGetValue("Descricao", out descricao);

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

            object artigoID, descricao;
            Dictionary<string, object> linhaArtigo = artigo.ElementAt(0);
            linhaArtigo.TryGetValue("Artigo", out artigoID);
            linhaArtigo.TryGetValue("Descricao", out descricao);

            List<Dictionary<string, object>> stockArtigo = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM ArtigoArmazem WITH (NOLOCK) WHERE Artigo = '@0@' ORDER BY StkActual DESC", codArtigo);

            List<StockArtigo> listaStockArtigo = new List<StockArtigo>();

            for (int i = 0; i < stockArtigo.Count; i++) {

                Dictionary<string, object> line = stockArtigo.ElementAt(i);

                object armazem, localizacao, lote, stock;
                line.TryGetValue("Armazem", out armazem);
                line.TryGetValue("Localizacao", out localizacao);
                line.TryGetValue("Lote", out lote);
                line.TryGetValue("StkActual", out stock);

                listaStockArtigo.Add(new StockArtigo(armazem as string, localizacao as string, lote as string, stock.ToString()));
            }

            return new Artigo(artigoID as string, descricao as string, listaStockArtigo);
        }

        #endregion Artigo


        #region Encomendas

        public static List<EncomendaCliente> GetEncomendasClientes(string clienteID = null) {

            List<EncomendaCliente> listaArtigos = new List<EncomendaCliente>();

            List<Dictionary<string, object>> listaEncomendas = null;

            if (clienteID == null)
                listaEncomendas = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM CabecDoc WITH (NOLOCK) WHERE TipoDoc = 'ECL' ORDER BY NumDoc");
            else
                listaEncomendas = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM CabecDoc WITH (NOLOCK) WHERE TipoDoc = 'ECL' AND EntidadeFac = '@0@' ORDER BY NumDoc", clienteID);

            foreach (var item in listaEncomendas) {

                object encomendaID = item["Id"], filial = item["Filial"], serie = item["Serie"], numDoc = item["NumDoc"], cliente = item["EntidadeFac"];
                /*
                item.TryGetValue("Id", out encomendaID);
                item.TryGetValue("Filial", out filial);
                item.TryGetValue("Serie", out serie);
                item.TryGetValue("NumDoc", out numDoc);
                 * */

                List<Dictionary<string, object>> linhasEncomenda = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM LinhasDoc WITH (NOLOCK) WHERE IdCabecDoc = '@0@' AND Artigo IS NOT NULL", encomendaID.ToString());

                List<LinhaEncomendaCliente> artigosEncomenda = new List<LinhaEncomendaCliente>();

                foreach (var linha in linhasEncomenda) {
                    object linhaID, artigoID, quantidade, numLinha;
                    linha.TryGetValue("Id", out linhaID);
                    linha.TryGetValue("Artigo", out artigoID);
                    linha.TryGetValue("Quantidade", out quantidade);
                    linha.TryGetValue("NumLinha", out numLinha);

                    artigosEncomenda.Add(new LinhaEncomendaCliente(linhaID.ToString(), artigoID.ToString(), quantidade.ToString(), numLinha.ToString()));
                }

                listaArtigos.Add(new EncomendaCliente(encomendaID.ToString(), numDoc.ToString(), cliente.ToString(), serie.ToString(), filial.ToString(), artigosEncomenda));
            }

            return listaArtigos;
        }


        public static bool GerarGuiaRemessa(PedidoTransformacaoECL encomenda) {

            if (PriEngine.IniciaTransaccao()) {

                if (encomenda == null)
                    throw new InvalidOperationException("Pedido de encomenda inválido (parâmetro a null)");

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

        #endregion Encomendas



        #region Cliente

        public static List<Cliente> GetListaClientes() {

            List<Cliente> listaClientes = new List<Cliente>();
            List<Dictionary<string, object>> queryRows = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM Clientes WITH (NOLOCK)");

            foreach (var item in queryRows) {
                object id, nome;
                item.TryGetValue("Cliente", out id);
                item.TryGetValue("Nome", out nome);

                listaClientes.Add(new Cliente(id.ToString(), nome.ToString()));
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

            object id, nome, nomeFiscal, morada, local, codPostal, locCodPostal, telefone, pais;
            clientRow.TryGetValue("Cliente", out id);
            clientRow.TryGetValue("Nome", out nome);
            clientRow.TryGetValue("NomeFiscal", out nomeFiscal);
            clientRow.TryGetValue("Fac_Mor", out morada);
            clientRow.TryGetValue("Fac_Local", out local);
            clientRow.TryGetValue("Fac_Cp", out codPostal);
            clientRow.TryGetValue("Fac_Cploc", out locCodPostal);
            clientRow.TryGetValue("Fac_Tel", out telefone);
            clientRow.TryGetValue("Pais", out pais);

            return new Cliente(id.ToString(), nome.ToString(), nomeFiscal.ToString(), morada.ToString(), local.ToString(), codPostal.ToString(), locCodPostal.ToString(), telefone.ToString(), pais.ToString());
        }

        #endregion Cliente


        #region Armazem

        public static List<Armazem> GetArmazens() {

            List<Armazem> listaArmazens = new List<Armazem>();
            List<Dictionary<string, object>> queryRows = Utilities.performQuery(PriEngine.DBConnString, "SELECT * FROM Armazens WITH (NOLOCK)");

            foreach (var item in queryRows) {
                object id, descricao;
                item.TryGetValue("Armazem", out id);
                item.TryGetValue("Descricao", out descricao);

                listaArmazens.Add(new Armazem(id.ToString(), descricao.ToString()));
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
                object id, localizacao, descricao, nomeNivel;
                item.TryGetValue("Id", out id);
                item.TryGetValue("Localizacao", out localizacao);
                item.TryGetValue("Descricao", out descricao);
                item.TryGetValue("NomeNivel", out nomeNivel);

                listaLocalizacoesArmazem.Add(new LocalizacaoArmazem(id.ToString(), localizacao.ToString(), descricao.ToString(), nomeNivel.ToString()));
            }

            return listaLocalizacoesArmazem;
        }


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

        #endregion Armazem


        #region Testes

        #endregion Testes

    }
}

// TODO try...catch em cada controlador