using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace SimpleBackup
{
    public class Backup
    {
        private readonly System.Timers.Timer _timer;

        public Backup()
        {
             _timer = new System.Timers.Timer(60000 * 1440) {AutoReset = true};  //60000 = 1 minute  1400 = 1440/60 = 24 
                                                                                 // so, 1440 minutes / 60 = 24hours
            _timer.Elapsed += timerElapsed;
        }

        private void timerElapsed(object sender, ElapsedEventArgs e)
        {
            Tuple<string, string> con = GetConnection.getConn();
            string sourceDir = con.Item1;
            string destination = con.Item2;

            string[] extensions = new string[] { "*.gif", "*.xls", "*.xlsx", "*.txt", "*.pdf", "*.docx", "*.csv" };

            foreach (string extension in extensions)
            {
                string[] allFiles = Directory.GetFiles(sourceDir, extension, SearchOption.AllDirectories);

                foreach (string textFile in allFiles)
                {
                    string fileName = textFile.Substring(sourceDir.Length);
                    string directoryPath = Path.Combine(destination, Path.GetDirectoryName(fileName));
                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);

                    File.Copy(textFile, Path.Combine(directoryPath, Path.GetFileName(textFile)), true);
                }
            }
            //Console.ReadLine();
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop() 
        {
            _timer.Stop();
        }

    }

    public class GetConnection
    {    
        public static Tuple<string, string> getConn()
        {
            var ConString = ConfigurationManager.ConnectionStrings["BackupConn"].ConnectionString;
            SqlConnection con = new SqlConnection(ConString);
            string query = "select * from dbo.Simplebackup";
            con.Open();
            SqlCommand cmd = new SqlCommand(query, con);
            DataTable dt = new DataTable();
            dt.Load(cmd.ExecuteReader());

            string source = dt.Rows[0]["SourceDir"].ToString();
            string destination = dt.Rows[0]["Destination"].ToString();
            con.Close();
            return Tuple.Create(source, destination);
        }
    }
}
