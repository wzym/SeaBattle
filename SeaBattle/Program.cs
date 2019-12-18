using System;
using System.Windows.Forms;

namespace SeaBattle
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);            
            var form = new MainWindow();
            var game = new GameModel(form);
            Application.Run(form);
        }
    }
}
