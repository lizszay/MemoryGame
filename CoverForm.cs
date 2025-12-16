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
            this.TopMost = true;    //форма всегда переднего плана
            this.ShowInTaskbar = false; //не появится на панели позадач 
            this.ControlBox = false;    //убрать верхнюю панеь окна
            this.MaximizeBox = false;   //нельзя развернуть
            this.MinimizeBox = false;   //нельзя свернуть

            // Черный фон для заглушки
            this.BackColor = Color.Black;

            // Измените текст на "ЗАГРУЗКА..."
            Label loadingLabel = new Label();
            loadingLabel.Text = "ЗАГРУЗКА...";
            loadingLabel.Font = new Font("Arial", 48, FontStyle.Bold);
            loadingLabel.ForeColor = Color.White; 
            loadingLabel.TextAlign = ContentAlignment.MiddleCenter;
            loadingLabel.Dock = DockStyle.Fill;

            this.Controls.Add(loadingLabel);
        }
    }
}