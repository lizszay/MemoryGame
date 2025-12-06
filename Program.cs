using System;
using System.Windows.Forms;

namespace MemoryGame
{
    static class Program
    {
        public static MainMenuForm MainMenuForm { get; private set; }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainMenuForm = new MainMenuForm();
            Application.Run(MainMenuForm);
        }
    }
}