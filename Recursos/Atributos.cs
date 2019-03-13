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

//Campos_Browse

namespace Recursos
{
    class Atributos
    {
        public string Campos_Browse(HttpListenerRequest request, DBConnect MeuDB, ArtLib MeuLib, string cMeuPath, string cDados)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string cQueryString = HttpUtility.UrlDecode(request.Url.Query);
            string cJSon = HttpUtility.UrlDecode(cDados.Replace("dados=", ""));
            dynamic oLogin = serializer.Deserialize<dynamic>(cJSon);
            string cHtml = "ERRO: Html nao atribuido";

            LogFile.Log(" --- Campos_Browse:");

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




            } while (false);

            return cHtml;

        } //Campos_Browse()

        private string Campos_Browse_HTML(DBConnect MeuDB, ArtLib MeuLib, dynamic oLogin, string cHtml, string cUsuario, string cMenu, string cTabela, string cFiltrarSN, 
                                          ref string cTrace2, ref string cFiltro, ref string cCabecalho, ref string cStrLinha)
        {

            do
            {

                //--------------- Dados do Usuario

                string cQuery = String.Format("SELECT * FROM aa10usuarios a10 Where a10.USUARIO = '{0}' ", cUsuario);
                List<string> campos = new List<string>(new string[] { "idSequencial", "PERMISSAO" });
                MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["idSequencial"].Count <= 0))
                {
                    LogFile.Log(String.Format("Campos_Browse_HTML: Problema para obter os dados do Usuario! Usuario: {0}", cUsuario));
                    LogFile.Log(cQuery);
                    break;
                }

                string cIdUsuario = list["idSequencial"].First();
                string cPermissao = list["PERMISSAO"].First();

                //--------------- Trace do Processo

                cQuery = String.Format("SELECT MENU, DESCRICAO FROM aa30menu WHERE idSequencial = {0} ", cMenu);
                campos = new List<string>(new string[] { "MENU", "DESCRICAO" });
                list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["idSequencial"].Count <= 0))
                {
                    LogFile.Log(String.Format("Campos_Browse_HTML: Problema para obter os dados do Menu! Menu: {0}", cMenu));
                    LogFile.Log(cQuery);
                    break;
                }

                string cCodMenu = list["MENU"].First();

                cTrace2 = list["DESCRICAO"].First();

                //--------------- Contém Status

                cQuery = String.Format("SELECT Count(*) AS TEMSTATUS FROM aa41campos WHERE id0aa40tabelas = {0} AND CAMPO='STATUS' ", cTabela);
                campos = new List<string>(new string[] { "TEMSTATUS" });
                list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["TEMSTATUS"].Count <= 0))
                {
                    LogFile.Log(String.Format("Campos_Browse_HTML: Problema para verificar se a Tabela tem Status! Tabela: {0}", cTabela));
                    LogFile.Log(cQuery);
                    break;
                }

                Boolean lTemStatus = (Int32.Parse(list["TEMSTATUS"].First()) > 0);

                //--------------- Campos da Tabela

                cQuery = String.Format("SELECT * FROM aa41campos WHERE id0aa40tabelas = {0} AND BROWSESN='S' AND OPERACAO <> 'I' ORDER BY ORDEM ", cTabela);
                campos = new List<string>(new string[] { "idSequencial", "CAMPO", "BROWSETIT", "TITULO", "OPCOES" });
                MultiValueDictionary<string, string> lsCampos = MeuDB.Select(cQuery, campos);

                if ((lsCampos.Count < campos.Count) || (lsCampos["idSequencial"].Count <= 0))
                {
                    LogFile.Log(String.Format("Campos_Browse_HTML: Problema para obter os dados do Menu! Menu: {0}", cMenu));
                    LogFile.Log(cQuery);
                    break;
                }

                //Pode ser passado o filtro por parametro com (cFiltrarSN = 'I')
                if (String.IsNullOrEmpty(cFiltro) || (cFiltrarSN != "I")) { cFiltro = ""; }

                if (cFiltrarSN == "S")
                {
                    //Parametro de:
                    cFiltro += String.Format(" WHERE {0} >= {1} ", "idSequencial", oLogin["XSEQUENCIAL1"]);
                    //Parametro ate:
                    cFiltro += String.Format("   AND {0} <= {1} ", "idSequencial", oLogin["XSEQUENCIAL2"]);
                }

                //Obtem os campos da tabela

                MultiValueDictionary<string, string> aCampos = new MultiValueDictionary<string, string>();
                Boolean lCmpRetirar = false;

                for (int x = 0; x < lsCampos.Count; x++)
                {
                    string cParametro = "";
                    string cBrowseTit = lsCampos["BROWSETIT"].ElementAt(x);

                    cBrowseTit = (String.IsNullOrEmpty(cBrowseTit) ? lsCampos["TITULO"].ElementAt(x) : cBrowseTit);

                    aCampos.Add("TITULO", cBrowseTit);
                    aCampos.Add("CAMPO", lsCampos["CAMPO"].ElementAt(x));
                    aCampos.Add("OPCOES", lsCampos["OPCOES"].ElementAt(x));
                    aCampos.Add("idSequencial", lsCampos["idSequencial"].ElementAt(x));

                    lCmpRetirar = (lCmpRetirar || (lsCampos["CAMPO"].ElementAt(x) == "RETIRAR"));

                    if (cFiltrarSN == "S")
                    {
                        if (lsCampos["FILTRO"].ElementAt(x) == "2")
                        {
                            //Parametro de:
                            cParametro = String.Format("X{0}1", lsCampos["CAMPO"].ElementAt(x));
                            cFiltro += String.Format(" AND {0} >= '{1}' ", lsCampos["CAMPO"].ElementAt(x), oLogin[cParametro]);
                            //Parametro ate:
                            cParametro = String.Format("X{0}2", lsCampos["CAMPO"].ElementAt(x));
                            cFiltro += String.Format(" AND {0} <= '{1}' ", lsCampos["CAMPO"].ElementAt(x), oLogin[cParametro]);
                        }
                        else if (lsCampos["FILTRO"].ElementAt(x) == "3")
                        {
                            //Parametro parcial:
                            cParametro = String.Format("X{0}1", lsCampos["CAMPO"].ElementAt(x));
                            cFiltro += String.Format(" AND {0} LIKE '%{1}%' ", lsCampos["CAMPO"].ElementAt(x), oLogin[cParametro]);
                        }
                        else if (lsCampos["FILTRO"].ElementAt(x) == "4")
                        {
                            //Parametro parcial:
                            cParametro = String.Format("X{0}1", lsCampos["CAMPO"].ElementAt(x));
                            cFiltro += String.Format(" AND {0} = '%{1}%' ", lsCampos["CAMPO"].ElementAt(x), oLogin[cParametro]);
                        }

                    } //if (cFiltrar)

                } //for(x)

                cFiltro = MeuLib.Base64Encode(cFiltro);

                //--------------- Opções de Menu

                cQuery = "SELECT * FROM aa30menu a30 ";
                if (cPermissao != "1") { cQuery += ", aa11permissao a11 "; }
                cQuery += String.Format(" WHERE (MENU LIKE '{0}%') AND (TIPO >= '1') AND (TIPO <= '9') AND (STATUS = 'A') ", cCodMenu);
                if (cPermissao != "1") { cQuery += String.Format(" AND a30.idSequencial = a11.id0aa30menu AND a11.id0aa10usuarios = {0} ", cIdUsuario); }
                cQuery += " ORDER BY REPLACE(MENU, 'IN', '0IN') ";

                campos = new List<string>(new string[] { "MENU", "DESCRICAO", "TIPO", "IMAGEM", "PAGINA" });
                list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["MENU"].Count <= 0))
                {
                    LogFile.Log(String.Format("Campos_Browse_HTML: Problema para obter os dados do Menu! Menu: {0}", cCodMenu));
                    LogFile.Log(cQuery);
                    break;
                }

                MultiValueDictionary<string, string> aOpcoes = new MultiValueDictionary<string, string>();

                for (int x = 0; x < list.Count; x++)
                {
                    string cOpcao = list["MENU"].ElementAt(x);
                    string cImagem = "";

                    if (cOpcao.Length != (cCodMenu.Length + 2)) { continue; }

                    if ((cCodMenu + "IN") == cOpcao) { cImagem = "portal_mais.gif"; }
                    else if ((cCodMenu + "AL") == cOpcao) { cImagem = "portal_edit.gif"; }
                    else if ((cCodMenu + "EX") == cOpcao)
                    {
                        if (lCmpRetirar) { continue; } //Ignora opção excluir para a tabela que tem o campo RETIRAR
                        cImagem = "portal_menos.gif";
                    }
                    else  //Opções específicas da tabela
                    {
                        cImagem = list["IMAGEM"].ElementAt(x);
                    }

                    string cPagina = list["PAGINA"].ElementAt(x);

                    if (String.IsNullOrEmpty(cPagina)) { cPagina = "tabelas_cadastro"; }

                    aOpcoes.Add("MENU", cOpcao);
                    aOpcoes.Add("DESCRICAO", list["DESCRICAO"].ElementAt(x));
                    aOpcoes.Add("TIPO", list["TIPO"].ElementAt(x));
                    aOpcoes.Add("IMAGEM", cImagem);
                    aOpcoes.Add("PAGINA", cPagina);

                } //for(x)

                //--------------- Opções de Menu

                LogFile.Log("cTabela........: " + cTabela);

                cCabecalho = "";
                cStrLinha = "";

                if (lTemStatus) { cCabecalho += "<td style='margin-right: 1mm; margin-left: 1mm;'><strong>Sts</strong></td>"; }

                string cLinha = String.Format("<tbody data-bind='foreach: xbrowse{0}'>", cTabela);

                cLinha += "<tr data-bind='attr: { id: TR_ID }'>";

                cLinha += "   <td>";
                cLinha += "  	<a name='2' href data-bind=\"click: $root.manutencao, attr: {title : 'tabelas_cadastro'}\"><img src='/imagens/portal_visualizar.gif' border='0' title='Visualizar'></a>&nbsp;";

                for (int x = 0; x < aOpcoes.Count; x++)
                {
                    cLinha += String.Format("&nbsp;  <a name='{0}' href data-bind=\"click: $root.manutencao, attr: {"+"title : '{1}'} \"><img src='/imagens/{2}' width=18px height=16px border='0' title='{3}'></a>",
                                             aOpcoes["TIPO"].ElementAt(x), aOpcoes["PAGINA"].ElementAt(x), aOpcoes["IMAGEM"].ElementAt(x), aOpcoes["DESCRICAO"].ElementAt(x));
                }

                cLinha += "   </td>";

                if (lTemStatus)
                {
                    //cLinha += format('<td data-bind="template: { name: $root.displayStatus, foreach: $root.statuss%s }"></td>',[cTabela]);
                    cLinha += String.Format("<td data-bind=\"template: { "+"name: 'statusTemplate1', data: IMGSTATUS }\"></td>",cTabela);
                }

                for (int x = 0; x < aCampos.Count; x++)
                {
                    if (String.IsNullOrEmpty(cStrLinha))
                    {
                        cCabecalho += String.Format("<td style='margin-right: 1mm; margin-left: 1mm;'><strong>{0}</strong></td>", aCampos["TITULO"].ElementAt(x));
                    }

                    cLinha += String.Format("<td data-bind='text: {0}' style='margin-left:1mm; margin-right:1mm; font-weight:bold;'></td>", aCampos["CAMPO"].ElementAt(x));

                } //for(x)

                cLinha += "   <td data-bind='text: idSequencial' style='margin-left:1mm; margin-right:10px;text-align:right; font-weight:bold;'></td>";
                cLinha += "</tr>";
                cLinha += "</tbody>";

                cStrLinha += cLinha;

            } while (false);

            return cHtml;

        }
    }
}
