using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.IO;

namespace Recursos
{
    public class DBConnect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Constructor
        public  DBConnect(string cDataBase)
        {
            Initialize(cDataBase);
        }

        //Initialize values
        private void Initialize(string cDataBase)
        {
            server   = "127.0.0.1";
            database = cDataBase;
            uid      = "admin";
            password = "admin01";
            string connectionString = "Server=" + server + ";Port=3306;" + "Database=" +database + ";" + 
                                      "Uid=" + uid + ";" + "Pwd=" + password + ";SslMode=none;";

            connection = new MySqlConnection(connectionString);
        }

        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        LogFile.Log("Problema para conectar com o servidor.  Contate o administrador!");
                        LogFile.Log(ex.Message);
                        break;

                    case 1045:
                        LogFile.Log("Invalido usuario/senha, tente novamente!");
                        LogFile.Log(ex.Message);
                        break;
                }
                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                LogFile.Log(ex.Message);
                return false;
            }
        }

        //Insert statement
        public int Insert(string query)
        {
            //string query = "INSERT INTO tableinfo (name, age) VALUES('John Smith', '33')";
            int nReg = 0;

            //open connection
            if  (this.OpenConnection())
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                nReg = cmd.ExecuteNonQuery();

                LogFile.Log(" --- Insert processado! regs.: " + nReg.ToString());

                //close connection
                this.CloseConnection();

            }

            return nReg;

        }

        //Update statement
        public void Update(string query)
        {
            //string query = "UPDATE tableinfo SET name='Joe', age='22' WHERE name='John Smith'";

            //Open connection
            if  (this.OpenConnection())
            {
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = connection;

                //Execute query
                int nReg = cmd.ExecuteNonQuery();

                LogFile.Log(" --- Update processado! regs.: " + nReg.ToString());

                //close connection
                this.CloseConnection();
            }
        }

        //Delete statement
        public void Delete(string query)
        {
            //string query = "DELETE FROM tableinfo WHERE name='John Smith'";

            if  (this.OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);

                int nReg = cmd.ExecuteNonQuery();

                LogFile.Log(" --- Delete processado! regs.: " + nReg.ToString());

                this.CloseConnection();
            }
        }

        //Select statement
        public List<string>[] Select(string query, List<string> campos)
        {
            //string query = "SELECT * FROM tableinfo";

            //List<string> campos = new List<string>(new string[] { "id", "name", "age" });

            //Create a list to store the result
            List<string>[] list = new List<string>[3];

            foreach (string campo in campos)
            {
                list[campos.IndexOf(campo)] = new List<string>();
            }
            //list[0] = new List<string>();
            //list[1] = new List<string>();
            //list[2] = new List<string>();

            //Open connection
            if (this.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    //list[0].Add(dataReader["id"] + "");
                    //list[1].Add(dataReader["name"] + "");
                    //list[2].Add(dataReader["age"] + "");

                    foreach (string campo in campos)
                    {
                        list[campos.IndexOf(campo)].Add(dataReader[campo] + "");
                    }

                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return list;
            }
            else
            {
                return list;
            }
        }

        //Count statement
        public int Count(string query)
        {
            //string query = "SELECT Count(*) FROM tableinfo";
            int Count = -1;

            //Open Connection
            if (this.OpenConnection() == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //ExecuteScalar will return one value
                Count = int.Parse(cmd.ExecuteScalar() + "");

                //close Connection
                this.CloseConnection();

                return Count;
            }
            else
            {
                return Count;
            }
        }

        //Backup
        public void Backup()
        {
            try
            {
                DateTime Time = DateTime.Now;
                int year = Time.Year;
                int month = Time.Month;
                int day = Time.Day;
                int hour = Time.Hour;
                int minute = Time.Minute;
                int second = Time.Second;
                int millisecond = Time.Millisecond;

                //Save file to C:\ with the current date as a filename
                string path;
                path = "C:\\MySqlBackup" + year + "-" + month + "-" + day +
                        "-" + hour + "-" + minute + "-" + second + "-" + millisecond + ".sql";
                StreamWriter file = new StreamWriter(path);


                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysqldump";
                psi.RedirectStandardInput = false;
                psi.RedirectStandardOutput = true;
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}",
                    uid, password, server, database);
                psi.UseShellExecute = false;

                Process process = Process.Start(psi);

                string output;
                output = process.StandardOutput.ReadToEnd();
                file.WriteLine(output);
                process.WaitForExit();
                file.Close();
                process.Close();
            }
            catch //(IOException ex)
            {
                LogFile.Log("Erro: Incapaz de realizar o backup!");
            }
        }

        //Restore
        public void Restore()
        {
            try
            {
                //Read file from C:\
                string path;
                path = "C:\\MySqlBackup.sql";
                StreamReader file = new StreamReader(path);
                string input = file.ReadToEnd();
                file.Close();

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysql";
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = false;
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}",
                    uid, password, server, database);
                psi.UseShellExecute = false;


                Process process = Process.Start(psi);
                process.StandardInput.WriteLine(input);
                process.StandardInput.Close();
                process.WaitForExit();
                process.Close();
            }
            catch //(IOException ex)
            {
                LogFile.Log("Erro: Incapaz de realizar a restauracao!");
            }
        }
    }
}
