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
    public class Distribuidor
    {
        public static string Pagina(HttpListenerRequest request, string cDataBase, string cRaiz)
        {
            DBConnect MeuDB = new DBConnect(cDataBase);
            ArtLib MeuLib = new ArtLib();
            Browser MeuBrowser = new Browser();
            string cHtml = "ERRO: Html nao atribuido";
            string cDados = "";

            char[] charSeparators = new char[] { '/' };
            string[] MeuPath = request.Url.LocalPath.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            string cMeuPath = MeuPath[MeuPath.Length - 1];

            LogFile.Log(MeuPath[MeuPath.Length-1]);

            LogFile.Log(string.Format(" --- Distribuidor - {0}", DateTime.Now.ToString("dd-MM-yyyy h:mm:ss tt")));

            do
            {
                if (request.HttpMethod == "POST")
                {
                    int nTam = 1024;
                    byte[] formData = new byte[nTam];
                    request.InputStream.Read(formData, 0, nTam);
                    cDados = Encoding.ASCII.GetString(formData);
                    if  ((cDados.IndexOf("dados=") >= 0) && (cDados.IndexOf("dados=") <= 1))
                    {
                        cDados = HttpUtility.UrlDecode(cDados.Replace("\0", ""));
                    }
                    else
                    {
                        cDados =cDados.Replace("\0", "");
                    }
                    
                }

                if (cMeuPath.Contains(".css"))
                {
                    LogFile.Log(" .");
                    cMeuPath = cMeuPath.Replace(".", "_");

                    if (Resource_CSS.ResourceManager.GetObject(cMeuPath) == null)
                    {
                        LogFile.Log(" CSS nao encontrado! - " + cMeuPath);
                        break;
                    }

                    cHtml = Resource_CSS.ResourceManager.GetString(cMeuPath).ToString();

                    if (cHtml.Length <= 0)
                    {
                        LogFile.Log(" CSS nao encontrado! - " + cMeuPath);
                        break;
                    }
                }
                else if (cMeuPath.Contains(".js"))
                {
                    LogFile.Log(" .");
                    cMeuPath = cMeuPath.Replace(".", "_");
                    cMeuPath = cMeuPath.Replace("-", "_");

                    if (Resource_JS.ResourceManager.GetObject(cMeuPath) == null)
                    {
                        LogFile.Log(" JavaScript nao encontrado! - " + cMeuPath);
                        break;
                    }

                    cHtml = Resource_JS.ResourceManager.GetString(cMeuPath).ToString();

                    if (cHtml.Length <= 0)
                    {
                        LogFile.Log(" JavaScript nao encontrado! - " + cMeuPath);
                        break;
                    }
                }
                else if (request.Url.LocalPath.Contains("/login"))
                {
                    LogFile.Log(" .");

                    cHtml = Resources.ResourceManager.GetObject("Login").ToString();

                    LogFile.Log(" .");
                }
                else if ((cMeuPath == "valida_login") && !(String.IsNullOrEmpty(cDados)))
                {
                    cHtml = Valida_Login(request, MeuDB, MeuLib, cDados);
                }
                else if ((cMeuPath == "autentica_login") && !(String.IsNullOrEmpty(cDados)))
                {
                    cHtml = Autentica_Login(request, MeuDB, MeuLib, cMeuPath, cDados);
                }
                else if (cMeuPath == "menutree_esquerdo")
                {
                    cHtml = Menutree_Esquerdo(request, MeuDB, MeuLib, cMeuPath, cDados);
                }
                else if (cMeuPath == "tabelas_filtro")
                {
                    cHtml = MeuBrowser.Tabelas_Filtro(request, MeuDB, MeuLib, cMeuPath, cDados);
                }
                else if (cMeuPath == "pagina_browser")
                {
                    cHtml = MeuBrowser.Pagina_Browser(request, MeuDB, MeuLib, cMeuPath, cDados);
                }
                else if (cMeuPath == "obter_browser")
                {
                    cHtml = MeuBrowser.Obter_Browser(request, MeuDB, MeuLib, cMeuPath, cDados);
                }
                else if (Resources.ResourceManager.GetObject(cMeuPath) != null)
                {
                    cHtml = Generico(request, MeuDB, MeuLib, cMeuPath, cDados);
                }
                else
                {
                    cHtml = string.Format("ERRO: Solicitacao nao atendida! {0}", DateTime.Now);
                }

            } while (false);

            return cHtml;

        }

        private static string Valida_Login(HttpListenerRequest request, DBConnect MeuDB, ArtLib MeuLib, string cDados)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            dynamic oLogin = serializer.Deserialize<dynamic>(cDados);
            string cHtml = "ERRO: Html nao atribuido";

            LogFile.Log(" --- Valida_login:");

            do
            {
                if (String.IsNullOrEmpty(oLogin["login"]))
                {
                    cHtml = string.Format("ERRO: Login deve ser informado! {0}", DateTime.Now);
                    break;
                }

                if (String.IsNullOrEmpty(oLogin["senha"]))
                {
                    cHtml = string.Format("ERRO: Senha deve ser informada! {0}", DateTime.Now);
                    break;
                }

                string cQuery = "SELECT * FROM aa10usuarios WHERE USUARIO = '" + oLogin["login"] + "'";
                List<string> campos = new List<string>(new string[] { "USUARIO", "SENHA", "VALIDADE" });

                MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                if (list.Count < campos.Count)
                {
                    cHtml = string.Format("ERRO: Usuario nao encontrado! {0:d} {0:t}", DateTime.Now);
                    break;
                }

                string cSenha = list["SENHA"].First();
                DateTime dDataValida = DateTime.ParseExact(list["VALIDADE"].First().Substring(0, 10), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                if (dDataValida < DateTime.Now)
                {
                    cHtml = string.Format("ERRO: Usuario com validade vencida! {0}", dDataValida);
                    break;
                }

                if (oLogin["senha"] != MeuLib.DesCobrir(cSenha))
                {
                    cHtml = "ERRO: Senha nao confere!";
                    break;
                }

                string cSessao = MeuLib.CobrirHTML(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                //Limpa Sessões do usuário
                cQuery = "DELETE from aa20sessao where USUARIO = '" + oLogin["login"] + "'";
                MeuDB.Delete(cQuery);

                string cTempo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                //Abrir Sessao
                cQuery = "INSERT INTO aa20sessao (USUARIO, SESSAO, TEMPO) VALUES ('" + oLogin["login"] + "','" + cSessao + "','" + cTempo + "')";
                int nRegs = MeuDB.Insert(cQuery);

                if (nRegs <= 0)
                {
                    cHtml = "ERRO: Problema para registrar a Sessao!";
                    break;
                }

                //responde com a sessao criada
                cHtml = cSessao;

                LogFile.Log(".");
                LogFile.Log("Usuario logado.: " + oLogin["login"] + " - tempo: " + cTempo);
                LogFile.Log(".");

            } while (false);

            LogFile.Log(" --- Fim Valida_login!");

            return cHtml;

        }

        private static string Autentica_Login(HttpListenerRequest request, DBConnect MeuDB, ArtLib MeuLib, string cMeuPath, string cDados)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string cQueryString = HttpUtility.UrlDecode(request.Url.Query);
            dynamic oLogin = serializer.Deserialize<dynamic>(cDados.Replace("dados=", ""));
            string cHtml = "ERRO: Html nao atribuido";

            LogFile.Log(" --- Autentica_Login:");

            if (!String.IsNullOrEmpty(cQueryString))
            {
                int nP = (cQueryString.Contains("dados=") ? 7 : 1);
                oLogin = serializer.Deserialize<dynamic>(cQueryString.Substring(nP));
            }

            do
            {

                cHtml = Generico(request, MeuDB, MeuLib, cMeuPath, cDados);

                if (cHtml.Contains("Sessao Expirou")) { break; }

                //Registrar os cookies
                Cookie MeuCookie = new Cookie();
                MeuCookie.Name = "login";
                MeuCookie.Value = oLogin["login"];

                request.Cookies.Add(MeuCookie);

                MeuCookie.Name = "sessao";
                MeuCookie.Value = oLogin["sessao"];

                request.Cookies.Add(MeuCookie);

            } while (false);

            LogFile.Log(" --- Fim Autentica_Login!");

            return cHtml;
        }

        private static string Menutree_Esquerdo(HttpListenerRequest request, DBConnect MeuDB, ArtLib MeuLib, string cMeuPath, string cDados)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string cQueryString = HttpUtility.UrlDecode(request.Url.Query);
            dynamic oLogin = serializer.Deserialize<dynamic>(cDados.Replace("dados=", ""));
            string cHtml = "ERRO: Html nao atribuido";

            LogFile.Log(" --- Menutree_Esquerdo:");

            if (!String.IsNullOrEmpty(cQueryString))
            {
                int nP = (cQueryString.Contains("dados=") ? 7 : 1);
                oLogin = serializer.Deserialize<dynamic>(cQueryString.Substring(nP));
            }

            do
            {

                cHtml = Generico(request, MeuDB, MeuLib, cMeuPath, cDados);

                if  (cHtml.Contains("Sessao Expirou")) { break; }

                //Registrar os cookies
                Cookie MeuCookie = new Cookie();
                MeuCookie.Name = "login";
                MeuCookie.Value = oLogin["login"];
                request.Cookies.Add(MeuCookie);
                MeuCookie.Name = "sessao";
                MeuCookie.Value = oLogin["sessao"];
                request.Cookies.Add(MeuCookie);

                // Dados do Usuario
                string cQuery = "SELECT * FROM aa10usuarios a10 Where a10.USUARIO = '" + oLogin["login"] + "'";
                List<string> campos = new List<string>(new string[] { "idSequencial", "PERMISSAO" });

                MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                if (list.Count < campos.Count)
                {
                    cHtml = string.Format("ERRO: Usuario nao encontrado! {0:d} {0:t}", DateTime.Now);
                    break;
                }

                string cIdUsuario = list["idSequencial"].First();
                string cPermissao = list["PERMISSAO"].First();

                //Opções de Menu do usuário
                campos = new List<string>(new string[] { "idSequencial", "MENU", "DESCRICAO", "TIPO", "PAGINA", "STATUS" });
                cQuery = "SELECT * FROM aa30menu a30 ";

                if  (cPermissao != "1")
                {
                    cQuery += ", aa11permissao a11 ";
                }

                cQuery += " Where a30.STATUS = 'A' AND a30.TIPO IN ('M','H') ";

                if (cPermissao != "1")
                {
                    cQuery += " AND a30.idSequencial = a11.id0aa30menu AND a11.id0aa10usuarios = " + cIdUsuario;
                }

                cQuery += " ORDER BY MENU ";

                list = MeuDB.Select(cQuery, campos);

                if (list.Count < campos.Count)
                {
                    cHtml = string.Format("ERRO: Usuario nao encontrado! {0:d} {0:t}", DateTime.Now);
                    break;
                }

                string cMeuMenu   = "";
                string cMenu      = "M";
                string cParametros = "{\"login\":\"" + oLogin["login"] + "\",\"sessao\":\"" + oLogin["sessao"] + "\",\"menu\":\"xxx\"}";

                for (int i = 0; i < list["idSequencial"].Count; i++)
                {
                    MenuNivel(MeuLib, list, cMenu, 1, ref i, ref cMeuMenu, cParametros);
                }

                cHtml = cHtml.Replace("!XMENUTROCA!", cMeuMenu);

            } while (false);

            LogFile.Log(" --- Fim Menutree_Esquerdo!");

            return cHtml;
        }

        private static void MenuNivel(ArtLib MeuLib, MultiValueDictionary<string, string> list, string cMenu, int nNivel, ref int i, ref string cMeuMenu, string cParametros)
        {
            string cMenuNivel = "";
            string cDescricao = "";
            string cRotina    = "";

            do
            {
                cMenuNivel = list["MENU"].ElementAt(i).Trim();

                if ((list["STATUS"].ElementAt(i) != "A") || (cMenuNivel.Length > (nNivel+2)))
                {
                    i++;
                    continue;
                }

                cDescricao = MeuLib.HTMLAcento(list["DESCRICAO"].ElementAt(i).Trim());

                if (list["TIPO"].ElementAt(i) == "M")
                {
                    cMeuMenu += "<li class='dcjq-current-parent'><a href='#'>" + cDescricao + "</a>";
                    cMeuMenu += "<ul>";

                    i++;
                    MenuNivel(MeuLib, list, cMenuNivel, (nNivel + 2), ref i, ref cMeuMenu, cParametros);

                    cMeuMenu += "</ul>";
                    cMeuMenu += "</li>";
                }
                else if (list["TIPO"].ElementAt(i) == "H")
                {
                    cRotina = list["PAGINA"].ElementAt(i).Trim(); 
                    if (!String.IsNullOrEmpty(cRotina)) { cRotina = cRotina + "?" + MeuLib.Base64Encode(cParametros.Replace("xxx", list["idSequencial"].ElementAt(i))); } //id Menu
                    cMeuMenu += "   <li><a href=\"javascript:fPagina('" + cRotina + "')\">" + cDescricao + "</a></li>";
                }

                i++;

            } while ((list["MENU"].Count() > i) && (list["MENU"].ElementAt(i).Substring(0, nNivel) == cMenu));

            i--;
        }

        public static string Generico(HttpListenerRequest request, DBConnect MeuDB, ArtLib MeuLib, string cMeuPath, string cDados)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string cHtml = "ERRO: Html nao atribuido";
            string cQueryString = HttpUtility.UrlDecode(request.Url.Query);
            dynamic oLogin = serializer.Deserialize<dynamic>(cDados.Replace("dados=", ""));

            LogFile.Log(" --- Generico:");


            if (!String.IsNullOrEmpty(cQueryString))
            {
                int nP = (cQueryString.Contains("dados=") ? 7 : 1 );
                string cJSon = cQueryString.Substring(nP);
                if (!cJSon.Contains("{")) { cJSon = MeuLib.Base64Decode(cJSon); }
                oLogin = serializer.Deserialize<dynamic>(cJSon);
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

                if (!Check_Sessao(request, MeuDB, MeuLib, oLogin["login"], oLogin["sessao"]))
                {
                    cHtml = S_mensagem("Sessão expirou! Logue novamente!", "javascript:fLogin()");
                    break;
                }

                cHtml = Resources.ResourceManager.GetObject(cMeuPath).ToString();

                cHtml = cHtml.Replace("!XLOGIN!", oLogin["login"]);
                cHtml = cHtml.Replace("!XSESSAO!", oLogin["sessao"]);

            } while (false);

            LogFile.Log(" --- Fim Generico!");

            return cHtml;
        }

        public static bool Check_Sessao(HttpListenerRequest request, DBConnect MeuDB, ArtLib MeuLib, string cLogin, string cSessao)
        {
            bool lOk = false;

            do
            {
                LogFile.Log(" --- Cookies: ");
                if  (request.Cookies.Count < 2)
                {
                    LogFile.Log(" sem Cookies!");
                } else
                {
                    LogFile.Log(request.Cookies[0].Name + " - " + request.Cookies[0].Value);
                    LogFile.Log(request.Cookies[1].Name + " - " + request.Cookies[1].Value);
                }

                string cQuery = "SELECT * FROM aa20sessao WHERE USUARIO = '" + cLogin + "'";
                List<string> campos = new List<string>(new string[] { "SESSAO", "PERMISSAO", "TEMPO" });

                MultiValueDictionary<string, string> list = MeuDB.Select(cQuery, campos);

                if ((list.Count < campos.Count) || (list["SESSAO"].Count <= 0))
                {
                    LogFile.Log(string.Format("Check_Sessao: Sessao do usuario nao encontrado! Login: {0}", cLogin));
                    break;
                }

                if (list["SESSAO"].First() != cSessao)
                {
                    LogFile.Log(string.Format("Check_Sessao: Sessao invalida! Login: {0}", cLogin));
                    break;
                }

                if (list["PERMISSAO"].First() != "1")
                {
                    LogFile.Log(string.Format("Check_Sessao: Sem permissao! Login: {0}", cLogin));
                    break;
                }

                DateTime dTempo = DateTime.ParseExact(list["TEMPO"].First(), "G", null);

                var diff = DateTime.Now.Subtract(dTempo);

                LogFile.Log("Diferenca de tempo: " + String.Format("{0}:{1}:{2}", diff.Hours, diff.Minutes, diff.Seconds));

                if ((diff.Hours > 0) || (diff.Minutes > 5))
                {
                    LogFile.Log(string.Format("Check_Sessao: Sessao expirou! Login: {0}", cLogin));
                    break;
                }

                // --- Atualiza o tempo da sessao
                //Limpa Sessões do usuário
                cQuery = "UPDATE aa20sessao SET TEMPO = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                                                ", IP = '" + request.RemoteEndPoint.ToString() + "'" +
                         " where USUARIO = '" + cLogin + "'";
                MeuDB.Update(cQuery);

                lOk = true;

            } while (false);

            return lOk;
        }

       public static string S_mensagem(string cMensagem, string onclick)
        {
            ArtLib MeuLib = new ArtLib();

            string mensagem  = "<html>" +
                               " <head>" +
                               " <title>Sessao Expirou</title>" +
                               " <script type='text/JavaScript'> " +
                               " function fLogin() {" +
                               "     top.window.location='./login';" +
                               " } " +
                               " </script>" +
                               " </head>" +
                               " <body bgcolor='#BEBEBE'>" +
                               " <br><br><br><br><br><br><br><br><br><br><br>" +
                               " <form name='form1' method='post' action='" + onclick + "'>" +
                               " <center>" + cMensagem + "</center>" +
                               " <br><br>" +
                               " <center><input type='submit' name='voltar' value='Voltar'></center>" +
                               " </form>" +
                               " <SCRIPT LANGUAGE='JavaScript'> " +
                               "    parent.Frame1.hmheader.meutd1.innerHTML=''; " +
                               "    parent.Frame1.hmheader.meutd2.innerHTML=''; " +
                               "    parent.Frame1.hmheader.meutd3.innerHTML=''; " +
                               " </SCRIPT> " +
                               " </body> " +
                               "</html> ";

            return MeuLib.HTMLAcento(mensagem);
        }

    }
}
