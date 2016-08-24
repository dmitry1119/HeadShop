using System;
using System.Windows.Forms;

namespace RH.HeadShop
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                ProgramCore.MainForm = new frmMain(args.Length == 0 ? string.Empty : args[0]);
                Application.Run(ProgramCore.MainForm);
            }
            catch (Exception e)
            {
                ProgramCore.EchoToLog(e);
                MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, e.Message);
            }
        }
    }
}
