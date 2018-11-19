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
    public class Browser
    {
        public string Tabelas_Filtro(HttpListenerRequest request, DBConnect MeuDB, ArtLib MeuLib, string cMeuPath, string cDados)
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

                string cQuery = string.Format("SELECT * FROM aa30menu WHERE idSequencial = {0} ", oLogin["menu"]); //Menu

                //Opções de Menu do usuário
                List<string> campos = new List<string>(new string[] { "id1aa40tabelas", "DESCRICAO" });

                MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["id1aa40tabelas"].Count <= 0))
                {
                    LogFile.Log(string.Format("Tabelas_Filtro: Problema para obter os dados do menu! Menu: {0}", oLogin["login"]));
                    break;
                }

                string cTabela = list["id1aa40tabelas"].First();
                string cTrace = list["DESCRICAO"].First();

                if (String.IsNullOrEmpty(cTabela) || (cTabela == "0"))
                {
                    cHtml = Distribuidor.S_mensagem(string.Format("Tabela nao definida para esta opcao de menu! Menu: {0} ", oLogin["menu"]), "javascript:history.go(-1)");
                    break;
                }

                cHtml = cHtml.Replace("!XTABELA!", cTabela);
                cHtml = cHtml.Replace("!XLOGIN!", oLogin["login"]);
                cHtml = cHtml.Replace("!XSESSAO!", oLogin["sessao"]);
                cHtml = cHtml.Replace("!XMENU!", oLogin["menu"]);
                cHtml = cHtml.Replace("!XTRACE1!", cTrace);
                cHtml = cHtml.Replace("!XTRACE2!", cTrace.ToUpper());

                //--------------- Grupos de Browser --------------------------------------------------

                List<string> listGrupo = Grupos(MeuDB, cTabela);

                //--------------- Campos -------------------------------------------------------------

                cQuery = string.Format("SELECT * FROM aa41campos WHERE id0aa40tabelas = {0} and FILTRO <> 1", cTabela);

                //Opções de Menu do usuário
                campos = new List<string>(new string[] { "idSequencial", "CAMPO", "TITULO", "TAMANHO", "FILTRO", "FILTRODE", "FILTROATE" });

                list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count))
                {
                    cHtml = cHtml.Replace("!XPARAMETROS!", "");

                    LogFile.Log(string.Format("Tabelas_Filtro: Campos nao encontrado! Tabela: {0}", cTabela));
                    break;
                }

                //--------------- Filtros -------------------------------------------------------------
                string cAux = "";

                for (int i = 0; i < list["idSequencial"].Count; i++)
                {

                    if (list["FILTRO"].ElementAt(i) == "2")
                    {
                        //Parametro de:
                        cAux += "<div><TABLE style='width: 100%; '>";
                        cAux += string.Format("<td style='left: 030px; width: 160px; text-align:right; font-size:11pt; '>{0} de:</td>", list["TITULO"].ElementAt(i)); //Titulo
                        cAux += "<td style='left: 170px;'>";
                        cAux += string.Format("<Input TYPE=text ID='X{0}1'VALUE='{2}' style='border-color:#808080; border-width:thin;' MaxLength={3} SIZE={4} />", list["CAMPO"].ElementAt(i), list["CAMPO"].ElementAt(i), list["FILTRODE"].ElementAt(i), list["TAMANHO"].ElementAt(i), list["TAMANHO"].ElementAt(i));
                        cAux += "</td></TABLE></div>";
                        //Parametro ate:
                        cAux += "<div><TABLE style='width: 100%; '>";
                        cAux += string.Format("<td style='left: 030px; width: 160px; text-align:right; font-size:11pt; '>{0} ate:</td>", list["TITULO"].ElementAt(i)); //Titulo
                        cAux += "<td style='left: 170px;'>";
                        cAux += string.Format("<Input TYPE=text ID='X{0}2' VALUE='{2}' style='border-color:#808080; border-width:thin;' MaxLength={3} SIZE={4} />", list["CAMPO"].ElementAt(i), list["CAMPO"].ElementAt(i), list["FILTROATE"].ElementAt(i), list["TAMANHO"].ElementAt(i), list["TAMANHO"].ElementAt(i));
                        cAux += "</td></TABLE></div>";
                    }
                    else if (list["FILTRO"].ElementAt(i) == "3")
                    {
                        //Parametro parcial:
                        cAux += "<div><TABLE style='width: 100%; '>";
                        cAux += string.Format("<td style='left: 030px; width: 160px; text-align:right; font-size:11pt; '>{0} [contem]:</td>", list["TITULO"].ElementAt(i)); //Titulo
                        cAux += "<td style='left: 170px;'>";
                        cAux += string.Format("<Input TYPE=text ID='X{0}3' VALUE='' style='border-color:#808080; border-width:thin;' MaxLength={2} SIZE={3} />", list["CAMPO"].ElementAt(i), list["CAMPO"].ElementAt(i), list["TAMANHO"].ElementAt(i), list["TAMANHO"].ElementAt(i));
                        cAux += "</td></TABLE></div>";
                    }
                    else if (list["FILTRO"].ElementAt(i) == "4")
                    {
                        //Parametro integral:
                        cAux += "<div><TABLE style='width: 100%; '>";
                        cAux += string.Format("<td style='left: 030px; width: 160px; text-align:right; font-size:11pt; '>{0}:</td>", list["TITULO"].ElementAt(i)); //Titulo
                        cAux += "<td style='left: 170px;'>";
                        cAux += string.Format("<Input TYPE=text ID='X{0}4' VALUE='' style='border-color:#808080; border-width:thin;' MaxLength={2} SIZE={3} />", list["CAMPO"].ElementAt(i), list["CAMPO"].ElementAt(i), list["TAMANHO"].ElementAt(i), list["TAMANHO"].ElementAt(i));
                        cAux += "</td></TABLE></div>";
                    }

                    for (int j = 0; j < listGrupo.Count; j++)
                    {
                        if (listGrupo[j] == list["idSequencial"].ElementAt(i))
                        {
                            //Parametro parcial:
                            cAux += "<div><TABLE style='width: 100%; '>";
                            cAux += string.Format("<td style='left: 030px; width: 160px; text-align:right; font-size:11pt; '>{0} [contem]:</td>", list["TITULO"].ElementAt(i)); //Titulo
                            cAux += "<td style='left: 170px;'>";
                            cAux += string.Format("<Input TYPE=text ID='X{0}5' VALUE='' style='border-color:#808080; border-width:thin;' MaxLength={2} SIZE={3} />", list["CAMPO"].ElementAt(i), list["CAMPO"].ElementAt(i), "50", "50");
                            cAux += "</td></TABLE></div>";
                        }
                    }

                }

                cHtml = cHtml.Replace("!XPARAMETROS!", cAux);

            } while (false);

            LogFile.Log(" --- Fim Tabelas_Filtro!");

            return cHtml;
        }

        private static List<string> Grupos(DBConnect MeuDB, string cTabela)
        {

            List<string> Grupo = new List<string>();

            string cQuery = string.Format("SELECT a43.id0aa41campos FROM aa42indices a42, aa43cmpindices a43 " +
                                          "WHERE a43.id0aa42indices = a42.idSequencial " +
                                          "AND a43.QUEBRA = 'S' AND a42.id0aa40tabelas = {0} ", cTabela);
            //Opções de Menu do usuário
            List<string> campos = new List<string>(new string[] { "id0aa41campos" });

            MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

            if ((list.Count < campos.Count) || (list["id0aa41campos"].Count <= 0)) { return Grupo; }

            for (int i = 0; i < list["id0aa41campos"].Count; i++)
            {
                Grupo.Add(list["id0aa41campos"].ElementAt(i));
            }

            return Grupo;

        }

        public string Pagina_Browser(HttpListenerRequest request, DBConnect MeuDB, ArtLib MeuLib, string cMeuPath, string cDados)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string cQueryString = HttpUtility.UrlDecode(request.Url.Query);
            dynamic oLogin = serializer.Deserialize<dynamic>(cDados.Replace("dados=", ""));
            string cHtml = "ERRO: Html nao atribuido";

            LogFile.Log(" --- Pagina_Browser:");

            if (!String.IsNullOrEmpty(cQueryString))
            {
                int nP = (cQueryString.Contains("dados=") ? 7 : 1);
                oLogin = serializer.Deserialize<dynamic>(cQueryString.Substring(nP));
            }

            do
            {

                cHtml = Distribuidor.Generico(request, MeuDB, MeuLib, cMeuPath, cDados);

                if (cHtml.Contains("Sessao Expirou")) { break; }

                //--------------- Filtro -----------------------------------------------------------

                MultiValueDictionary<int, string> aCampos = new MultiValueDictionary<int, string>();

                String cFiltro = Pagina_Browser_Filtro(MeuDB, MeuLib, oLogin, ref aCampos);

                //--------------- Usuario -----------------------------------------------------------

                string cQuery = string.Format("SELECT * FROM aa10usuarios WHERE USUARIO = '{0}' ", oLogin["login"]);

                List<string> campos = new List<string>(new string[] { "idSequencial", "PERMISSAO" });

                MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count))
                {
                    LogFile.Log(string.Format("Pagina_Browser: Usuario nao encontrado! Tabela: {0}", oLogin["login"]));
                    break;
                }

                string cIdUsuario = list["idSequencial"].First();
                string cPermissao = list["PERMISSAO"].First();

                //--------------- Tem Campo RETIRAR e STATUS -------------------------------------------

                cQuery = string.Format("select Count(If(CAMPO='RETIRAR',1,null)) REGS1, Count(If(CAMPO='STATUS',1,null)) REGS2 FROM aa41campos WHERE id0aa40tabelas =  {0} ", oLogin["tabela"]);

                campos = new List<string>(new string[] { "REGS1", "REGS2" });

                list = MeuDB.Select(cQuery, campos);

                bool lCampoRETIRAR = !((list.Count < campos.Count) || (Int32.Parse(list["REGS1"].First()) <= 0));
                bool lCampoSTATUS = !((list.Count < campos.Count) || (Int32.Parse(list["REGS2"].First()) <= 0));

                //--------------- Trace do processo --------------------------------------------------

                cQuery = string.Format("SELECT * FROM aa30menu WHERE idSequencial = {0} ", oLogin["menu"]);

                campos = new List<string>(new string[] { "MENU", "DESCRICAO" });

                list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["MENU"].Count <= 0))
                {
                    LogFile.Log(string.Format("Pagina_Browser: Problema para obter os dados do menu! Menu: {0}", oLogin["menu"]));
                    break;
                }

                string cCodMenu = list["MENU"].First();
                string cTrace2 = list["DESCRICAO"].First();

                //--------------- Opcoes de menu do Cadastro --------------------------------------------------

                cQuery = "SELECT * FROM aa30menu a30 ";
                cQuery += (cPermissao == "1" ? "" : ", aa11permissao a11 ");
                cQuery += " WHERE (MENU LIKE '%{0}%') AND (TIPO >= '1') AND (TIPO <= '9') AND (STATUS = 'A') ";
                cQuery += (cPermissao == "1" ? "" : " AND a30.idSequencial = a11.id0aa30menu AND a11.id0aa10usuarios = " + cIdUsuario);
                cQuery += " ORDER BY REPLACE(MENU, 'IN', '0IN') ";

                cQuery = String.Format(cQuery, cCodMenu);

                campos = new List<string>(new string[] { "MENU", "DESCRICAO", "TIPO", "IMAGEM", "PAGINA" });

                list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["MENU"].Count <= 0))
                {
                    LogFile.Log(string.Format("Pagina_Browser: Problema para obter os dados do menu do cadastro! Menu: {0}", cCodMenu));
                    break;
                }

                string cOpcao = "";
                string cImagem = "";
                string cPagina = "";
                int nA = 0;

                MultiValueDictionary<int, string> aOpcoes = new MultiValueDictionary<int, string>();

                for (int i = 0; i < list["MENU"].Count; i++)
                {
                    cOpcao = list["MENU"].ElementAt(i);
                    cPagina = (String.IsNullOrEmpty(list["PAGINA"].ElementAt(i)) ? "pagina_cadastro" : list["PAGINA"].ElementAt(i));

                    if (cOpcao.Length != (cCodMenu.Length + 2))
                    {
                        continue;
                    }

                    if (cCodMenu + "IN" == cOpcao) { cImagem = "portal_mais.gif"; }
                    else if (cCodMenu + "AL" == cOpcao) { cImagem = "portal_edit.gif"; }
                    else if (cCodMenu + "EX" == cOpcao)
                    {
                        //Ignora opção excluir para a tabela que tem o campo RETIRAR
                        if (lCampoRETIRAR) { continue; }

                        cImagem = "portal_menos.gif";
                    } else if (!String.IsNullOrEmpty(list["IMAGEM"].ElementAt(i))) { cImagem = list["IMAGEM"].ElementAt(i); }

                    aOpcoes.Add(nA, cOpcao);
                    aOpcoes.Add(nA, list["DESCRICAO"].ElementAt(i));
                    aOpcoes.Add(nA, list["TIPO"].ElementAt(i));
                    aOpcoes.Add(nA, cImagem);
                    aOpcoes.Add(nA, cPagina);
                    nA++;

                }

                //--------------- Cabeçalho e Linha --------------------------------------------------

                string cCabecalho = "";
                string cLinha = "";

                if (lCampoSTATUS)
                {
                    cCabecalho += "<div style='margin - right: 1mm; margin - left: 1mm; display: inline-block;'><strong>Sts</strong></div>";
                }

                cLinha = String.Format("<tbody id='meutab'  data-bind='foreach: xbrowse{0}'>", oLogin["tabela"]);

                //Grupo Browser
                int nColunas = (1 + (lCampoSTATUS ? 1 : 0) + aCampos.Count + 2);

                cLinha += String.Format("<tr data-bind='visible: $root.veGRUPO($index(),TR_GRUPO())'><td colspan={0} bgcolor=#CCCCFF style='padding-left: 10px;' font='bold;' data-bind='text: $root.atualGRUPO'></td></tr>", nColunas.ToString());
                //SubGrupo Browser
                cLinha += String.Format("<tr data-bind='visible: $root.veSUBGRUPO($index(),TR_SUBGRUPO())'><td colspan={0} bgcolor=#CCCCFF style='padding-left: 20px;' font='bold;' data-bind='text: $root.atualSUBGRUPO'></td></tr>", nColunas.ToString());
                //MiniGrupo Browser
                cLinha += String.Format("<tr data-bind='visible: $root.veMINIGRUPO($index(),TR_MINIGRUPO())'><td colspan={0} bgcolor=#CCCCFF style='padding-left: 30px;' font='bold;' data-bind='text: $root.atualMINIGRUPO'></td></tr>", nColunas.ToString());

                cLinha += "<tr data-bind='attr: { id: TR_ID }'>";

                cLinha += "   <td>";
                cLinha += "  	<a name='2' href data-bind=\"click: $root.manutencao, attr: {title : 'tabelas_cadastro'}\"><img src='portal_visualizar.gif' border='0' title='Visualizar'></a>&nbsp;";

                for (int i = 0; i < aOpcoes.Count; i++)
                {
                    cLinha += "&nbsp;  <a name='" + aOpcoes[i].ElementAt(2) + "' href data-bind=\"click: $root.manutencao, attr: {title : '" + aOpcoes[i].ElementAt(4) + "'}\"><img src='" + aOpcoes[i].ElementAt(3) + "' width=18px height=16px border='0' title='" + aOpcoes[i].ElementAt(1) + "'></a>";
                }

                cLinha += "   </td>";

                if (lCampoSTATUS)
                {
                    cLinha += "<td data-bind=\"template: { name: 'statusTemplate1', data: IMGSTATUS }\"></td>";
                }

                //--------------- Grupos de Browser --------------------------------------------------

                bool lTaEmGrupo = false;

                List<string> listGrupo = Grupos(MeuDB, oLogin["tabela"]);

                for (int i = 0; i < aCampos.Count; i++)
                {
                    for (int j = 0; j < listGrupo.Count; j++)
                    {
                        if (listGrupo[j] == aCampos[i].ElementAt(0))
                        {
                            lTaEmGrupo = true;
                            break;
                        }
                    }

                    if (!lTaEmGrupo)
                    {
                        cCabecalho += String.Format("<div style='margin-right: 1mm; margin-left: 1mm;display: inline-block;'><strong>{0}</strong></div>", aCampos[i].ElementAt(0));

                        cLinha += String.Format("<td data-bind=\"text: {0}\" style='margin-left:1mm; margin-right:1mm; font-weight:bold;'></td>", aCampos[i].ElementAt(1));
                    }
                }

                cLinha += "   <td data-bind='text: idSequencial' style='margin-left:1mm; margin-right:10px;text-align:right; font-weight:bold;'></td>";
                cLinha += "</tr>";
                cLinha += "</tbody>";

                //--------------- Nome da Tabela --------------------------------------------------

                cQuery = String.Format("SELECT TABELA FROM aa40tabelas WHERE idSequencial = {0} ", oLogin["tabela"]);

                campos = new List<string>(new string[] { "TABELA" });

                list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["TABELA"].Count <= 0))
                {
                    LogFile.Log(string.Format("Pagina_Browser: Problema para obter o nome da tabela! Tabela: {0}", oLogin["tabela"]));
                    break;
                }

                string cNomeTabela = list["TABELA"].First();

                //--------------- Total de paginas --------------------------------------------------

                string cTrace1 = "";
                int nRegs = 0;
                int nPaginaAtu = 0;
                int nPaginaFim = 0;

                /*
                if (oLogin["paginaatu"] != null)
                {
                    nPaginaAtu = oLogin["paginaatu"];
                    nPaginaFim = oLogin["paginafim"];
                    cTrace1    = oLogin["trace1"];
                }
                else
                {
                */
                    cTrace1 = oLogin["trace1"] + oLogin["trace2"];

                    nRegs = MeuDB.Count(String.Format("SELECT Count(idSequencial) REGS FROM {0} ", cNomeTabela));

                    if (nRegs < 0)
                    {
                        LogFile.Log(string.Format("Pagina_Browser: Problema para obter o total de registros da tabela! Tabela: {0}", cNomeTabela));
                        break;
                    }
                    else if (nRegs > 0)
                    {
                        nPaginaFim = (int)(nRegs / MeuLib.nTPagina);
                    }

                //}

                //--------------- Pagina final --------------------------------------------------

                cHtml = cHtml.Replace("!XCABECALHO_TABELAS!", cCabecalho);
                cHtml = cHtml.Replace("!XLINHAS_TABELAS!", cLinha);

                cHtml = cHtml.Replace("!XLOGIN!", oLogin["login"]);
                cHtml = cHtml.Replace("!XSESSAO!", oLogin["sessao"]);
                cHtml = cHtml.Replace("!XMENU!", oLogin["menu"]);
                cHtml = cHtml.Replace("!XTRACE1!", cTrace1);
                cHtml = cHtml.Replace("!XTRACE2!", cTrace2);
                cHtml = cHtml.Replace("!XTRACE3!", cTrace2.ToUpper());
                cHtml = cHtml.Replace("!XTABELA!", oLogin["tabela"]);
                cHtml = cHtml.Replace("!XTABPASTA!", oLogin["tabela"]);
                cHtml = cHtml.Replace("!XNOMETABELA!", cNomeTabela);
                cHtml = cHtml.Replace("!XVIEWFILTRO!", cFiltro);
                cHtml = cHtml.Replace("!XFILTRO!", cFiltro);

                cHtml = cHtml.Replace("!XPAGINAATU!", nPaginaAtu.ToString());
                cHtml = cHtml.Replace("!XPAGINAFIM!", nPaginaFim.ToString());

            } while (false);

            LogFile.Log(" --- Fim Pagina_Browser!");

            return cHtml;
        }

        private string Pagina_Browser_Filtro(DBConnect MeuDB, ArtLib MeuLib, dynamic oLogin, ref MultiValueDictionary<int, string> aCampos)
        {
            String cFiltro = "";

            //--------------- Grupos de Browser --------------------------------------------------
            List<string> listGrupo = Grupos(MeuDB, oLogin["tabela"]);

            //Parametro de:
            cFiltro += String.Format(" WHERE {0} >= {1} ", "idSequencial", oLogin["XSEQUENCIAL1"]);
            //Parametro ate:
            cFiltro += String.Format(" AND   {0} <= {1} ", "idSequencial", oLogin["XSEQUENCIAL2"]);

            do
            {
                //--------------- Campos -------------------------------------------------------------

                string cQuery = string.Format("SELECT * FROM aa41campos WHERE id0aa40tabelas = {0} AND BROWSESN='S' AND OPERACAO <> 'I' ORDER BY ORDEM ", oLogin["tabela"]);

                //Opções de Menu do usuário
                List<string> campos = new List<string>(new string[] { "idSequencial", "CAMPO", "TITULO", "FILTRO", "OPCOES", "BROWSETIT" });

                MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["idSequencial"].Count <= 0)) { break; }

                string cParametro = "";
                string cBrowseTit = "";

                for (int i = 0; i < list["idSequencial"].Count; i++)
                {

                    cBrowseTit = (String.IsNullOrEmpty(list["BROWSETIT"].ElementAt(i)) ? list["TITULO"].ElementAt(i) : list["BROWSETIT"].ElementAt(i));

                    aCampos.Add(i, cBrowseTit);
                    aCampos.Add(i, list["CAMPO"].ElementAt(i));
                    aCampos.Add(i, list["OPCOES"].ElementAt(i));
                    aCampos.Add(i, list["idSequencial"].ElementAt(i));

                    //Filtro
                    if (list["FILTRO"].ElementAt(i) == "2")
                    {
                        //Parametro de:
                        cParametro = String.Format("X{0}1", list["CAMPO"].ElementAt(i));
                        cFiltro += String.Format(" AND {0} >= '{1}' ", list["CAMPO"].ElementAt(i), oLogin[cParametro]);
                        //Parametro ate:
                        cParametro = String.Format("X{0}2", list["CAMPO"].ElementAt(i));
                        cFiltro += String.Format(" AND {0} <= '{1}' ", list["CAMPO"].ElementAt(i), oLogin[cParametro]);
                    }
                    else if (list["FILTRO"].ElementAt(i) == "3")
                    {
                        //Parametro parcial:
                        cParametro = String.Format("X{0}3", list["CAMPO"].ElementAt(i));
                        if (!String.IsNullOrEmpty(oLogin[cParametro]))
                        {
                            cFiltro += String.Format(" AND {0} LIKE '%{1}%' ", list["CAMPO"].ElementAt(i), oLogin[cParametro]);
                        }
                    }
                    else if (list["FILTRO"].ElementAt(i) == "4")
                    {
                        //Parametro integral:
                        cParametro = String.Format("X{0}4", list["CAMPO"].ElementAt(i));
                        cFiltro += String.Format(" AND {0} = '{1}' ", list["CAMPO"].ElementAt(i), oLogin[cParametro]);
                    } //if

                    for (int j = 0; j < listGrupo.Count; j++)
                    {
                        if (listGrupo[j] == list["idSequencial"].ElementAt(i))
                        {
                            //Parametro parcial:
                            cParametro = String.Format("X{0}5", list["CAMPO"].ElementAt(i));
                            if (!String.IsNullOrEmpty(oLogin[cParametro]))
                            {
                                cFiltro += String.Format(" AND {0} LIKE '%{1}%' ", list["CAMPO"].ElementAt(i), oLogin[cParametro]);
                            }
                        }

                    } //for[j]
                    
                } //for[i]

            } while (false);

            return MeuLib.Base64Encode(cFiltro);
        }

        public string Obter_Browser(HttpListenerRequest request, DBConnect MeuDB, ArtLib MeuLib, string cMeuPath, string cDados)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string cQueryString = HttpUtility.UrlDecode(request.Url.Query);
            dynamic oLogin = serializer.Deserialize<dynamic>(cDados.Replace("dados=", ""));
            string cHtml = "ERRO: Html nao atribuido";

            LogFile.Log(" --- Obter_Browser:");

            if (!String.IsNullOrEmpty(cQueryString))
            {
                int nP = (cQueryString.Contains("dados=") ? 7 : 1);
                oLogin = serializer.Deserialize<dynamic>(cQueryString.Substring(nP));
            }

            do
            {

                if (String.IsNullOrEmpty(oLogin["login"]))
                {
                    cHtml = string.Format("ERRO: Login deve ser informado! {0}", DateTime.Now);
                    break;
                }

                if (String.IsNullOrEmpty(oLogin["sessao"]))
                {
                    cHtml = string.Format("ERRO: Sessao deve ser informada! {0}", DateTime.Now);
                    break;
                }

                if (!Distribuidor.Check_Sessao(request, MeuDB, MeuLib, oLogin["login"], oLogin["sessao"]))
                {
                    cHtml = Distribuidor.S_mensagem("Sessão expirou! Logue novamente!", "javascript:fLogin()");
                    break;
                }

                //--------------- Query: Limit -------------------------------------------------------

                string cLimite = "";

                if  (oLogin["paginaatu"] > 0)
                {
                    cLimite = String.Format(" Limit {0}, {1} ", (oLogin["paginaatu"]-1)*MeuLib.nTPagina, MeuLib.nTPagina);
                }

                //--------------- Filtro -----------------------------------------------------------

                string cFiltro = oLogin["filtro"];

                cFiltro = MeuLib.Base64Decode(cFiltro);

                string[] aTabela = null;

                if  (oLogin["tabela"].Contains("|"))
                {
                    aTabela = oLogin["tabela"].Split("|");
                }
                else
                {
                    aTabela = new string[] { oLogin["tabela"] };
                }
                

                string cJSon = "{";

                for (int i=0; i < aTabela.Length; i++)
                {
                    cJSon += Obter_Browse_JSon(MeuDB, MeuLib, aTabela[i], cFiltro, cLimite);
                }

                cJSon += "}";

                cHtml = cJSon;

            } while (false);

            LogFile.Log(" --- Fim Obter_Browser!");

            return cHtml;
        }

        private static string Obter_Browse_JSon(DBConnect MeuDB, ArtLib MeuLib, string cTabela, string cFiltro, string cLimite)
        {

            LogFile.Log("Obter_Browse_Inicio: cTabela: " + cTabela);

            string cJSon = "";

            do
            {
                //--------------- Nome e ordem da Tabela -----------------------------------------------------------

                string cQuery = string.Format("SELECT * FROM aa40tabelas WHERE idSequencial = {0} ", cTabela);
                List<string> campos = new List<string>(new string[] { "TABELA" });
                MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);
                if ((list.Count < campos.Count))
                {
                    LogFile.Log(string.Format("Obter_Browse_Inicio: Problema para obter o Nome da Tabela! Tabela: {0}", cTabela));
                    break;
                }
                string cNomeTabela = list["TABELA"].First();

                string cOrderBy = MeuDB.OrdemTabela(cTabela);

                //--------------- Tem Status da Tabela -----------------------------------------------------------

                bool lTemStatus = false;
                int nRegs = MeuDB.Count(String.Format("SELECT Count(idSequencial) REGS FROM aa46status WHERE id0aa40tabelas = '{0}' ", cTabela));
                MultiValueDictionary<string, string> aStatus = null;

                if (nRegs < 0)
                {
                    LogFile.Log(string.Format("Obter_Browse_Inicio: Problema para obter o total de status da tabela! Tabela: {0}", cTabela));
                    break;
                }
                else if (nRegs > 0)
                {
                    lTemStatus = true;

                    cQuery = string.Format("SELECT CODIGO, DESCRICAO, STATUS, " +
                                           "(SELECT DESCRICAO FROM aa46status b46 WHERE b46.id0aa40tabelas = 13 AND b46.CODIGO = a46.STATUS LIMIT 0,1) AS COR " +
                                           "FROM aa46status a46 " +
                                           "WHERE id0aa40tabelas = {0}  ORDER BY ORDEM ", cTabela);
                    campos = new List<string>(new string[] { "CODIGO", "DESCRICAO", "STATUS", "COR" });
                    aStatus = MeuDB.Select(cQuery, campos);
                }

                //--------------- Campos da Tabela -----------------------------------------------------------

                cQuery = string.Format("SELECT * FROM aa41campos WHERE id0aa40tabelas = {0} AND BROWSESN='S' AND OPERACAO <> 'I' ORDER BY ORDEM", cTabela);
                campos = new List<string>(new string[] { "TITULO", "CAMPO", "OPCOES", "CONSULTATIPO", "CONSULTACODIGO", "CONSULTACAMPO", "IDSEQUENCIAL" });
                if (lTemStatus) { campos.Add("STATUS"); }

                MultiValueDictionary<string, string> aCampos = MeuDB.Select(cQuery, campos);
                if ((aCampos.Count < campos.Count))
                {
                    LogFile.Log(String.Format("Obter_Browse_Inicio: Problema para obter os Campos da Tabela! Tabela: {0}", cTabela));
                    break;
                }

                string cQueryCombo = "";
                int nSomaStatus = (lTemStatus ? 1 : 0);
                string[] aXCampos = new string[aCampos["CAMPO"].Count + 1 + nSomaStatus];

                for (int i = 0; i < aCampos["CAMPO"].Count(); i++)
                {
                    aXCampos[i] = aCampos["CAMPO"].ElementAt(i);

                    if  (aCampos["CONSULTATIPO"].ElementAt(i) == "1" /*Combo de tabela*/)
                    {
                        cQueryCombo = "," + MeuDB.SQLCombo(aCampos["CONSULTACODIGO"].ElementAt(i), aCampos["CONSULTACAMPO"].ElementAt(i), aCampos["CAMPO"].ElementAt(i));
                        aXCampos = new List<string>(aXCampos) { null }.ToArray();
                        i++;
                        aXCampos[i] = "D" + aXCampos[i-1];
                    }
                }

                //Remove o campo STATUS se existir
                aXCampos = aXCampos.Where(x => x != "STATUS").ToArray();

                if  (lTemStatus)
                {
                    aXCampos[aXCampos.Length-2] = "IDSEQUENCIAL";
                    aXCampos[aXCampos.Length-1] = "STATUS";
                }
                else
                {
                    aXCampos[aXCampos.Length - 1] = "IDSEQUENCIAL";
                }

                //--------------- Trace do processo --------------------------------------------------

                cQuery = String.Format("Select * From (SELECT *{0} FROM {1} t1) t2 {2} {3} ", cQueryCombo, cNomeTabela, cFiltro, (String.IsNullOrEmpty(cOrderBy) ? "" : "ORDER BY " + cOrderBy)) + cLimite;

                campos = new List<string>(aXCampos);
                list = MeuDB.Select(cQuery, campos, false);
                if ((list.Count < campos.Count))
                {
                    LogFile.Log(string.Format("Obter_Browse_Inicio: Problema para obter o Nome da Tabela! Tabela: {0}", cTabela));
                    break;
                }

                //--------------- Grupos de Browser --------------------------------------------------
                List<string> listGrupo = Grupos(MeuDB, cTabela);

                //--------------- Montando o JSON --------------------------------------------------

                cJSon += (String.IsNullOrEmpty(cJSon) ? "" : ",");
                cJSon += String.Format("\"status\":\"1\", \"xbrowse{0}\":[", cTabela);

                string cJSon2 = "{";
                string cJSon3 = "";
                string cValor = "";
                string cStatus = "";
                string cImagem = "";
                int nLin = 0;
                int nG = 0;
                bool lTaEmGrupo = false;

                for (int i = 0; i < list[aXCampos[0]].Count; i++)
                {
                    cJSon += (cJSon2 == "{" ? "" : ",");
                    cJSon2 = "{";
                    cJSon3 = "";
                    nG = 0;

                    for (int j = 0; j < aCampos["CAMPO"].Count; j++)
                    {
                        cValor = list[aCampos["CAMPO"].ElementAt(j)].ElementAt(i);

                        if (!String.IsNullOrEmpty(aCampos["OPCOES"].ElementAt(j)))
                        {
                            cValor = MeuLib.ValCombo(aCampos["OPCOES"].ElementAt(j), cValor);
                        }

                        if (aCampos["CONSULTATIPO"].ElementAt(j) == "1") //Combo de consulta
                        {
                            cValor = list["D" + aCampos["CAMPO"].ElementAt(j)].ElementAt(i);
                        }

                        lTaEmGrupo = false;

                        for (int k = 0; k < listGrupo.Count(); k++)  //Grupos de registro
                        {
                            if (listGrupo[k] == aCampos["IDSEQUENCIAL"].ElementAt(j))
                            {
                                nG++;

                                cJSon3 += (nG == 1 ? String.Format(", \"TR_GRUPO\"     : \"{0}\" ", cValor) : "");
                                cJSon3 += (nG == 2 ? String.Format(", \"TR_SUBGRUPO\"  : \"{0}\" ", cValor) : "");
                                cJSon3 += (nG == 3 ? String.Format(", \"TR_MINIGRUPO\" : \"{0}\" ", cValor) : "");

                                lTaEmGrupo = true;
                            }

                        } //for k

                        if (!lTaEmGrupo)
                        {
                            cJSon2 += (cJSon2 == "{" ? "" : ",") + String.Format("\"{0}\" : \"{1}\"", aCampos["CAMPO"].ElementAt(j), cValor);
                        }


                    } //for j

                    if (lTemStatus)
                    {
                        cStatus = list["STATUS"].ElementAt(i);

                        cJSon2 += (cJSon2 == "{" ? "" : ",") + String.Format(" \"STATUS\" : \"{0}\" ", cStatus);

                        for (int k = 0; k < aStatus["CODIGO"].Count(); k++)  //Status da tabela
                        {
                            if (aStatus["CODIGO"].ElementAt(k) == cStatus)
                            {
                                cImagem = "{" + String.Format("\"imagem\" : \"portal_led_{0}.png\", \"titulo\" : \"{1}\"", aStatus["COR"].ElementAt(k), aStatus["DESCRICAO"].ElementAt(k)) + "}";
                                cJSon2 += String.Format(", \"IMGSTATUS\" : {0} ", cImagem);
                                break;
                            }
                        }

                    }

                    cJSon2 += (cJSon2 == "{" ? "" : ",") + String.Format(" \"idSequencial\" : {0} ", list["IDSEQUENCIAL"].ElementAt(i));

                    cJSon2 += String.Format(", \"TABELA\" : {0} ", cTabela);

                    cJSon2 += cJSon3;

                    if (cJSon3.IndexOf("TR_GRUPO") <= 0) { cJSon2 += ", \"TR_GRUPO\" : \"\", \"TR_SUBGRUPO\" : \"\", \"TR_MINIGRUPO\" : \"\""; }
                    else if (cJSon3.IndexOf("TR_SUBGRUPO") <= 0) { cJSon2 += ", \"TR_SUBGRUPO\" : \"\", \"TR_MINIGRUPO\" : \"\""; }
                    else if (cJSon3.IndexOf("TR_MINIGRUPO") <= 0) { cJSon2 += ", \"TR_MINIGRUPO\" : \"\""; }

                    nLin++;

                    cJSon2 += String.Format(", \"TR_ID\" : \"{0}\" ", ((nLin % 2) == 0 ? "xTitulo3" : "TRBranco"));

                    cJSon += cJSon2 + "}";

                } //for i

                cJSon += "]";

            } while (false);
            
            return cJSon;
        }
    }
}
