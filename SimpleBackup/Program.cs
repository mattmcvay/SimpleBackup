using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace SimpleBackup
{
    public class Program
    {
        static void Main(string[] args)
        {
            var exitcode = HostFactory.Run(x => 
            {
                x.Service<Backup>(s =>
                {
                    s.ConstructUsing(backup => new Backup());
                    s.WhenStarted(backup => backup.Start());
                    s.WhenStopped(backup => backup.Stop());
                });

                x.RunAsLocalSystem();

                x.SetServiceName("SimpleBackupService");
                x.SetDisplayName("Simple Backup");
                x.SetDescription("Backs up selected files.");

            });

            int exitCodeValue = (int)Convert.ChangeType(exitcode, exitcode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }
    }
}
