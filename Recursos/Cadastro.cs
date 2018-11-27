using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Web.Script.Serialization;

namespace Recursos
{
    public class Cadastro
    {
        public string Tabelas_Cadastro(HttpListenerRequest request, DBConnect MeuDB, ArtLib MeuLib, string cMeuPath, string cDados)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string cQueryString = HttpUtility.UrlDecode(request.Url.Query);
            string cJSon = cDados.Replace("dados=", "");
            dynamic oLogin = serializer.Deserialize<dynamic>(cJSon);
            string cHtml = "ERRO: Html nao atribuido";

            LogFile.Log(" --- Tabelas_Filtro:");

            if (!String.IsNullOrEmpty(cQueryString))
            {
                int nP = (cQueryString.Contains("dados=") ? 7 : 1);
                cJSon = MeuLib.Base64Decode(cQueryString.Substring(nP));
                oLogin = serializer.Deserialize<dynamic>(cJSon);
            }

            do
            {

                cHtml = Distribuidor.Generico(request, MeuDB, MeuLib, cMeuPath, cJSon);

                if (cHtml.Contains("Sessao Expirou")) { break; }

                string cTabela       = Convert.ToString(oLogin["tabela"]);
                string cTabOrigem    = oLogin["taborigem"];
                string cTabPasta     = Convert.ToString(oLogin["tabela"]);
                string cOrigem       = oLogin["origem"];
                string[] aOp         = { "", "", "VISUALIZAR", "INCLUIR", "ALTERAR", "EXCLUIR", "", "" };
                string cOperacao     = oLogin["operacao"];
                string cDescOperacao = aOp[Int32.Parse(cOperacao)];
                string cCodigo       = "";
                string cTipoTabela   = oLogin["tipotabela"];
                string cTrace1       = oLogin["trace1"] + oLogin["trace2"];
                string cTrace2       = cTipoTabela + " Cadastro > ";
                string cChave        = "tabelas";
                string cCadastro     = cTipoTabela.ToUpper() + " CADASTRO ";
                string cMenu         = oLogin["menu"];
                string cPaginaAtu    = Convert.ToString(oLogin["paginaatu"]);
                string cPaginaFim    = Convert.ToString(oLogin["paginafim"]);
                string cFiltro       = oLogin["filtro"];

                cTabPasta = (String.IsNullOrEmpty(cTabPasta) ? cTabela : cTabPasta);

                if ("245".Contains(cOperacao)) { cCodigo = Convert.ToString(oLogin["codigo"]); }

            //------------------ Dados da configuracao da tabela

                string cQuery = string.Format("SELECT * FROM aa40tabelas WHERE idSequencial = {0} ", cTabela);

                List<string> campos = new List<string>(new string[] { "TABELA", "PASTAS" });

                MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["TABELA"].Count <= 0))
                {
                    LogFile.Log(string.Format("Tabelas_Cadastro: Problema para obter os dados da tabela! Tabela: {0}", cTabela));
                    break;
                }

                string cPastas        = list["PASTAS"].First();
                string cTabPastas     = "";
                string cNomeTabela    = list["TABELA"].First();
                string cNomeTabOrigem = "";

                cPastas = (String.IsNullOrEmpty(cPastas) ? "Dados" : cPastas);

                var aPastas = cPastas.Split(';');

            //------------------ Dados da pasta da tabela

                string[] aTabPastas = {};

                if (String.IsNullOrEmpty(cTabPasta) || (cTabPasta == cTabela))
                {
                    aTabPastas = aPastas;
                    cTabPastas = cPastas;
                } else
                {
                    cQuery = string.Format("SELECT * FROM aa40tabelas WHERE idSequencial = {0} ", cTabPasta);

                    campos = new List<string>(new string[] { "PASTAS" });

                    list = MeuDB.Select(cQuery, campos);

                    if ((list.Count < campos.Count) || (list["PASTAS"].Count <= 0))
                    {
                        LogFile.Log(string.Format("Tabelas_Cadastro: Problema para obter os dados da pasta da tabela! Tabela: {0}", cTabPasta));
                        break;
                    }

                    cTabPastas = list["PASTAS"].First();

                    aTabPastas = cTabPastas.Split(';');

                }

