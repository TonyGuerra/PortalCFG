using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recursos
{
    class ArtLib
    {
        string cAlfabeto = " 1234567890ABCDEFGHIJKLMNOPQRSTUVXZWYabcdefghijklmnopqrstuvxzwy -_=/?\\|*+.,;:!@#$%&()[]{}<>~^\"ÁÉÍÓÚáéíóúÂÊÔâêôÃÕãõÜüÇç©³¡'+#13+#10+«»";
        string cAlfHTML  = " -_=/?\\|*+.,;:!@#$%&()[]{}<>~^\"1234567890ABCDEFGHIJKLMNOPQRSTUVXZWYabcdefghijklmnopqrstuvxzwy";
        string cDecimal  = "0123456789";
        //string cNumeros  = "123456789";

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

                if (cS.Substring(0,nTam).Contains(cP))
                {
                    nPos = cS.Substring(0, nTam).IndexOf(cP);
                }

                if  (nPos >= 0)
                {
                    cS += nPos.ToString();

                } else
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
                    cS += cS.Substring(nPos,1);
                } else
                {
                    cP = cCodigo[i];
                    nPos = (int)cP - 70;
                    cS += cAlfHTML.Substring(nPos, 1);
                }
            }

            return cS;
        }
    }
}
