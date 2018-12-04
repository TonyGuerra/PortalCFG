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

            LogFile.Log(" --- Tabelas_Cadastro:");

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
                cHtml = cHtml.Replace("!XJSPASTAS!"     , cJSPastas);

            } while (false);

            return cHtml;
        }

        public string Tabelas_Obter(HttpListenerRequest request, DBConnect MeuDBP, DBConnect MeuDB, ArtLib MeuLib, string cMeuPath, string cDados)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string cQueryString = HttpUtility.UrlDecode(request.Url.Query);
            string cJSon = cDados.Replace("dados=", "");
            dynamic oLogin = serializer.Deserialize<dynamic>(cJSon);
            string cHtml = "ERRO: Html nao atribuido";

            LogFile.Log(" --- Tabelas_Obter:");

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

                string cTabela = Convert.ToString(oLogin["tabela"]);
                string cTabOrigem = oLogin["taborigem"];
                string cCodigo = oLogin["codigo"];
                string cOperacao = oLogin["operacao"];
                string cFiltro = oLogin["filtro"];
                string cTabItens = "0";
                int nPastaItem = 0;

            //--------------- Dados da configuracao da tabela de itens
               string cQuery = string.Format("SELECT * FROM aa48itens WHERE id0aa40tabelas = {0} ", cTabela);

                List<string> campos = new List<string>(new string[] { "id1aa40tabelas", "PASTA" });

                MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["PASTA"].Count <= 0))
                {
                    LogFile.Log(string.Format("Tabelas_Obter: Sem itens! Tabela: {0}", cTabela));

                } else
                {
                    cTabItens = Convert.ToString(list["id1aa40tabelas"].First());
                    nPastaItem = Int32.Parse(list["PASTA"].First());
                }

            //--------------- Dados da configuracao da tabela do cabecalho
                cQuery = string.Format("SELECT * FROM aa40tabelas WHERE idSequencial = {0} ", cTabela);
                campos = new List<string>(new string[] { "TABELA", "PASTAS" });
                list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["TABELA"].Count <= 0))
                {
                    LogFile.Log(string.Format("Tabelas_Obter: Problema para obter os dados da tabela de cabecalho! Tabela: {0}", cTabela));
                    break;
                }

                string cNomeTabela = list["TABELA"].First();
                string cPastas = list["PASTAS"].First();

                cPastas = (String.IsNullOrEmpty(cPastas) ? "0=Dados" : cPastas);

                cCodigo = (cOperacao == "3" ? "-1" : cCodigo);

                LogFile.Log("cTabela........: " + cTabela);
                LogFile.Log("cCodigo........: " + cCodigo);
                LogFile.Log("cTabOrigem.....: " + cTabOrigem);
                LogFile.Log("cPastas........: " + cPastas);
                LogFile.Log("cNomeTabela....: " + cNomeTabela);
                LogFile.Log("cTabItens......: " + cTabItens);

                //0=Dados;1=Funcoes
                var aPastas = cPastas.Split(';');

            //--------------- Campos da tabela
                cQuery = "SELECT * FROM aa41campos WHERE id0aa40tabelas = " + cTabela + " ORDER BY ORDEM ";

                campos = new List<string>(new string[] { "CAMPO", "TITULO", "TIPO", "TAMANHO", "CASADECIMAL", "OPCOES", "OBRIGATORIO", "PASTA", "EDICAO", "ALTERACAO", "CONSULTATIPO",
                                                         "CONSULTACODIGO", "CONSULTACAMPO", "CONSULTACONDICAO", "CONSULTASEQ", "CONSULTABLK", "DESTINO", "POSICAO", "PADRAO",
                                                         "REVELADOR", "REVELARCAMPO", "REVELARVALOR", "GATILHOTIPO", "GATILHOCAMPO", "ULTIMOVALOR", "ALTURA", "CHARCASE", "TELA",
                                                         "TIPOPADRAO", "COMENTARIO", "EDITCOND" });

                MultiValueDictionary<string, string> lsCampos = MeuDBP.Select(cQuery, campos);

                if ((lsCampos.Count < campos.Count) || (lsCampos["CAMPO"].Count <= 0))
                {
                    LogFile.Log(string.Format("Tabelas_Obter: Problema para obter os campos da tabela! Tabela: {0}", cTabela));
                    break;
                }

            //--------------- Dados da tabela 
                cQuery = "SELECT * FROM " + cNomeTabela + " WHERE idSequencial = " + cCodigo;
                campos = new List<string>(new string[] {});

                for (int i=0; i < lsCampos["CAMPO"].Count; i++) { campos.Add(lsCampos["CAMPO"].ElementAt(i)); }

                MultiValueDictionary<string, string>  lsDados = MeuDB.Select(cQuery, campos);

                if ((lsDados.Count < campos.Count) || (lsDados[lsCampos["CAMPO"].First()].Count <= 0))
                {
                    LogFile.Log(string.Format("Tabelas_Obter: Problema para obter os dados da tabela! Tabela: {0}", cTabela));
                    break;
                }

             //--------------- Iniciando o JSon de Dados 

                cJSon = "{\"xconteudo\" : [";
                string cDisplay = "block";

                for(int i=0; i < aPastas.Length; i++)
                {
                    if (i > 0) { cJSon += ","; }

                    cJSon += "{";
                    cJSon += string.Format("\"xpasta\"   : \"tab{0}\","    , (i+1).ToString());
                    cJSon += string.Format("\"xdisplay\" : \"display{0}\",", cDisplay);

                    cDisplay = "none";

                    cJSon += "\"xcampos\" : [";

                //--------------- CABEÇALHO

                    cJSon += Tabelas_Obter_Cabecalho(MeuDB, MeuLib, lsCampos, lsDados, cTabOrigem, cNomeTabela, cCodigo, cOperacao, i);

                    cJSon += "],";
                }

                LogFile.Log("cJSon:");
                LogFile.Log(cJSon);

                cHtml = cJSon;

            } while (false);

            return cHtml;
        }

        private string Tabelas_Obter_Cabecalho(DBConnect MeuDB, ArtLib MeuLib, MultiValueDictionary<string, string> lsCampos, MultiValueDictionary<string, string> lsDados, string cTabOrigem, string cNomeTabela, string cCodigo, string cOperacao, int nPasta)
        {
            int nSeq = 0;
            string cSeq = "";
            int nLin = 0;      //Alternar a cor
            string cJSon = "";
            MultiValueDictionary<int,string> aValores = new MultiValueDictionary<int,string>();  //aValores.Add(0, "VALOR1"); aValores.Add(0, "VALOR2");

            for (int i=0; i < lsCampos["CAMPO"].Count; i++)
            {
                //Se não for da pasta ignora
                if((Int32.Parse(lsCampos["PASTA"].ElementAt(i)) != nPasta)) { continue; }

                string cCampo          = lsCampos["CAMPO"].ElementAt(i);
                string cCmpTipo        = lsCampos["TIPO"].ElementAt(i).Trim();
                string cCmpValPadrao   = lsCampos["PADRAO"].ElementAt(i).Trim();
                string cCmpEdicao      = lsCampos["EDICAO"].ElementAt(i);
                string cCmpAlteracao   = lsCampos["ALTERACAO"].ElementAt(i);
                string cCmpTpConsulta  = lsCampos["CONSULTATIPO"].ElementAt(i);
                string cCmpOpcoes      = lsCampos["OPCOES"].ElementAt(i);
                string cCmpTela        = lsCampos["TELA"].ElementAt(i);
                string cCmpIDRevelar   = lsCampos["REVELARCAMPO"].ElementAt(i);
                string cCmpVlrRevelar  = lsCampos["REVELARVALOR"].ElementAt(i);
                string cCmpTpGatilho   = lsCampos["GATILHOTIPO"].ElementAt(i);
                string cCmpIDGatilho   = lsCampos["GATILHOCAMPO"].ElementAt(i);
                string cCmpUltimoValor = lsCampos["ULTIMOVALOR"].ElementAt(i);
                string cCmpCharCase    = lsCampos["CHARCASE"].ElementAt(i).Trim();

                string cCmpRevelador   = (lsCampos["REVELADOR"].ElementAt(i) == "S" ? "1" : "0");
                string cCmpAltura      = (String.IsNullOrEmpty(lsCampos["ALTURA"].ElementAt(i)) || (lsCampos["ALTURA"].ElementAt(i) == "0") ? "400" : lsCampos["ALTURA"].ElementAt(i)) + "px";
                string cCmpObriga      = (lsCampos["OBRIGATORIO"].ElementAt(i) == "S" ? "1" : "0");
                Boolean lDireita       = (lsCampos["POSICAO"].ElementAt(i) == "2");

                string cCmpGatilho    = "ok";
                string cCmpCheckBox   = "";
                string cCmpSequencia  = "";
                string cCmpBloco      = "";
                string cTabCheckBox   = "";
                string cCondCheckBox  = "";
                string cDestCheckBox  = "";
                Boolean lCheckBox     = false;
                Boolean lCheckNum     = false;
                string cType          = "6";
                string cValor         = "";
                string cLinCor        = "";
                string cGatilhoClasse = "x";
                MultiValueDictionary<int, string> aOpcoes = new MultiValueDictionary<int, string>();


                if (lsCampos["TIPOPADRAO"].ElementAt(i) == "3")
                {
                    cCmpValPadrao = MeuDB.FMacro(MeuDB, cCmpValPadrao, cTabOrigem);
                }

                cCmpValPadrao = cCmpValPadrao.Replace("|||XTABORIGEM|||", cTabOrigem);

                if ( cCmpValPadrao.Contains("|X") && cCmpValPadrao.Contains("X|"))
                {
                    for(int j=0; j < aValores.Count; j++)
                    {
                        cCmpValPadrao = cCmpValPadrao.Replace(aValores[j].ElementAt(0), aValores[j].ElementAt(1));
                    }
                }

                Boolean lComboBox = !( String.IsNullOrEmpty(lsCampos["OPCOES"].ElementAt(i)) );

                if (lComboBox)
                {
                    lComboBox = !( lsCampos["OPCOES"].ElementAt(i).Contains("!X") ); //!XPASTAS!
                }

                string cSoLeitura = "0";

                if ( "25".Contains(cOperacao) || "36".Contains(cCmpEdicao) ||
                     ((cOperacao == "4") && (cCmpAlteracao == "2")) ||
                     ((cOperacao == "3") && (cCmpAlteracao == "3")) )
                {
                    cSoLeitura = "1";
                }

                /* CONSULTA COMBO */ if (cCmpTpConsulta == "1")
                {
                    int nCodConsulta     = Int32.Parse(lsCampos["CONSULTACODIGO"].ElementAt(i));
                    int nCmpConsulta     = Int32.Parse(lsCampos["CONSULTACAMPO"].ElementAt(i));
                    string cCondConsulta = lsCampos["CONSULTACONDICAO"].ElementAt(i);

                    cCmpOpcoes = MeuDB.FConsultaCombo(MeuDB, cTabOrigem, nCodConsulta, nCmpConsulta, cCondConsulta);

                } /* CONSULTA COMBO */ else

                /* CHECKBOX */ if (cCmpTpConsulta == "3")
                {
                    cTabCheckBox  = lsCampos["CONSULTACODIGO"].ElementAt(i);
                    cCmpCheckBox  = lsCampos["CONSULTACAMPO"].ElementAt(i);
                    cCondCheckBox = lsCampos["CONSULTACONDICAO"].ElementAt(i);
                    cDestCheckBox = lsCampos["DESTINO"].ElementAt(i);
                    lCheckBox     = true;
                    cType         = "6";

                    cValor        = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : "0"));

                } /* CHECKBOX */ else

                /* PERIODO */ if (cCmpTpConsulta == "4")
                {
                    cType = "7";

                    cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : "0000000000000000000000"));

                } /* PERIODO */ else

                /* CHECKNUM */
                if (cCmpTpConsulta == "4")
                {
                    cTabCheckBox  = lsCampos["CONSULTACODIGO"].ElementAt(i);
                    cCmpCheckBox  = lsCampos["CONSULTACAMPO"].ElementAt(i);
                    cCondCheckBox = lsCampos["CONSULTACONDICAO"].ElementAt(i);
                    cCmpSequencia = lsCampos["CONSULTASEQ"].ElementAt(i);
                    cCmpBloco     = lsCampos["CONSULTABLQ"].ElementAt(i);
                    cDestCheckBox = lsCampos["DESTINO"].ElementAt(i);
                    lCheckNum     = true;
                    cType         = "8";

                    cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : "0"));

                } /* CHECKNUM */ else

                if ((cCmpTipo == "C") || lComboBox)
                {
                    /* SELECT */ if ( lComboBox )
                    {
                        aOpcoes = MeuLib.FMatriz(cCmpOpcoes);
                        cType = "2";

                        cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : "0"));

                    } /* SELECT */

                    /* TEXT/PASSWORD/HIDDEN */ if ("12345679".Contains(cCmpEdicao))
                    {
                        int nPos = "12345679".IndexOf(cCmpEdicao);
                        cType = "11111149".Substring(nPos, 1);

                        cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : cCmpValPadrao);

                    } /* TEXT/PASSWORD/HIDDEN */

                } else

                /* MEMO */ if (cCmpTipo == "M")
                {
                    cType = "3";

                    cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : cCmpValPadrao);
                                       
                } /* MEMO */ else

                /* DATA */ if (cCmpTipo == "D")
                {
                    cType = "5";

                    cCmpValPadrao = (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : "  /  /    ");

                    cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : cCmpValPadrao);

                } /* DATA */ else

                /* HORA */ if (cCmpTipo == "H")
                {
                    cType = "1";

                    cCmpValPadrao = (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : "  :  ");

                    cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : cCmpValPadrao);

                } /* HORA */ else

                /* NUMERICO */ if (cCmpTipo == "N")
                {
                    cType = "1";

                    cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao.Trim() : "0"));

                } /* NUMERICO */

                cValor.Replace("\r", "\n");
                cValor.Replace("\"", "'");

                if ((cCmpTela == "2") || (cCmpEdicao == "9")) { cType = "9"; }

            //--------------- JSON

                if ( !lDireita ) { ++nSeq; }
                if ( !lDireita && (cType != "9")) { ++nLin; }

                if (cType == "9")
                {
                    cCmpIDRevelar  = "esconder";
                    cCmpVlrRevelar = "esconder";
                } else
                {
                    cLinCor = ((nLin % 2) == 0 ? "1" : "0");
                }

                if ((nSeq > 1) && !lDireita) { cJSon += "}]"; }

                if ((nSeq <= 1) && !lDireita) { cJSon += "{"; }
                else { cJSon += "},{"; }

                if (!lDireita)
                {
                    cSeq = ("00" + nSeq.ToString());
                    cSeq = cSeq.Substring(cSeq.Length - 2, 2);

                    cJSon += String.Format(" \"classe\": \"{0}\",", cLinCor);
                    cJSon += String.Format(" \"nome\": \"xlinha{0}\",", cSeq);
                    cJSon += String.Format(" \"rev1\": \"{0}\",", cCmpIDRevelar);
                    cJSon += String.Format(" \"rev2\": \"{0}\",", cCmpVlrRevelar);
                    cJSon += " \"xlinha\": [{";
                }

                /* GATILHO CONSULTA */ if (cCmpTpGatilho == "1")
                {
                    cGatilhoClasse = "xgatilho";

                    string cQuery = string.Format("SELECT CAMPO FROM aa41campos WHERE idSequencial = {0} ", cCmpIDGatilho);

                    List<string> campos = new List<string>(new string[] { "CAMPO" });

                    MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                    if ( !( (list.Count < campos.Count) || (list["CAMPO"].Count <= 0) ) )
                    {
                        cCmpGatilho = list["CAMPO"].First();
                    }
                } /* GATILHO CONSULTA */


                /* GATILHO REVELADOR DE LINHA */ if (cCmpRevelador == "1")
                {
                    cGatilhoClasse = "xgatilho";

                } /* GATILHO REVELADOR DE LINHA */


                /* INCLUSAO - ULTIMO VALOR */ if ((cOperacao == "3") && (cCmpUltimoValor == "S"))
                {
                    string cQuery = string.Format("SELECT Max({0}) AS {1} FROM {2} ", cCampo, cCampo, cNomeTabela);

                    List<string> campos = new List<string>(new string[] { cCampo });

                    MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                    if (!((list.Count < campos.Count) || (list[cCampo].Count <= 0)))
                    {
                        cValor = list[cCampo].First();
                    }
                }

                cValor = (String.IsNullOrEmpty(cValor) && !String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : cValor);

                cJSon += String.Format("  \"sequencia\": {0}", nSeq.ToString());
                cJSon += String.Format(", \"titulo\": \"{0}:\"", lsCampos["TITULO"].ElementAt(i));
                cJSon += String.Format(", \"titulo2\": \"{0}:\"", lsCampos["COMENTARIO"].ElementAt(i));
                cJSon += String.Format(", \"campo\": \"{0}\"", cCampo);
                cJSon += String.Format(", \"tamanho\": \"{0}\"", lsCampos["TAMANHO"].ElementAt(i));
                cJSon += String.Format(", \"altura\": \"{0}\"", cCmpAltura);
                cJSon += String.Format(", \"valor\": \"{0}\"", cValor);
                cJSon += String.Format(", \"config\": \"{0}{1}{2}{3}{4}\"", cType, cSoLeitura, cCmpRevelador, cCmpObriga, cCmpCharCase);
                cJSon += String.Format(", \"editcond\": \"{0}\"", lsCampos["EDITCOND"].ElementAt(i));
                cJSon += String.Format(", \"gatilhoclasse\": \"{0}\"", cGatilhoClasse);
                cJSon += String.Format(", \"gatilhoid\": \"{0}\"", cCmpIDGatilho);
                cJSon += String.Format(", \"gatilhocampo\": \"{0}\"", cCmpGatilho);

                if (lComboBox)
                {
                    cJSon += ", \"option\"   : [";

                    for(int j=0; j < aOpcoes.Count; j++)
                    {
                        if (j > 0) { cJSon += ","; }

                        cJSon += " {";
                        cJSon += String.Format("\"id\": \"{0}\", \"name\": \"{1}\"", aOpcoes[j].ElementAt(0), aOpcoes[j].ElementAt(1));
                        cJSon += "}";
                    }

                    cJSon += " ]";
                }

                if (lCheckBox)
                {
                    cJSon += ", \"checks\"   : [";

                    cJSon += MeuDB.FVerCheck(MeuDB, cNomeTabela, cCodigo, cTabCheckBox, cCmpCheckBox, cCondCheckBox, cDestCheckBox);

                    cJSon += " ]";
                }

                if (lCheckNum)
                {
                    cJSon += ", \"checks\"   : [";

                    cJSon += MeuDB.FVerCheckNum(MeuDB, cNomeTabela, cCodigo, cTabCheckBox, cCmpCheckBox, cCmpSequencia, cCmpBloco, cCondCheckBox, cDestCheckBox);

                    cJSon += " ]";
                }

                aValores.Add(i, "|X" + cCampo + "X|");  //Macro substituição de campos
                aValores.Add(i, cValor);

            } //for(i)(lsCampos)

            cJSon = (String.IsNullOrEmpty(cJSon) ? "" : cJSon+"}] }");

            cJSon = MeuLib.JSONAcento(cJSon);

            cJSon = cJSon.Replace("\\x", "!!x");

            return cJSon;
        }
    }
}
