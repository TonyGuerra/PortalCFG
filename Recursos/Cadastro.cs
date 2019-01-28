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

//Tabelas_Cadastro
//Tabelas_Obter
//Tabelas_Obter_Cabecalho
//Tabelas_Obter_Itens
//Tabelas_Valida
//Tabelas_Gravar

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

                string cTabela = oLogin["taborigem"]; //Convert.ToString(oLogin["tabela"]);
                string cTabOrigem = oLogin["taborigem"];
                string cTabPasta = oLogin["taborigem"]; //Convert.ToString(oLogin["tabela"]);
                string cOrigem = oLogin["origem"];
                string[] aOp = { "", "", "VISUALIZAR", "INCLUIR", "ALTERAR", "EXCLUIR", "", "" };
                string cOperacao = oLogin["operacao"];
                string cDescOperacao = aOp[Int32.Parse(cOperacao)];
                string cCodigo = "";
                string cTipoTabela = oLogin["tipotabela"];
                string cTrace1 = oLogin["trace1"] + oLogin["trace2"];
                string cTrace2 = cTipoTabela + " Cadastro > ";
                string cChave = "tabelas";
                string cCadastro = cTipoTabela.ToUpper() + " CADASTRO ";
                string cMenu = oLogin["menu"];
                string cPaginaAtu = Convert.ToString(oLogin["paginaatu"]);
                string cPaginaFim = Convert.ToString(oLogin["paginafim"]);
                string cFiltro = oLogin["filtro"];

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

                string cPastas = list["PASTAS"].First();
                string cTabPastas = "";
                string cNomeTabela = list["TABELA"].First();
                string cNomeTabOrigem = "";

                cPastas = (String.IsNullOrEmpty(cPastas) ? "Dados" : cPastas);

                var aPastas = cPastas.Split(';');

                //------------------ Dados da pasta da tabela

                string[] aTabPastas = { };

                if (String.IsNullOrEmpty(cTabPasta) || (cTabPasta == cTabela))
                {
                    aTabPastas = aPastas;
                    cTabPastas = cPastas;
                }
                else
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
                            //Acento no formato JSon: 0=Configuração =>> eval("'0=Configura!!xE7!!xE3o'".replace(/!!x/g,'\\x'))
                            cJSPastas += string.Format("self.items.push(new ItemViewModel(eval(\"'{0}'\".replace(/!!x/g,'\\\\x'))));", aTabPastas[i]);
                        }
                    }
                }

                //LogFile.Log(" --- cJSPastas:");
                //LogFile.Log(cJSPastas);

                cJSPastas = MeuLib.JSONAcento(cJSPastas);

                //LogFile.Log(cJSPastas);

                cHtml = cHtml.Replace("!XLOGIN!", oLogin["login"]);
                cHtml = cHtml.Replace("!XSESSAO!", oLogin["sessao"]);
                cHtml = cHtml.Replace("!XMENU!", cMenu);
                cHtml = cHtml.Replace("!XTRACE1!", cTrace1);
                cHtml = cHtml.Replace("!XTRACE2!", cTrace2);
                cHtml = cHtml.Replace("!XCADASTRO!", cCadastro);
                cHtml = cHtml.Replace("!XOPERACAO!", cOperacao);
                cHtml = cHtml.Replace("!XPAGINAATU!", cPaginaAtu);
                cHtml = cHtml.Replace("!XPAGINAFIM!", cPaginaFim);
                cHtml = cHtml.Replace("!XMANUTENCAO!", cDescOperacao);
                cHtml = cHtml.Replace("!XTABELA!", cTabela);
                cHtml = cHtml.Replace("!XNOMETABELA!", cNomeTabela);
                cHtml = cHtml.Replace("!XTABORIGEM!", cTabOrigem);
                cHtml = cHtml.Replace("!XNOMETABORIGEM!", cNomeTabOrigem);
                cHtml = cHtml.Replace("!XCODIGO!", cCodigo);
                cHtml = cHtml.Replace("!XORIGEM!", cOrigem);
                cHtml = cHtml.Replace("!XCHAVE!", cChave);
                cHtml = cHtml.Replace("!XITEM!", "000");
                cHtml = cHtml.Replace("!XFILTRO!", cFiltro);
                cHtml = cHtml.Replace("!XJSPASTAS!", cJSPastas);

            } while (false);

            return cHtml;

        } //Tabelas_Cadastro()


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
                    nPastaItem = -1;

                }
                else
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

                //--------------- Campos da tabela Cabeçalho
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

                //--------------- Dados da tabela Cabeçalho
                cQuery = "SELECT * FROM " + cNomeTabela + " WHERE idSequencial = " + cCodigo;
                campos = new List<string>(new string[] { });
                for (int i = 0; i < lsCampos["CAMPO"].Count; i++) { campos.Add(lsCampos["CAMPO"].ElementAt(i)); }  //Campos do Cabeçalho
                MultiValueDictionary<string, string> lsDados = MeuDB.Select(cQuery, campos);

                if ((lsDados.Count < campos.Count) || (lsDados[lsCampos["CAMPO"].First()].Count <= 0))
                {
                    LogFile.Log(string.Format("Tabelas_Obter: Problema para obter os dados da tabela! Tabela: {0}", cTabela));
                    break;
                }

                //--------------- Iniciando o JSon de Dados 

                cJSon = "{\"xconteudo\" : [";
                string cDisplay = "block";

                for (int i = 0; i < aPastas.Length; i++)
                {
                    if (i > 0) { cJSon += ","; }

                    cJSon += "{";
                    cJSon += string.Format("\"xpasta\" : \"tab{0}\",", (i + 1).ToString());
                    cJSon += string.Format("\"xdisplay\" : \"display:{0};\", ", cDisplay);

                    cDisplay = "none";

                    cJSon += "\"xcampos\" : [";

                    //--------------- CABEÇALHO

                    cJSon += Tabelas_Obter_Cabecalho(MeuDB, MeuLib, lsCampos, lsDados, cTabOrigem, cNomeTabela, cCodigo, cOperacao, i);

                    cJSon += "],";

                    //--------------- ITENS

                    if (nPastaItem != i)
                    {
                        //Item vazio
                        cJSon += "\"xtititem\" : [], \"xitens\" : []";

                    }
                    else
                    {
                        cJSon += Tabelas_Obter_Itens(MeuDB, MeuDBP, MeuLib, cTabOrigem, cTabItens, ("id0" + cNomeTabela), cCodigo, cOperacao);
                    }


                    cJSon += "}";
                }

                cJSon += "]}";

                //LogFile.Log("cJSon:");
                //LogFile.Log(cJSon);

                cHtml = cJSon;

            } while (false);

            return cHtml;

        } //Tabelas_Obter()


        private string Tabelas_Obter_Cabecalho(DBConnect MeuDB, ArtLib MeuLib, MultiValueDictionary<string, string> lsCampos, MultiValueDictionary<string, string> lsDados, string cTabOrigem, string cNomeTabela, string cCodigo, string cOperacao, int nPasta)
        {
            int nSeq = 0;
            string cSeq = "";
            int nLin = 0;      //Alternar a cor
            string cJSon = "";
            string cLinCor = "";
            MultiValueDictionary<int, string> aValores = new MultiValueDictionary<int, string>();  //aValores.Add(0, "VALOR1"); aValores.Add(0, "VALOR2");

            for (int i = 0; i < lsCampos["CAMPO"].Count; i++)
            {
                //Se não for da pasta ignora
                if ((Int32.Parse(lsCampos["PASTA"].ElementAt(i)) != nPasta)) { continue; }

                string cCampo = lsCampos["CAMPO"].ElementAt(i);
                string cCmpValPadrao = lsCampos["PADRAO"].ElementAt(i).Trim();
                string cCmpOpcoes = lsCampos["OPCOES"].ElementAt(i);
                string cCmpIDRevelar = lsCampos["REVELARCAMPO"].ElementAt(i);
                string cCmpVlrRevelar = lsCampos["REVELARVALOR"].ElementAt(i);

                string cCmpRevelador = (lsCampos["REVELADOR"].ElementAt(i) == "S" ? "1" : "0");
                string cCmpAltura = (String.IsNullOrEmpty(lsCampos["ALTURA"].ElementAt(i)) || (lsCampos["ALTURA"].ElementAt(i) == "0") ? "400" : lsCampos["ALTURA"].ElementAt(i)) + "px";
                string cCmpObriga = (lsCampos["OBRIGATORIO"].ElementAt(i) == "S" ? "1" : "0");
                Boolean lDireita = (lsCampos["POSICAO"].ElementAt(i) == "2");

                string cCmpGatilho = "ok";
                string cCmpCheckBox = "";
                string cCmpSequencia = "";
                string cCmpBloco = "";
                string cTabCheckBox = "";
                string cCondCheckBox = "";
                string cDestCheckBox = "";
                Boolean lCheckBox = false;
                Boolean lCheckNum = false;
                string cType = "6";
                string cValor = "";
                string cGatilhoClasse = "x";
                MultiValueDictionary<int, string> aOpcoes = new MultiValueDictionary<int, string>();


                if (lsCampos["TIPOPADRAO"].ElementAt(i) == "3")
                {
                    cCmpValPadrao = MeuDB.FMacro(MeuDB, cCmpValPadrao, cTabOrigem);
                }

                cCmpValPadrao = cCmpValPadrao.Replace("|||XTABORIGEM|||", cTabOrigem);

                if (cCmpValPadrao.Contains("|X") && cCmpValPadrao.Contains("X|"))
                {
                    for (int j = 0; j < aValores.Count; j++)
                    {
                        cCmpValPadrao = cCmpValPadrao.Replace(aValores[j].ElementAt(0), aValores[j].ElementAt(1));
                    }
                }

                Boolean lComboBox = !(String.IsNullOrEmpty(lsCampos["OPCOES"].ElementAt(i)));

                if (lComboBox)
                {
                    lComboBox = !(lsCampos["OPCOES"].ElementAt(i).Contains("!X")); //!XPASTAS!
                }

                string cSoLeitura = "0";

                if ("25".Contains(cOperacao) || "36".Contains(lsCampos["EDICAO"].ElementAt(i)) ||
                     ((cOperacao == "4") && (lsCampos["ALTERACAO"].ElementAt(i) == "2")) ||
                     ((cOperacao == "3") && (lsCampos["ALTERACAO"].ElementAt(i) == "3")))
                {
                    cSoLeitura = "1";
                }

                /* CONSULTA COMBO */
                if (lsCampos["CONSULTATIPO"].ElementAt(i) == "1")
                {
                    int nCodConsulta = Int32.Parse(lsCampos["CONSULTACODIGO"].ElementAt(i));
                    int nCmpConsulta = Int32.Parse(lsCampos["CONSULTACAMPO"].ElementAt(i));
                    string cCondConsulta = lsCampos["CONSULTACONDICAO"].ElementAt(i);

                    cCmpOpcoes = MeuDB.FConsultaCombo(MeuDB, cTabOrigem, nCodConsulta, nCmpConsulta, cCondConsulta);

                    lComboBox = true;

                } /* CONSULTA COMBO */
                else

/* CHECKBOX */ if (lsCampos["CONSULTATIPO"].ElementAt(i) == "3")
                {
                    cTabCheckBox = lsCampos["CONSULTACODIGO"].ElementAt(i);
                    cCmpCheckBox = lsCampos["CONSULTACAMPO"].ElementAt(i);
                    cCondCheckBox = lsCampos["CONSULTACONDICAO"].ElementAt(i);
                    cDestCheckBox = lsCampos["DESTINO"].ElementAt(i);
                    lCheckBox = true;
                    cType = "6";

                    cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : "0"));

                } /* CHECKBOX */
                else

