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
             _timer = new System.Timers.Timer(60000 * 720) {AutoReset = true};  // Every 12 hours.

            // _timer = new System.Timers.Timer(10000) { AutoReset = false };  // Left in for testing

            _timer.Elapsed += timerElapsed;

        }

        private void timerElapsed(object sender, ElapsedEventArgs e)
        {
            GetConnection getConnection = new GetConnection();

            Tuple<string, string> con = getConnection.getConn();
            string sourceDir = con.Item1;
            string destination = con.Item2;
           
            string[] extensions = new string[] { ".gif", ".xls", ".xlsx", ".txt", ".pdf", ".doc", ".docx", ".csv", ".ppt", ".pptx" };

            string[] allFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);

            foreach (string textFile in allFiles)
            {
                if (extensions.Any(textFile.Contains))
                {
                    string fileName = textFile.Substring(sourceDir.Length);
                    string directoryPath = Path.Combine(destination, Path.GetDirectoryName(fileName));

                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);

                    File.Copy(textFile, Path.Combine(directoryPath, Path.GetFileName(textFile)), true);
                    getConnection.InsertIntoBackupLog(textFile, directoryPath);
                }                              
            }         
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
        private readonly string _connectionString;

        public GetConnection()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["BackupConn"].ConnectionString;
        }
        public  Tuple<string, string> getConn()
        {

            using (SqlConnection con = new SqlConnection(_connectionString))
            {

                string query = "select * from dbo.Simplebackup";
                con.Open();
                SqlCommand cmd = new SqlCommand(query, con);
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                string source = dt.Rows[0]["SourceDir"].ToString();
                string destination = dt.Rows[0]["Destination"].ToString();

                return Tuple.Create(source, destination);
            }
        }

        public void InsertIntoBackupLog(string fileName, string backupLocation)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                var dateNow = DateTime.Now;

                con.Open();
                SqlCommand cmd = new SqlCommand("dbo.insertRecord", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@FileName", fileName);
                cmd.Parameters.AddWithValue("@BackupLocation", backupLocation);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
