using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace MemoryGame
{
    //шаблон для остальных форм
    public class BufferedForm : Form
    {
        public BufferedForm()
        {
            this.DoubleBuffered = true; //создание механизм адвойной буферизации

            // метод класса Control в WinForms, который устанавливает стили поведения элемента управления.
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |  //элемент управления не обрабатывает сообщение окна WM_ERASEBKGND, чтобы снизить мерцание
                         ControlStyles.UserPaint |  //отображение элемента управления выполняет сам элемент, а не ОС
                         ControlStyles.DoubleBuffer, true); //рисование выполняется в буфере, а после завершения результат выводится на экран
            

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;   //форму на весь экран
            this.BackColor = Color.Black; // Основной фон черный
        }

        // Переопределяем свойство CreateParams для настройки параметров создания окна
        //буферизация -для устранения мерцания путем рисования следующего кадра в скрытом буфере,
        //а затем мгновенной замены им текущего, что дает плавную картинку
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;    // Получаем базовые параметры
                // Добавляем флаг двойной буферизации на уровне Windows API
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED - полная двойная буферизация
                return cp;
            }
        }

        // Метод для включения двойной буферизации любому контролу
        public static void EnableDoubleBuffering(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(control, true, null);
        }
    }
}