/* PERIODO */ if (lsCampos["CONSULTATIPO"].ElementAt(i) == "4")
                {
                    cType = "7";

                    cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : "0000000000000000000000"));

                } /* PERIODO */
                else

/* CHECKNUM */
if (lsCampos["CONSULTATIPO"].ElementAt(i) == "4")
                {
                    cTabCheckBox = lsCampos["CONSULTACODIGO"].ElementAt(i);
                    cCmpCheckBox = lsCampos["CONSULTACAMPO"].ElementAt(i);
                    cCondCheckBox = lsCampos["CONSULTACONDICAO"].ElementAt(i);
                    cCmpSequencia = lsCampos["CONSULTASEQ"].ElementAt(i);
                    cCmpBloco = lsCampos["CONSULTABLQ"].ElementAt(i);
                    cDestCheckBox = lsCampos["DESTINO"].ElementAt(i);
                    lCheckNum = true;
                    cType = "8";

                    cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : "0"));

                } /* CHECKNUM */
                else

if ((lsCampos["TIPO"].ElementAt(i) == "C") || lComboBox)
                {
                    /* SELECT */
                    if (lComboBox)
                    {
                        aOpcoes = MeuLib.FMatriz(cCmpOpcoes);
                        cType = "2";

                        cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : "0"));

                    } /* SELECT */
                    else

       /* TEXT/PASSWORD/HIDDEN */ if ("12345679".Contains(lsCampos["EDICAO"].ElementAt(i)))
                    {
                        int nPos = "12345679".IndexOf(lsCampos["EDICAO"].ElementAt(i));
                        cType = "11111149".Substring(nPos, 1);

                        cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : cCmpValPadrao);

                    } /* TEXT/PASSWORD/HIDDEN */

                }
                else

/* MEMO */ if (lsCampos["TIPO"].ElementAt(i) == "M")
                {
                    cType = "3";

                    cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : cCmpValPadrao);

                } /* MEMO */
                else

/* DATA */ if (lsCampos["TIPO"].ElementAt(i) == "D")
                {
                    cType = "5";

                    cCmpValPadrao = (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : "  /  /    ");

                    cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : cCmpValPadrao);

                } /* DATA */
                else

/* HORA */ if (lsCampos["TIPO"].ElementAt(i) == "H")
                {
                    cType = "1";

                    cCmpValPadrao = (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : "  :  ");

                    cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : cCmpValPadrao);

                } /* HORA */
                else

