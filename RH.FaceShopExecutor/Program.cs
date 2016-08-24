using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace RH.FaceShopExecutor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var path = Application.ExecutablePath;
            var di = new FileInfo(path);
            path = di.DirectoryName + @"\..\..\HeadShop Plugin\RH.HeadShop.exe";

      /*      using (var fs = new StreamWriter("C:\\1.txt"))
                foreach (var arg in args)
                    fs.WriteLine(arg);*/

            if (!File.Exists(path))
                MessageBox.Show("Can't find FaceShop plugin!", "FaceShop", MessageBoxButtons.OK);
            else
            {
                var processInfo = new ProcessStartInfo();
                processInfo.FileName = path;
                processInfo.Arguments = "fs," + string.Join(",", args);

                using (var process = Process.Start(processInfo))
                    process.WaitForExit();

            }
        }
    }
}
