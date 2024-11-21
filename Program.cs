using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TwitchGFL
{
    internal static class Program
    {
        public static bool poe2 = false;
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        /// 

        [STAThread]
        static void Main(string[] args)
        {
            if (PriorProcess() != null)
            {

                MessageBox.Show("Another instance of the app is already running.");
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            if(args.Length > 0)
            {
                if(args.Any(x => x.Contains("POE2")))
                {
                    poe2 = true;
                }


                Application.Run(new Form1(true));


            }
            else
            {
                Application.Run(new Form1());
            }
        }

        public static Process PriorProcess()
        // Returns a System.Diagnostics.Process pointing to
        // a pre-existing process with the same name as the
        // current one, if any; or null if the current process
        // is unique.
        {
            Process curr = Process.GetCurrentProcess();
            Process[] procs = Process.GetProcessesByName(curr.ProcessName);
            foreach (Process p in procs)
            {
                if ((p.Id != curr.Id) &&
                    (p.MainModule.FileName == curr.MainModule.FileName))
                    return p;
            }
            return null;
        }



    }
}
