using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recursos
{
    public class ArtLib
    {
        public int nTPagina = 20;  //Total de registros por pagina no browser
        
        string cAlfabeto = " 1234567890ABCDEFGHIJKLMNOPQRSTUVXZWYabcdefghijklmnopqrstuvxzwy -_=/?\\|*+.,;:!@#$%&()[]{}<>~^\"ÁÉÍÓÚáéíóúÂÊÔâêôÃÕãõÜüÇç©³¡'+#13+#10+«»";
        string cAlfHTML = " -_=/?\\|*+.,;:!@#$%&()[]{}<>~^\"1234567890ABCDEFGHIJKLMNOPQRSTUVXZWYabcdefghijklmnopqrstuvxzwy";
        string cDecimal = "0123456789";

        public string Cobrir(string cCodigo)
        {
            string cL = "";
            string cS = "";
            char cP;
            int nPos = 0;

            for (int i = 0; i < cCodigo.Length; i++)
            {
                cL = cCodigo.Substring(i, 1);

                nPos = cAlfabeto.IndexOf(cL);

                if (nPos < 0)
                {
                    continue;
                }

                nPos += 71;
                cP = (char)nPos;

                int nTam = cS.Length;
                if (nTam > 9) { nTam = 9; }

                nPos = -1;

                if (cS.Substring(0, nTam).Contains(cP))
                {
                    nPos = cS.Substring(0, nTam).IndexOf(cP);
                }

                if (nPos >= 0)
                {
                    cS += nPos.ToString();

                }
                else
                {
                    cS += cP;
                }

            }

            return cS;
        }

        public string DesCobrir(string cCodigo)
        {
            string cL = "";
            string cS = "";
            char cP;
            int nPos = 0;

            for (int i = 0; i < cCodigo.Length; i++)
            {
                cL = cCodigo.Substring(i, 1);

                if (cDecimal.Contains(cL))
                {
                    nPos = Int32.Parse(cL);
                    cS += cS.Substring(nPos, 1);
                }
                else
                {
                    cP = cCodigo[i];
                    nPos = (int)cP - 71;
                    cS += cAlfabeto.Substring(nPos, 1);
                }
            }

            return cS;
        }

        public string CobrirHTML(string cCodigo)
        {
            string cL = "";
            string cS = "";
            int nPos = 0;
            char cP;

            for (int i = 0; i < cCodigo.Length; i++)
            {
                cL = cCodigo.Substring(i, 1);

                nPos = cAlfHTML.IndexOf(cL);

                if (nPos < 0)
                {
                    LogFile.Log("CobrirHTML: Caractere nao encontrado! char: " + cL);
                    continue;
                }

                nPos += 70;
                cP = (char)nPos;

                int nTam = cS.Length;
                if (nTam > 9) { nTam = 9; }

                nPos = -1;

                if (cS.Substring(0, nTam).Contains(cP))
                {
                    nPos = cS.Substring(0, nTam).IndexOf(cP);
                }

                if (nPos >= 0)
                {
                    cS += nPos.ToString();

                }
                else
                {
                    cS += cP;
                }

            }

            return cS;
        }

        public string DesCobrirHTML(string cCodigo)
        {
            string cL = "";
            string cS = "";
            int nPos = 0;
            char cP;

            for (int i = 0; i < cCodigo.Length; i++)
            {
                cL = cCodigo.Substring(i, 1);

                nPos = cDecimal.IndexOf(cL);

                if (nPos >= 0)
                {
                    cS += cS.Substring(nPos, 1);
                }
                else
                {
                    cP = cCodigo[i];
                    nPos = (int)cP - 70;
                    cS += cAlfHTML.Substring(nPos, 1);
                }
            }

            return cS;
        }

        public string HTMLAcento(string cCod)
        {
            string cAcento = "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸƒ";
            string[] aHTMLAcento = {"192", "193", "194", "195", "196", "197", "198", "199",
                                    "200", "201", "202", "203", "204", "205", "206", "207",
                                    "208", "209", "210", "211", "212", "213", "214", "216",
                                    "217", "218", "219", "220", "221", "222", "223", "224",
                                    "225", "226", "227", "228", "229", "230", "231", "232",
                                    "233", "234", "235", "236", "237", "238", "239", "240",
                                    "241", "242", "243", "244", "245", "246", "248", "249",
                                    "250", "251", "252", "253", "254", "255", "338", "339",
                                    "352", "353", "376", "402"};
            string cL = "";
            string cS = "";
            int nPos = 0;

            for (int i = 0; i < cCod.Length; i++)
            {
                cL = cCod.Substring(i, 1);
                nPos = cAcento.IndexOf(cL);

                if (nPos >= 0)
                {
                    cS += "&#" + aHTMLAcento[nPos] + ";";
                }
                else
                {
                    cS += cL;
                }

            }

            return cS;

        }

        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
