using System;
using System.Windows.Forms;

namespace Client
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 form;

            if (args.Length == 1)
                form = new Form1(args[0]);
            else
                form = new Form1();

            Application.Run(form);
        }
    }
}