using System;
using System.Drawing;
using System.Windows.Forms;

namespace MemoryGame
{
    //форма заглушка
    public class CoverForm : BufferedForm
    {
        public CoverForm()
        {
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Опционально: можно загрузить фоновое изображение
            try
            {
                string bgPath = System.IO.Path.Combine(Application.StartupPath, "img", "ui", "background2.jpg");
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