using System;
using System.Drawing;
using System.Windows.Forms;

namespace MemoryGame
{
    public class CoverForm : Form
    {
        public CoverForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.Black;

            // Опционально: можно загрузить фоновое изображение
            try
            {
                string bgPath = System.IO.Path.Combine(Application.StartupPath, "img", "ui", "background.jpg");
                if (System.IO.File.Exists(bgPath))
                {
                    this.BackgroundImage = Image.FromFile(bgPath);
                    this.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch
            {
                // Игнорируем ошибки загрузки фона
            }
        }
    }
}