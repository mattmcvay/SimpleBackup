﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.IO;

namespace SimpleBackup
{
    public class Backup
    {
        private readonly System.Timers.Timer _timer;

        public Backup()
        {
            //_timer = new System.Timers.Timer(20000) {AutoReset = true};  // For every 5 seconds

            // For every 24 hours
            _timer = new System.Timers.Timer(TimeSpan.FromHours(24).TotalMilliseconds) { AutoReset = true };

            _timer.Elapsed += timerElapsed;
        }

        private void timerElapsed(object sender, ElapsedEventArgs e)
        {      
            string sourceDir = @"C:\Users\Matt\OneDrive\Desktop\NeedsBackedUp\";
            string destination = @"D:\SimpleBackup\";

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
            Console.ReadLine();
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
}
