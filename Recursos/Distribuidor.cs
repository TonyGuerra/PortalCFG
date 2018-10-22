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
                    cDados = HttpUtility.UrlDecode(cDados.Replace("\0", ""));
                }

                if (cMeuPath.Contains(".js"))
                {
                    LogFile.Log(" .");
                    cMeuPath = cMeuPath.Replace(".", "_");

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
                    cHtml = Autentica_Login(request, MeuDB, MeuLib, cDados);
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

                string cQuery = "SELECT USUARIO, SENHA, VALIDADE FROM aa10usuarios WHERE USUARIO = '" + oLogin["login"] + "'";
                List<string> campos = new List<string>(new string[] { "USUARIO", "SENHA", "VALIDADE" });

                List<string>[] list = MeuDB.Select(cQuery, campos);

                if (list.Length < 3)
                {
                    cHtml = string.Format("ERRO: Usuario nao encontrado! {0:d} {0:t}", DateTime.Now);
                    break;
                }

                string cSenha = list[1][0];
                DateTime dDataValida = DateTime.ParseExact(list[2][0].Substring(0, 10), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

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

        private static string Autentica_Login(HttpListenerRequest request, DBConnect MeuDB, ArtLib MeuLib, string cDados)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            dynamic oLogin = serializer.Deserialize<dynamic>(cDados.Replace("dados=", ""));
            string cHtml = "ERRO: Html nao atribuido";

            LogFile.Log(" --- Autentica_Login:");

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
                    break;
                }

                cHtml = Resources.ResourceManager.GetObject("autentica_login").ToString();

                cHtml = cHtml.Replace("!XLOGIN!" , oLogin["login"] );
                cHtml = cHtml.Replace("!XSESSAO!", oLogin["sessao"]);

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

        private static string Generico(HttpListenerRequest request, DBConnect MeuDB, ArtLib MeuLib, string cMeuPath, string cDados)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            dynamic oLogin = serializer.Deserialize<dynamic>(cDados.Replace("dados=", ""));
            string cHtml = "ERRO: Html nao atribuido";
            string cQueryString = HttpUtility.UrlDecode(request.Url.Query.Substring(1));

            LogFile.Log(" --- Generico:");

            if (!String.IsNullOrEmpty(cQueryString))
            {
                oLogin = serializer.Deserialize<dynamic>(cQueryString);
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
                    break;
                }

                cHtml = Resources.ResourceManager.GetObject(cMeuPath).ToString();

                cHtml = cHtml.Replace("!XLOGIN!", oLogin["login"]);
                cHtml = cHtml.Replace("!XSESSAO!", oLogin["sessao"]);

            } while (false);

            LogFile.Log(" --- Fim Generico!");

            return cHtml;
        }

        private static bool Check_Sessao(HttpListenerRequest request, DBConnect MeuDB, ArtLib MeuLib, string cLogin, string cSessao)
        {
            bool lOk = false;

            do
            {
                LogFile.Log(" --- Cookies: ");
                if  (request.Cookies.Count <= 0)
                {
                    LogFile.Log(" sem Cookies!");
                } else
                {
                    LogFile.Log(request.Cookies[0].Name + " - " + request.Cookies[0].Value);
                    LogFile.Log(request.Cookies[1].Name + " - " + request.Cookies[1].Value);
                }

                string cQuery = "SELECT SESSAO, PERMISSAO, TEMPO FROM aa20sessao WHERE USUARIO = '" + cLogin + "'";
                List<string> campos = new List<string>(new string[] { "SESSAO", "PERMISSAO", "TEMPO" });

                List<string>[] list = MeuDB.Select(cQuery, campos);

                if (list.Length < 3)
                {
                    LogFile.Log(string.Format("Check_Sessao: Sessao do usuario nao encontrado! Login: {0}", cLogin));
                    break;
                }

                if (list[0][0] != cSessao)
                {
                    LogFile.Log(string.Format("Check_Sessao: Sessao invalida! Login: {0}", cLogin));
                    break;
                }

                if (list[1][0] != "1")
                {
                    LogFile.Log(string.Format("Check_Sessao: Sem permissao! Login: {0}", cLogin));
                    break;
                }

                DateTime dTempo = DateTime.ParseExact(list[2][0], "G", null);

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
    }
}
