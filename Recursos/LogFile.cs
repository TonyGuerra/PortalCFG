using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Recursos
{
    public class LogFile
    {
        private static StreamWriter log;

        public static void OpenLog()
        {
            string path = @".\log";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (!File.Exists("./log/logfile.txt"))
            {
                var file = new FileStream(@".\log\logfile.txt", FileMode.Create);
                log = new StreamWriter(file);
            }
            else
            {
                var file = new FileStream(@".\log\logfile.txt", FileMode.Append);
                log = new StreamWriter(file);
            }

        }

        public static void Log(string cLog)
        {
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    OpenLog();

                    //cLog = string.Format("{0} - {1}", cLog, DateTime.Now.ToString("dd-MM-yyyy h:mm:ss tt"));

                    log.WriteLine(cLog);

                    Console.WriteLine(cLog);

                    CloseLog();

                    break;

                }
                catch
                {
                    Console.WriteLine(" *** " + cLog);
                }
            }
        }

        public static void CloseLog()
        {
            log.Close();
        }

    }
}
