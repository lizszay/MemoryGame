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
            Application.EnableVisualStyles();   //включение визуальных стилей
            Application.SetCompatibleTextRenderingDefault(false);   //спользует: Систему GDI+, лучше текст

            MainMenuForm = new MainMenuForm();
            Application.Run(MainMenuForm);
        }
    }
}