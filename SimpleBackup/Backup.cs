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

            List<(string, string)> backup = getConnection.getConn();

            foreach (var backupItem in backup)
            {
                string sourceDir = backupItem.Item1;
                string destination = backupItem.Item2;

                string[] allFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);

                List<string> filesWithExtensions = new List<string>();

                foreach (var item in allFiles)
                {
                    string extension = Path.GetExtension(item);
                    if (!String.IsNullOrEmpty(extension))
                    {
                        filesWithExtensions.Add(item);
                    }
                }

                foreach (string file in filesWithExtensions)
                {
                    string fileName = file.Substring(sourceDir.Length);
                    string directoryPath = Path.Combine(destination, Path.GetDirectoryName(fileName));

                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);

                    File.Copy(file, Path.Combine(directoryPath, Path.GetFileName(file)), true);
                    getConnection.InsertIntoBackupLog(file, directoryPath);
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
        public List<(string, string)> getConn()
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "select * from dbo.Simplebackup";
                con.Open();
                SqlCommand cmd = new SqlCommand(query, con);
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                List<(string, string)> SourceAndDestination = new List<(string, string)>();

                foreach (DataRow row in dt.Rows)
                {
                    string source = row["SourceDir"].ToString();
                    string destination = row["Destination"].ToString();
                    var pair = Tuple.Create(source, destination);
                    SourceAndDestination.Add(pair.ToValueTuple());
                }
                return SourceAndDestination;
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