/* NUMERICO */ if (lsCampos["TIPO"].ElementAt(i) == "N")
                {
                    cType = "1";

                    cValor = (!String.IsNullOrEmpty(lsDados[cCampo].First()) ? lsDados[cCampo].First() : (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao.Trim() : "0"));

                } /* NUMERICO */

                cValor.Replace("\r", "\n");
                cValor.Replace("\"", "'");

                if ((lsCampos["TELA"].ElementAt(i) == "2") || (lsCampos["EDICAO"].ElementAt(i) == "9")) { cType = "9"; }

                //--------------- JSON

                if (!lDireita) { ++nSeq; }
                if (!lDireita && (cType != "9")) { ++nLin; }

                if (cType == "9")
                {
                    cCmpIDRevelar = "esconder";
                    cCmpVlrRevelar = "esconder";
                }
                else
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

                /* GATILHO CONSULTA */
                if (lsCampos["GATILHOTIPO"].ElementAt(i) == "1")
                {
                    cGatilhoClasse = "xgatilho";

                    string cQuery = string.Format("SELECT CAMPO FROM aa41campos WHERE idSequencial = {0} ", lsCampos["GATILHOCAMPO"].ElementAt(i));

                    List<string> campos = new List<string>(new string[] { "CAMPO" });

                    MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                    if (!((list.Count < campos.Count) || (list["CAMPO"].Count <= 0)))
                    {
                        cCmpGatilho = list["CAMPO"].First();
                    }
                } /* GATILHO CONSULTA */


                /* GATILHO REVELADOR DE LINHA */
                if (cCmpRevelador == "1")
                {
                    cGatilhoClasse = "xgatilho";

                } /* GATILHO REVELADOR DE LINHA */


                /* INCLUSAO - ULTIMO VALOR */
                if ((cOperacao == "3") && (lsCampos["ULTIMOVALOR"].ElementAt(i) == "S"))
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
                cJSon += String.Format(", \"config\": \"{0}{1}{2}{3}{4}\"", cType, cSoLeitura, cCmpRevelador, cCmpObriga, lsCampos["CHARCASE"].ElementAt(i).Trim());
                cJSon += String.Format(", \"editcond\": \"{0}\"", lsCampos["EDITCOND"].ElementAt(i));
                cJSon += String.Format(", \"gatilhoclasse\": \"{0}\"", cGatilhoClasse);
                cJSon += String.Format(", \"gatilhoid\": \"{0}\"", lsCampos["GATILHOCAMPO"].ElementAt(i));
                cJSon += String.Format(", \"gatilhocampo\": \"{0}\"", cCmpGatilho);

                if (lComboBox)
                {
                    cJSon += ", \"option\"   : [";

                    for (int j = 0; j < aOpcoes.Count; j++)
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

                aValores.Add(i, "|X" + cCampo + "X|");  //Adiciona para Macro substituição de campos
                aValores.Add(i, cValor);

            } //for(i)(lsCampos)

            cJSon = (String.IsNullOrEmpty(cJSon) ? "" : cJSon + "}] }");

            cJSon = MeuLib.JSONAcento(cJSon);

            cJSon = cJSon.Replace("\\x", "!!x");
            cJSon = cJSon.Replace("\\u", "!!u");

            return cJSon;

        } //Tabelas_Obter_Cabecalho()


        private string Tabelas_Obter_Itens(DBConnect MeuDB, DBConnect MeuDBP, ArtLib MeuLib, string cTabOrigem, string cTabItens, string cCmpIdPai, string cCodPai, string cOperacao)
        {

            string cJSon = "";
            string cJTitulo1 = "\"xtititem\" : [{0}]";
            string cJTitulo2 = "";
            string cJItem1 = ",\"xitens\"  : [{0}]";
            string cJItem2 = "";
            string cJItemV1 = ",\"xitemvazio\" : {0}";
            string cJItemV2 = "";
            string cJItemV3 = "";
            int nItem = 0;
            bool lItemVazio = true; //Iniciar a montagem do item vazio

            LogFile.Log(" --- Obter itens --- ");
            LogFile.Log(" --- cTabOrigem: " + cTabOrigem);
            LogFile.Log(" --- cCodPai...: " + cCodPai);

            do
            {

                //------------------ Dados da configuracao da tabela de itens

                string cQuery = string.Format("SELECT * FROM aa40tabelas WHERE idSequencial = {0} ", cTabItens);
                List<string> campos = new List<string>(new string[] { "TABELA" });
                MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["TABELA"].Count <= 0))
                {
                    LogFile.Log(string.Format("Tabelas_Obter_Itens: Problema para obter os dados de configuracao da tabela de itens! Tabela: {0}", cTabItens));
                    break;
                }

                string cNomeTabela = list["TABELA"].First();

                //------------------ Status da tabela

                cQuery = string.Format(" SELECT CODIGO, DESCRICAO, " +
                                       " (SELECT DESCRICAO FROM aa46status b46 WHERE b46.id0aa40tabelas = 13 AND b46.CODIGO = a46.STATUS LIMIT 0,1) AS COR " +
                                       " FROM aa46status a46 " +
                                       " WHERE id0aa40tabelas = {0} ORDER BY ORDEM ", cTabItens);
                campos = new List<string>(new string[] { "CODIGO", "DESCRICAO", "COR" });
                MultiValueDictionary<string, string> lsStatus = MeuDB.Select(cQuery, campos);

                if ((lsStatus.Count < campos.Count) || (lsStatus["CODIGO"].Count <= 0))
                {
                    LogFile.Log(string.Format("Tabelas_Obter_Itens: Problema para obter os status da tabela de itens! Tabela: {0}", cTabItens));
                    break;
                }

                //--------------- Campos da tabela de itens

                cQuery = "SELECT * FROM aa41campos WHERE id0aa40tabelas = " + cTabItens + " ORDER BY ORDEM ";
                campos = new List<string>(new string[] { "CAMPO", "TITULO", "TIPO", "TAMANHO", "CASADECIMAL", "OPCOES", "OBRIGATORIO", "PASTA", "EDICAO", "ALTERACAO", "CONSULTATIPO",
                                                         "CONSULTACODIGO", "CONSULTACAMPO", "CONSULTACONDICAO", "CONSULTASEQ", "CONSULTABLK", "DESTINO", "POSICAO", "PADRAO",
                                                         "REVELADOR", "REVELARCAMPO", "REVELARVALOR", "GATILHOTIPO", "GATILHOCAMPO", "ULTIMOVALOR", "ALTURA", "CHARCASE", "TELA",
                                                         "TIPOPADRAO", "COMENTARIO", "EDITCOND" });
                MultiValueDictionary<string, string> lsCampos = MeuDBP.Select(cQuery, campos);

                if ((lsCampos.Count < campos.Count) || (lsCampos["CAMPO"].Count <= 0))
                {
                    LogFile.Log(string.Format("Tabelas_Obter: Problema para obter os campos da tabela! Tabela: {0}", cTabItens));
                    break;
                }

                //------------------ Dados da tabela de itens

                cQuery = string.Format("SELECT * FROM {0} WHERE {1} = {2} ", cNomeTabela, cCmpIdPai, cTabOrigem);
                for (int i = 0; i < lsCampos["CAMPO"].Count; i++) { campos.Add(lsCampos["CAMPO"].ElementAt(i)); } //Campos do item
                MultiValueDictionary<string, string> lsDados = MeuDB.Select(cQuery, campos);

                if ((lsDados.Count < campos.Count) || (lsDados["idSequencial"].Count <= 0))
                {
                    LogFile.Log(string.Format("Tabelas_Obter_Itens: Problema para obter os dados da tabela de itens! Tabela: {0}", cTabItens));
                    break;
                }

                int nLin = 0;

                for (int i = 0; i < lsDados["idSequencial"].Count; i++)
                {
                    ++nLin;

                    int nSeq = 0;
                    string cJItem3 = "";

                    string cReg = lsDados["idSequencial"].ElementAt(i);

                    for (int j = 0; j < lsCampos["CAMPO"].Count; j++)
                    {
                        if (lsCampos["CAMPO"].ElementAt(j) == cCmpIdPai) { continue; }

                        bool lComboBox = !(String.IsNullOrEmpty(lsCampos["OPCOES"].ElementAt(j)));

                        if (lComboBox) { lComboBox = !(lsCampos["OPCOES"].ElementAt(j).Contains("!X")); }

                        string cCampo = lsCampos["CAMPO"].ElementAt(j);
                        string cCmpObriga = (lsCampos["OBRIGATORIO"].ElementAt(j) == "S" ? "1" : "0");
                        string cCmpValPadrao = lsCampos["PADRAO"].ElementAt(j).Trim();
                        string cCmpOpcoes = lsCampos["OPCOES"].ElementAt(j).Trim();

                        string cType = "";
                        string cSoLeitura = "0";
                        string cClasse = "";
                        string cValor = "";
                        string cValor2 = "";
                        string cVlrVazio1 = "";
                        string cVlrVazio2 = "";

                        MultiValueDictionary<int, string> aOpcoes = new MultiValueDictionary<int, string>();

                        if (("25".Contains(cOperacao)) || ("36".Contains(lsCampos["EDICAO"].ElementAt(j))) ||
                            ((cOperacao == "4") && (lsCampos["ALTERACAO"].ElementAt(j) == "2")) ||
                            ((cOperacao == "3") && (lsCampos["ALTERACAO"].ElementAt(j) == "3")))
                        {
                            cSoLeitura = "1";
                        }

                        /*CONSULTA COMBO*/
                        if (lsCampos["CONSULTATIPO"].ElementAt(j) == "1")
                        {
                            int nCodConsulta = Int32.Parse(lsCampos["CONSULTACODIGO"].ElementAt(j));
                            int nCmpConsulta = Int32.Parse(lsCampos["CONSULTACAMPO"].ElementAt(j));
                            string cCondConsulta = lsCampos["CONSULTACONDICAO"].ElementAt(j);

                            cCmpOpcoes = MeuDB.FConsultaCombo(MeuDB, cTabOrigem, nCodConsulta, nCmpConsulta, cCondConsulta);

                            lComboBox = true;

                        } /*CONSULTA COMBO*/

                        if ((lsCampos["TIPO"].ElementAt(j) == "C") || lComboBox)
                        {
                            /*STATUS*/
                            if (cCampo == "STATUS")
                            {
                                cType = "6";

                                cCmpValPadrao = (String.IsNullOrEmpty(cCmpValPadrao) ? lsStatus["DESCRICAO"].ElementAt(0) : cCmpValPadrao);

                                cValor = lsDados["STATUS"].ElementAt(i);

                                cValor = ((cOperacao == "3") || String.IsNullOrEmpty(cValor) ? cCmpValPadrao : cValor);

                                for (int k = 0; k < lsStatus["CODIGO"].Count; k++)
                                {
                                    if (lsStatus["CODIGO"].ElementAt(k) == cValor)
                                    {
                                        cValor = lsStatus["DESCRICAO"].ElementAt(k);
                                        cValor2 = String.Format("portal_led_{0}.png", lsStatus["COR"].ElementAt(k));
                                    }

                                    if (lsStatus["CODIGO"].ElementAt(k) == cCmpValPadrao)
                                    {
                                        cVlrVazio1 = lsStatus["DESCRICAO"].ElementAt(k);
                                        cVlrVazio2 = String.Format("portal_led_{0}.png", lsStatus["COR"].ElementAt(k));
                                    }
                                }/*for k*/

                            }/*STATUS*/

                            else

                 /*SELECT*/ if (lComboBox)
                            {
                                aOpcoes = MeuLib.FMatriz(cCmpOpcoes);
                                cType = "2";

                                cValor = (!String.IsNullOrEmpty(lsDados[cCampo].ElementAt(i)) ? lsDados[cCampo].ElementAt(i) : (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : "0"));

                            } /* SELECT */
                            else

                 /* TEXT/PASSWORD/HIDDEN */ if ("12345679".Contains(lsCampos["EDICAO"].ElementAt(j)))
                            {
                                int nPos = "12345679".IndexOf(lsCampos["EDICAO"].ElementAt(j));
                                cType = "11111149".Substring(nPos, 1);

                                cValor = (!String.IsNullOrEmpty(lsDados[cCampo].ElementAt(i)) ? lsDados[cCampo].ElementAt(i) : cCmpValPadrao);

                            } /* TEXT/PASSWORD/HIDDEN */

                        }
                        else

                        /* MEMO */ if (lsCampos["TIPO"].ElementAt(j) == "M")
                        {
                            cType = "3";

                            cValor = (!String.IsNullOrEmpty(lsDados[cCampo].ElementAt(i)) ? lsDados[cCampo].ElementAt(i) : cCmpValPadrao);

                        } /* MEMO */
                        else

                        /* DATA */ if (lsCampos["TIPO"].ElementAt(j) == "D")
                        {
                            cType = "5";

                            cCmpValPadrao = (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : "  /  /    ");

                            cValor = (!String.IsNullOrEmpty(lsDados[cCampo].ElementAt(i)) ? lsDados[cCampo].ElementAt(i) : cCmpValPadrao);

                        } /* DATA */
                        else

                        /* HORA */ if (lsCampos["TIPO"].ElementAt(j) == "H")
                        {
                            cType = "1";

                            cCmpValPadrao = (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao : "  :  ");

                            cValor = (!String.IsNullOrEmpty(lsDados[cCampo].ElementAt(i)) ? lsDados[cCampo].ElementAt(i) : cCmpValPadrao);

                        } /* HORA */
                        else

                        /* NUMERICO */ if (lsCampos["TIPO"].ElementAt(j) == "N")
                        {
                            cType = "1";

                            cValor = (!String.IsNullOrEmpty(lsDados[cCampo].ElementAt(i)) ? lsDados[cCampo].ElementAt(i) : (!String.IsNullOrEmpty(cCmpValPadrao) ? cCmpValPadrao.Trim() : "0"));

                        } /* NUMERICO */

                        cValor.Replace("\r", "\n");
                        cValor.Replace("\"", "'");

                        if ((lsCampos["TELA"].ElementAt(j) == "2") || (lsCampos["EDICAO"].ElementAt(j) == "9")) { cType = "9"; }

                        //------------------ JSON

                        ++nSeq;

                        string cJItem4 = "";

                        cJItem4 += String.Format("  \"sequencia\": {0}", nSeq.ToString());
                        cJItem4 += String.Format(", \"campo\"    : \"{0}\"", cCampo);
                        cJItem4 += String.Format(", \"tamanho\"  : \"{0}\"", lsCampos["TAMANHO"].ElementAt(j).Trim());
                        cJItem4 += String.Format(", \"valor\"    : \"{0}\"", cValor);
                        cJItem4 += String.Format(", \"valor2\"   : \"{0}\"", cValor2);
                        cJItem4 += String.Format(", \"config\"   : \"{0}{1}{2}{3}\"", cType, cSoLeitura, cCmpObriga, lsCampos["CHARCASE"].ElementAt(j));
                        cJItem4 += String.Format(", \"editcond\" : \"{0}\"", lsCampos["EDITCOND"].ElementAt(j));
                        cJItem4 += String.Format(", \"classe\"   : \"{0}\"", cClasse);

                        if (lComboBox)
                        {
                            cJItem4 += ", \"option\"   : [";

                            for (int k = 0; k < aOpcoes.Count; k++)
                            {
                                if (k > 0) { cJItem4 += ","; }

                                cJItem4 += " {";
                                cJItem4 += String.Format("\"id\": \"{0}\", \"name\": \"{1}\"", aOpcoes[k].ElementAt(0), aOpcoes[k].ElementAt(1));
                                cJItem4 += "}";
                            }

                            cJItem4 += " ]";
                        }

                        if (nLin == 1)
                        {
                            if (cCampo == "STATUS")
                            {
                                cJTitulo2 = "{" + String.Format("\"titulo\": \"{0}\"}", lsCampos["TITULO"].ElementAt(j)) +
                                            (String.IsNullOrEmpty(cJTitulo2) ? "" : ",") + cJTitulo2;
                            }
                            else
                            {
                                cJTitulo2 += (String.IsNullOrEmpty(cJTitulo2) ? "" : ",") + "{" + String.Format("\"titulo\": \"{0}\"}", lsCampos["TITULO"].ElementAt(j));
                            }
                        }

                        if (cCampo == "STATUS")
                        {
                            cJItem3 = "{" + String.Format("{0}}", cJItem4) +
                                      (String.IsNullOrEmpty(cJItem3) ? "" : ",") + cJItem3;
                        }
                        else
                        {
                            cJItem3 += (String.IsNullOrEmpty(cJItem3) ? "" : ",") + String.Format("{{0}}", cJItem4);
                        }

                        //------------------ JSON - Item Vazio
                        if (lItemVazio)
                        {
                            string cJItemV4 = "";

                            cJItemV4 += String.Format("  \"sequencia\": {0}", nSeq.ToString());
                            cJItemV4 += String.Format(", \"campo\"    : \"{0}\"", cCampo);
                            cJItemV4 += String.Format(", \"tamanho\"  : \"{0}\"", lsCampos["TAMANHO"].ElementAt(j).Trim());
                            cJItemV4 += String.Format(", \"valor\"    : \"{0}\"", cValor);
                            cJItemV4 += String.Format(", \"valor2\"   : \"{0}\"", cValor2);
                            cJItemV4 += String.Format(", \"config\"   : \"{0}{1}{2}{3}\"", cType, cSoLeitura, cCmpObriga, lsCampos["CHARCASE"].ElementAt(j));
                            cJItemV4 += String.Format(", \"editcond\" : \"{0}\"", lsCampos["EDITCOND"].ElementAt(j));
                            cJItemV4 += String.Format(", \"classe\"   : \"{0}\"", cClasse);

                            if (lComboBox)
                            {
                                cJItemV4 += ", \"option\"   : [";

                                for (int k = 0; k < aOpcoes.Count; k++)
                                {
                                    if (k > 0) { cJItemV4 += ","; }

                                    cJItemV4 += " {";
                                    cJItemV4 += String.Format("\"id\": \"{0}\", \"name\": \"{1}\"", aOpcoes[k].ElementAt(0), aOpcoes[k].ElementAt(1));
                                    cJItemV4 += "}";
                                }

                                cJItemV4 += " ]";
                            }

                            if (cCampo == "STATUS")
                            {
                                cJItemV3 = "{" + String.Format("{0}}", cJItemV4) +
                                          (String.IsNullOrEmpty(cJItemV3) ? "" : ",") + cJItemV3;
                            }
                            else
                            {
                                cJItemV3 += (String.IsNullOrEmpty(cJItemV3) ? "" : ",") + String.Format("{{0}}", cJItemV4);
                            }

                        }

                    }/*for j - campos*/

                    ++nItem;

                    cJItem2 += (String.IsNullOrEmpty(cJItem2) ? "" : ",") + "{" + String.Format("\"xitemid\"  : {0}, \"xitemsts\" : 8, \"xitemreg\" : {1}, \"xitem\" : [{2}]}", nItem.ToString(), lsDados["idSequencial"].ElementAt(i), cJItem3);

                    if (lItemVazio)
                    {
                        cJItemV2 += (String.IsNullOrEmpty(cJItemV2) ? "" : ",") + "{" + String.Format("\"xitemid\"  : {0}, \"xitemsts\" : 1, \"xitemtab\" : {1}, \"xitem\" : [{2}]}", nItem.ToString(), cTabItens, cJItem3);
                    }

                    lItemVazio = false;

                }/*for i - itens*/

            } while (false);

            if (String.IsNullOrEmpty(cJTitulo2))
            {
                cJTitulo2 = "{\"titulo\": \"Manuten!!xE7!!xE3o\"}," + cJTitulo2;
            }

            cJTitulo1 = String.Format(cJTitulo1, cJTitulo2);
            cJItemV1 = String.Format(cJItemV1, cJItemV2);
            cJItem1 = String.Format(cJItem1, cJItem2);

            cJSon = cJTitulo1 + cJItemV1 + cJItem1;

            cJSon = MeuLib.JSONAcento(cJSon);

            cJSon = cJSon.Replace("\\x", "!!x");
            cJSon = cJSon.Replace("\\u", "!!u");

            return cJSon;

        } //Tabelas_Obter_Itens()


        public string Tabelas_Valida(HttpListenerRequest request, DBConnect MeuDBP, DBConnect MeuDB, ArtLib MeuLib, string cMeuPath, string cDados)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string cQueryString = HttpUtility.UrlDecode(request.Url.Query);
            string cJSon = cDados.Replace("dados=", "");
            dynamic oLogin = serializer.Deserialize<dynamic>(cJSon);
            string cHtml = "ERRO: Html nao atribuido";

            LogFile.Log(" --- Tabelas_Valida:");

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
                string cCodigo = oLogin["codigo"];
                string cOperacao = oLogin["operacao"];

                //--------------- Dados da configuracao da tabela de itens
                string cQuery = string.Format("SELECT * FROM aa40tabelas WHERE idSequencial = {0} ", cTabela);
                List<string> campos = new List<string>(new string[] { "TABELA" });
                MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["TABELA"].Count <= 0))
                {
                    LogFile.Log(string.Format("Tabelas_Valida: tabela nao encontrada! Tabela: {0}", cTabela));
                    cHtml = "ERRO: Tabela invalida!";
                    break;
                }

                string cNomeTabela = list["TABELA"].First();

                LogFile.Log("cTabela........: " + cTabela);
                LogFile.Log("cCodigo........: " + cCodigo);
                LogFile.Log("cNomeTabela....: " + cNomeTabela);

                if (!(String.IsNullOrEmpty(cCodigo)))
                {
                    cQuery = "SELECT * FROM " + cNomeTabela + " WHERE idSequencial = " + cCodigo;
                    campos = new List<string>(new string[] { "idSequencial" });
                    list = MeuDB.Select(cQuery, campos);

                    if ((list.Count < campos.Count) || (list["idSequencial"].Count <= 0))
                    {
                        LogFile.Log(string.Format("Tabelas_Valida: Codigo invalido! Codigo: {0}", cCodigo));
                        cHtml = "ERRO: Codigo invalido!";
                        break;
                    }
                }
                else if ("45".Contains(cOperacao))
                {
                    LogFile.Log(string.Format("Tabelas_Valida: Codigo em branco! Codigo: {0}", cCodigo));
                    cHtml = "ERRO: Codigo em branco!";
                    break;
                }

                dynamic aConteudo = oLogin["dados"]["xconteudo"];
                string cCmpObr = "";
                string cCmpRetirar = "N";

                for (int h = 0; h < aConteudo.Length; h++)
                {
                    dynamic aCampos = aConteudo[h]["xcampos"];

                    for (int i = 0; i < aCampos.Length; i++)
                    {
                        dynamic aLinhas = aCampos[i]["xlinha"];

                        for (int j = 0; j < aLinhas.Length; j++)
                        {
                            string cObriga = (aLinhas[j]["config"].Substring(3, 1) == "1" ? " (OBRIGATORIO)" : "");

                            if (!(String.IsNullOrEmpty(cObriga)) && String.IsNullOrEmpty(aLinhas[j]["valor"]))
                            {
                                cCmpObr += (String.IsNullOrEmpty(cCmpObr) ? "" : ",") + aLinhas[j]["titulo"];
                            }

                            if (aLinhas[j]["campo"] == "RETIRAR")
                            {
                                cCmpRetirar = aLinhas[j]["valor"];
                            }
                        } //for(i)
                    }//for(j)
                }//for(h)

                cCmpObr = cCmpObr.Replace(":", "");

                if ((cOperacao == "5") && (cCmpRetirar == "S"))
                {
                    cHtml = "ERRO: Nao pode excluir este registro! Use o campo RETIRAR!";
                }
                else if ("34".Contains(cOperacao) && !(String.IsNullOrEmpty(cCmpObr)))
                {
                    cHtml = "ERRO : Estes campos sao obrigatorios: " + cCmpObr;
                }
                else
                {
                    cHtml = "Validado!";
                }

            } while (false);

            return cHtml;

        } //Tabelas_Valida()


        public string Tabelas_Gravar(HttpListenerRequest request, DBConnect MeuDBP, DBConnect MeuDB, ArtLib MeuLib, string cMeuPath, string cDados)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string cQueryString = HttpUtility.UrlDecode(request.Url.Query);
            string cJSon = cDados.Replace("dados=", "");
            dynamic oLogin = serializer.Deserialize<dynamic>(cJSon);
            string cHtml = "ERRO: Html nao atribuido";

            LogFile.Log(" --- Tabelas_Gravar:");

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
                string cCodigo = oLogin["codigo"];
                string cOperacao = oLogin["operacao"];

                //--------------- Dados da configuracao da tabela de itens
                string cQuery = string.Format("SELECT * FROM aa40tabelas WHERE idSequencial = {0} ", cTabela);
                List<string> campos = new List<string>(new string[] { "TABELA" });
                MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["TABELA"].Count <= 0))
                {
                    LogFile.Log(string.Format("Tabelas_Gravar: tabela nao encontrada! Tabela: {0}", cTabela));
                    cHtml = "ERRO: Tabela invalida!";
                    break;
                }

                string cNomeTabela = list["TABELA"].First();

                LogFile.Log("cTabela........: " + cTabela);
                LogFile.Log("cCodigo........: " + cCodigo);
                LogFile.Log("cNomeTabela....: " + cNomeTabela);

                if (!(String.IsNullOrEmpty(cCodigo)))
                {
                    cQuery = "SELECT * FROM " + cNomeTabela + " WHERE idSequencial = " + cCodigo;
                    campos = new List<string>(new string[] { "idSequencial" });
                    list = MeuDB.Select(cQuery, campos);

                    if ((list.Count < campos.Count) || (list["idSequencial"].Count <= 0))
                    {
                        LogFile.Log(string.Format("Tabelas_Gravar: Codigo invalido! Codigo: {0}", cCodigo));
                        cHtml = "ERRO: Codigo invalido!";
                        break;
                    }
                }

                //--------------- Campos da tabela Cabeçalho
                cQuery = "SELECT * FROM aa41campos WHERE id0aa40tabelas = " + cTabela + " ORDER BY ORDEM ";
                campos = new List<string>(new string[] { "CAMPO", "TIPO", "TIPOPADRAO", "EDICAO", "CONSULTACODIGO", "DESTINO" });
                MultiValueDictionary<string, string> lsCampos = MeuDBP.Select(cQuery, campos);

                if ((lsCampos.Count < campos.Count) || (lsCampos["CAMPO"].Count <= 0))
                {
                    LogFile.Log(string.Format("Tabelas_Gravar: Problema para obter os campos da tabela! Tabela: {0}", cTabela));
                    break;
                }

            //--------------- Obtendo os dados da pagina para gravacao
                dynamic aConteudo = oLogin["dados"]["xconteudo"];
                string cCmpObr = "";
                string cCmpRetirar = "N";
                string cCmpOperacao = "";
                int nP = -1;
                int nE = -1;
                string cValor = "";

                dynamic aJChecks = null;

                MultiValueDictionary<string, string> aDados = new MultiValueDictionary<string, string>();
                MultiValueDictionary<int, string> aChecks = new MultiValueDictionary<int, string>();
                MultiValueDictionary<int, string> aDChecks = new MultiValueDictionary<int, string>();

                for (int h = 0; h < aConteudo.Length; h++)
                {
                    dynamic aCampos = aConteudo[h]["xcampos"];

                    for (int i = 0; i < aCampos.Length; i++)
                    {
                        dynamic aLinhas = aCampos[i]["xlinha"];

                        for (int j = 0; j < aLinhas.Length; j++)
                        {
                            string cObriga = (aLinhas[j]["config"].Substring(3, 1) == "1" ? " (OBRIGATORIO)" : "");

                            cValor = (String.IsNullOrEmpty(aLinhas[j]["valor"]) ? "" : aLinhas[j]["valor"]);

                            if (aLinhas[j]["campo"] == "RETIRAR")
                            {
                                cCmpRetirar = cValor;

                                if  (cOperacao == "5") {   goto Pular_01;   }
                            }
                            else if (aLinhas[j]["campo"] == "OPERACAO")
                            {
                                cCmpOperacao = cValor;
                            }

                            if ( !(String.IsNullOrEmpty(cObriga)) && String.IsNullOrEmpty(cValor) )
                            {
                                cCmpObr += (String.IsNullOrEmpty(cCmpObr) ? "" : ",") + aLinhas[j]["titulo"];
                                continue;
                            }

                            if ("34".Contains(cOperacao))
                            {
                                //Posicao do campos em lsCampos[]
                                nP = -1;

                                for (int x = 0; x < lsCampos.Count; x++)
                                {
                                    if (lsCampos["CAMPO"].ElementAt(x) == aLinhas[j]["campo"])
                                    {
                                        nP = x;
                                        break;
                                    }
                                }

                                if (nP == -1)
                                {
                                    LogFile.Log(string.Format(" --- Tabelas_Gravar: Campo nao encontrado! Campo: {0}", aLinhas[j]["campo"]));
                                    continue;
                                }

                                if  (lsCampos["TIPOPADRAO"].ElementAt(nP) == "4"/*Trigger*/)
                                {
                                    LogFile.Log(string.Format(" --- Tabelas_Gravar: Campo trigger! Campo: {0}", aLinhas[j]["campo"]));
                                    continue;
                                }

                                aDados.Add("CAMPO" , aLinhas[j]["campo"]);
                                aDados.Add("TIPO"  , lsCampos["TIPO"].ElementAt(nP));
                                aDados.Add("VALOR" , cValor);
                                aDados.Add("EDICAO", lsCampos["EDICAO"].ElementAt(nP));

                                if (aLinhas[j].ContainsKey("checks") && (aLinhas[j]["checks"].Count > 0))
                                {
                                    aJChecks = aLinhas[j]["checks"];

                                    //Exclusao dos checks anteriores
                                    if ( !String.IsNullOrEmpty(lsCampos["DESTINO"].ElementAt(nP)) )
                                    {
                                        ++nE;
                                        aDChecks.Add(nE, lsCampos["DESTINO"].ElementAt(nP));
                                    }

                                    for (int c = 0; c < aJChecks; c++)
                                    {
                                        aChecks.Add(c, lsCampos["DESTINO"].ElementAt(nP));
                                        aChecks.Add(c, lsCampos["CONSULTACODIGO"].ElementAt(nP));
                                        aChecks.Add(c, aJChecks[c]["idSequencial"]);
                                    }

                                }

                            }//if

                        } //for(i)
                    }//for(j)
                }//for(h)

            Pular_01:

                cCmpObr = cCmpObr.Replace(":", "");

                if ((cOperacao == "5") && (cCmpRetirar == "S"))
                {
                    cHtml = "ERRO: Nao pode excluir este registro! Use o campo RETIRAR!";
                    break;
                }
                else if ("34".Contains(cOperacao) && !(String.IsNullOrEmpty(cCmpObr)))
                {
                    cHtml = "ERRO : Estes campos sao obrigatorios: " + cCmpObr;
                    break;
                }

                cQuery = "";

            //--------------- GRAVAR CABECALHO
                
                if (cOperacao == "4") //Alteracao
                {
                    LogFile.Log(" Alteracao: " + cNomeTabela);

                    cQuery = String.Format(" UPDATE {0} SET ", cNomeTabela);

                    for (int i=0; i < aDados.Count; i++)
                    {
                        if (cQuery.Substring(cQuery.Length-4,4) != "SET ") { cQuery += ","; }

                        cValor = aDados["VALOR"].ElementAt(i);

                        if (aDados["TIPO"].ElementAt(i) == "N")  //Numerico
                        {
                            cValor = (String.IsNullOrEmpty(cValor) ? "0" : cValor);

                        } else if (aDados["TIPO"].ElementAt(i) == "D") //Data
                        {
                            if ( (cValor.Trim() == "/ /") || String.IsNullOrEmpty(cValor) ) { cValor = "NULL"; }
                            else if (cValor.Contains("T00:00:00.000Z")) { cValor = String.Format("'{0}'", cValor.Substring(0, 10)); }
                            else { cValor = String.Format("'{0}'", DateTime.Parse(cValor).ToString("yyyy-MM-dd")); }

                        } else //Caractere
                        {
                            cValor = cValor.Replace("'", "\"");

                            //Cliptografado
                            if (aDados["EDICAO"].ElementAt(i) == "7") { cValor = MeuLib.Cobrir(cValor); }

                            cValor = String.Format("'{0}'", cValor);

                        }

                        cQuery += String.Format(" {0} = {1} ", aDados["CAMPO"].ElementAt(i), cValor);

                    }//for(i)

                    cQuery += " WHERE idSequencial=" + cCodigo;

                    cHtml = "Registro alterado com sucesso!";

                    if (MeuDB.Update(cQuery) <= 0)
                    {
                        cHtml = "ERRO: Problema para alterar o registro!";
                    } 

                }

            } while (false);

            return cHtml;

        } //Tabelas_Gravar()


        public string Tabelas_Sucesso(HttpListenerRequest request, DBConnect MeuDBP, DBConnect MeuDB, ArtLib MeuLib, string cMeuPath, string cDados)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string cQueryString = HttpUtility.UrlDecode(cDados);
            Dictionary<string, string> oLogin = MeuLib.DMatriz(cQueryString, '&','=');
            string cHtml = "ERRO: Html nao atribuido";

            //"cTrace1=%3E%3E+Principal+%3E++Tabelas+Filtro+%3E+Tabelas+Browse+%3E+&cTrace2=Tabelas+Cadastro+%3E+&cLogin=Admin&cSessao=fnemG1242gF20S2lSi0&cMenu=2&cTabela=7&cNomeTabela=aa40tabelas&cRegistro=%21XREGISTRO%21&cOrigem=BROWSE&cItem=000&cItensExcluidos=&cOperacao=4&cXFiltro=IFdIRVJFIGlkU2VxdWVuY2lhbCA%2BPSAwICBBTkQgICBpZFNlcXVlbmNpYWwgPD0gOTk5OTk5OSAgQU5EIFRBQkVMQSA%2BPSAnICcgIEFORCBUQUJFTEEgPD0gJ3p6enp6enp6enonIA%3D%3D&nXPaginaAtu=1&nXPaginaFim=0&dados=Registro+alterado+com+sucesso%21&DESCRICAO=PERMISS%C1O+DE+ACESSO&DESTAQUE=&PASTAS="
            //"cTrace1=>> Principal >  Tabelas Filtro > Tabelas Browse > &cTrace2=Tabelas Cadastro > &cLogin=Admin&cSessao=fnemG1242gF2gS11Sgj&cMenu=2&cTabela=7&cNomeTabela=aa40tabelas&cRegistro=!XREGISTRO!&cOrigem=BROWSE&cItem=000&cItensExcluidos=&cOperacao=4&cXFiltro=IFdIRVJFIGlkU2VxdWVuY2lhbCA+PSAwICBBTkQgICBpZFNlcXVlbmNpYWwgPD0gOTk5OTk5OSAgQU5EIFRBQkVMQSA+PSAnICcgIEFORCBUQUJFTEEgPD0gJ3p6enp6enp6enonIA==&nXPaginaAtu=1&nXPaginaFim=0&dados=Registro alterado com sucesso!&DESCRICAO=PERMISS�O DE ACESSO&DESTAQUE=&PASTAS="

            LogFile.Log(" --- Tabelas_Sucesso:");

            do
            {

                string cTabela     = Convert.ToString(oLogin["tabela"]);
                string cNomeTabela = oLogin["nometabela"];
                string cPaginaAtu  = oLogin["paginaatu"];
                string cPaginaFim  = oLogin["paginafim"];

                string cAcao = "pagina_browser";
                string cQuery = "";

                //Na inclusão, verifica em qual página do browse irá se encaixar
                if (oLogin["operacao"] == "3") //Inclusao
                {
                    cQuery = String.Format("select max(idSequencial) as xIdSeq from {0} ", cNomeTabela);
                    List<string> campos = new List<string>(new string[] { "xIdSeq" });
                    MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                    if ((list.Count < campos.Count) || (list["xIdSeq"].Count <= 0))
                    {
                        LogFile.Log(string.Format("Tabelas_Sucesso: tabela nao encontrada! Tabela: {0}", cTabela));
                        cHtml = "ERRO: Tabela invalida!";
                        break;
                    }

                    string cIdSequencial = list["xIdSeq"].First();

                    string cOrderBy = MeuDB.OrdemTabela(cTabela);

                    cQuery = "select xseq from ";
                    cQuery += String.Format(" (select t01.*, (@seq := @seq + 1) as xseq from (SELECT @seq := 0) AS nada, {0} as t01 order by {1}) as tabela ", cNomeTabela, cOrderBy);
                    cQuery += String.Format(" where idSequencial = {0} ", cIdSequencial);

                    campos = new List<string>(new string[] { "xseq" });
                    list = MeuDB.Select(cQuery, campos, false);

                    if ((list.Count < campos.Count) || (list["xseq"].Count <= 0))
                    {
                        LogFile.Log(string.Format("Tabelas_Sucesso: tabela nao encontrada! Tabela: {0}", cTabela));
                        cHtml = "ERRO: Tabela invalida!";
                        break;
                    }

                    int nReg = Int32.Parse(list["xseq"].First());

                    int nPagina = (((int)(nReg / MeuLib.nTPagina))+1);

                    cPaginaAtu = nPagina.ToString();
                }

                //Ver pagina final, para inclusao e exclusao
                if ("35".Contains(oLogin["operacao"])) //Inclusao
                {
                    cQuery = String.Format("select count(*) as nRegs from {0} ", cNomeTabela);
                    List<string> campos = new List<string>(new string[] { "nRegs" });
                    MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos, false);

                    if ((list.Count < campos.Count) || (list["nRegs"].Count <= 0))
                    {
                        LogFile.Log(string.Format("Tabelas_Sucesso: tabela nao encontrada! Tabela: {0}", cTabela));
                        cHtml = "ERRO: Tabela invalida!";
                        break;
                    }

                    int nReg = Int32.Parse(list["nRegs"].First()) - 1;

                    int nPagina = (((int)(nReg / MeuLib.nTPagina)) + 1);

                    if (Int32.Parse(cPaginaAtu) > nPagina)
                    {
                        cPaginaAtu = nPagina.ToString();
                    }

                    cPaginaFim = nPagina.ToString();
                }

                cHtml = "<html>";
                cHtml += " <head>";
                cHtml += " <title>Suporte - Mensagem</title>";
                cHtml += " <script type=\"text/JavaScript\">";
                cHtml += " function fLogin() {";
                cHtml += "     top.window.location=\"./ login\";";
                cHtml += " } ";
                cHtml += " </script>";
                cHtml += " </head>";
                cHtml += " <body bgcolor=\"#BEBEBE\">";
                cHtml += " <br><br><br><br><br><br><br><br><br><br><br>" + Environment.NewLine;
                cHtml += String.Format(" <form name=\"form1\" method=\"post\" action=\"{0}\">", cAcao) + Environment.NewLine;

                cHtml += String.Format(" <input name=\"trace1\"      id=\"trace1\"      type=\"hidden\" value=\"{0}\">", oLogin["trace1"])     + Environment.NewLine;
                cHtml += String.Format(" <input name=\"trace2\"      id=\"trace2\"      type=\"hidden\" value=\"{0}\">", oLogin["trace2"])     + Environment.NewLine;
                cHtml += String.Format(" <input name=\"login\"       id=\"login\"       type=\"hidden\" value=\"{0}\">", oLogin["login"])      + Environment.NewLine;
                cHtml += String.Format(" <input name=\"sessao\"      id=\"sessao\"      type=\"hidden\" value=\"{0}\">", oLogin["sessao"])     + Environment.NewLine;
                cHtml += String.Format(" <input name=\"menu\"        id=\"menu\"        type=\"hidden\" value=\"{0}\">", oLogin["menu"])       + Environment.NewLine;
                cHtml += String.Format(" <input name=\"tabela\"      id=\"tabela\"      type=\"hidden\" value=\"{0}\">", oLogin["tabela"])     + Environment.NewLine;
                cHtml += String.Format(" <input name=\"nometabela\"  id=\"nometabela\"  type=\"hidden\" value=\"{0}\">", oLogin["nometabela"]) + Environment.NewLine;
                cHtml += String.Format(" <input name=\"registro\"    id=\"registro\"    type=\"hidden\" value=\"{0}\">", oLogin["registro"])   + Environment.NewLine;
                cHtml += String.Format(" <input name=\"origem\"      id=\"origem\"      type=\"hidden\" value=\"{0}\">", oLogin["origem"])     + Environment.NewLine;
                cHtml += String.Format(" <input name=\"item\"        id=\"item\"        type=\"hidden\" value=\"{0}\">", oLogin["item"])       + Environment.NewLine;
                cHtml += String.Format(" <input name=\"operacao\"    id=\"operacao\"    type=\"hidden\" value=\"{0}\">", oLogin["operacao"])   + Environment.NewLine;
                cHtml += String.Format(" <input name=\"filtro\"      id=\"filtro\"      type=\"hidden\" value=\"{0}\">", oLogin["filtro"])    + Environment.NewLine;
                cHtml += String.Format(" <input name=\"paginaatu\"   id=\"paginaatu\"   type=\"hidden\" value=\"{0}\">", oLogin["paginaatu"]) + Environment.NewLine;
                cHtml += String.Format(" <input name=\"paginafim\"   id=\"paginafim\"   type=\"hidden\" value=\"{0}\">", oLogin["paginafim"]) + Environment.NewLine;
                cHtml += String.Format(" <input name=\"XSEQUENCIAL1\"   id=\"XSEQUENCIAL1\"   type=\"hidden\" value=\"{0}\">", "") + Environment.NewLine;
                cHtml += String.Format(" <input name=\"XSEQUENCIAL2\"   id=\"XSEQUENCIAL2\"   type=\"hidden\" value=\"{0}\">", "9999999") + Environment.NewLine;

                cHtml += String.Format(" <center>{0}</center>", oLogin["dados"]) + Environment.NewLine;
                cHtml += " <br><br>" + Environment.NewLine;
                cHtml += " <center><input type=\"submit\" name=\"voltar\" value=\"Voltar\"></center>" + Environment.NewLine;
                cHtml += " </form>" + Environment.NewLine;
                cHtml += " </body>" + Environment.NewLine;
                cHtml += "</html> " + Environment.NewLine;

            } while (false);

            return cHtml;

        } //Tabelas_Sucesso()

    }
}