            //------------------ Dados da configuracao da tabela origem

                if (String.IsNullOrEmpty(cTabOrigem) || (cTabOrigem == cTabela))
                {
                    cNomeTabOrigem = cNomeTabela;
                }
                else
                {
                    cQuery = string.Format("SELECT * FROM aa40tabelas WHERE idSequencial = {0} ", cTabOrigem);

                    campos = new List<string>(new string[] { "TABELA" });

                    list = MeuDB.Select(cQuery, campos);

                    if ((list.Count < campos.Count) || (list["TABELA"].Count <= 0))
                    {
                        LogFile.Log(string.Format("Tabelas_Cadastro: Problema para obter os dados da tabela origem! Tabela: {0}", cTabOrigem));
                        break;
                    }

                    cNomeTabOrigem = list["TABELA"].First();

                }

                string cJSPastas = "";

                if (cTabela == "8")
                {
                    if (String.IsNullOrEmpty(cPastas))
                    {
                        cJSPastas = "self.items.push(new ItemViewModel(\"Dados\"));";
                    }
                    else
                    {
                        for (int i = 0; i < aPastas.Length; i++)
                        {
                            cJSPastas += string.Format("self.items.push(new ItemViewModel({0}));", aPastas[i]);
                        }
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(cTabPastas))
                    {
                        cJSPastas = "self.items.push(new ItemViewModel(\"Dados\"));";
                    }
                    else
                    {
                        for (int i = 0; i < aTabPastas.Length; i++)
                        {
                            cJSPastas += string.Format("self.items.push(new ItemViewModel(\"{0}\"));", aTabPastas[i]);
                        }
                    }
                }

                cHtml = cHtml.Replace("!XTABELA!"       , cTabela);
                cHtml = cHtml.Replace("!XLOGIN!"        , oLogin["login"]);
                cHtml = cHtml.Replace("!XSESSAO!"       , oLogin["sessao"]);
                cHtml = cHtml.Replace("!XMENU!"         , cMenu);
                cHtml = cHtml.Replace("!XTRACE1!"       , cTrace1);
                cHtml = cHtml.Replace("!XTRACE2!"       , cTrace2);
                cHtml = cHtml.Replace("!XCADASTRO!"     , cCadastro);
                cHtml = cHtml.Replace("!XOPERACAO!"     , cOperacao);
                cHtml = cHtml.Replace("!XPAGINAATU!"    , cPaginaAtu);
                cHtml = cHtml.Replace("!XPAGINAFIM!"    , cPaginaFim);
                cHtml = cHtml.Replace("!XMANUTENCAO!"   , cDescOperacao);
                cHtml = cHtml.Replace("!XTABELA!"       , cTabela);
                cHtml = cHtml.Replace("!XNOMETABELA!"   , cNomeTabela);
                cHtml = cHtml.Replace("!XTABORIGEM!"    , cTabOrigem);
                cHtml = cHtml.Replace("!XNOMETABORIGEM!", cNomeTabOrigem);
                cHtml = cHtml.Replace("!XCODIGO!"       , cCodigo);
                cHtml = cHtml.Replace("!XORIGEM!"       , cOrigem);
                cHtml = cHtml.Replace("!XCHAVE!"        , cChave);
                cHtml = cHtml.Replace("!XITEM!"         , "000");
                cHtml = cHtml.Replace("!XFILTRO!"       , cFiltro);
                cHtml = cHtml.Replace("!XNOMETABORIGEM!", cNomeTabOrigem);
                cHtml = cHtml.Replace("!XJSPASTAS!"     , cJSPastas);

            } while (false);

            return cHtml;
        }
    }
}
