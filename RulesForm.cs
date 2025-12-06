using System;
using System.Drawing;
using System.Windows.Forms;

namespace MemoryGame
{
    public class RulesForm : Form
    {
        public RulesForm()
        {
            // Оптимизация отрисовки
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.DoubleBuffer, true);
            this.DoubleBuffered = true;

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;

            InitializeForm();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        private void InitializeForm()
        {
            this.SuspendLayout();

            this.Text = "Memory Game - Правила";

            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // Включаем двойную буферизацию
            EnableDoubleBuffering(mainPanel);

            // Фон
            try
            {
                string bgPath = System.IO.Path.Combine(Application.StartupPath, "img", "ui", "background.jpg");
                if (System.IO.File.Exists(bgPath))
                {
                    mainPanel.BackgroundImage = Image.FromFile(bgPath);
                    mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch { /* Игнорируем ошибки загрузки фона */ }

            // Основная таблица
            TableLayoutPanel tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill
            };
            tableLayout.RowCount = 3;
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 15));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 15));

            // Заголовок
            Label titleLabel = new Label
            {
                Text = "ПРАВИЛА ИГРЫ",
                Font = new Font("Times New Roman", 36, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            tableLayout.Controls.Add(titleLabel, 0, 0);

            // Панель с текстом
            Panel rulesPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(200, Color.White)
            };
            EnableDoubleBuffering(rulesPanel);

            Label rulesLabel = new Label
            {
                Text = GetRulesText(),
                Font = new Font("Times New Roman", 20),
                ForeColor = Color.Black,
                Dock = DockStyle.Fill,
                Padding = new Padding(50),
                TextAlign = ContentAlignment.MiddleCenter
            };
            rulesPanel.Controls.Add(rulesLabel);
            tableLayout.Controls.Add(rulesPanel, 0, 1);

            // Кнопка "Назад"
            Button backButton = new Button
            {
                Text = "Вернуться назад",
                Font = new Font("Times New Roman", 24, FontStyle.Bold),
                BackColor = Color.Gold,
                ForeColor = Color.DarkRed,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(200, 10, 200, 10),
                Cursor = Cursors.Hand
            };
            backButton.FlatAppearance.BorderSize = 3;
            backButton.FlatAppearance.BorderColor = Color.DarkRed;

            // Простое закрытие — всё!
            backButton.Click += (s, e) => this.Close();

            tableLayout.Controls.Add(backButton, 0, 2);

            mainPanel.Controls.Add(tableLayout);
            this.Controls.Add(mainPanel);

            this.ResumeLayout(true);
            this.PerformLayout();
        }

        private void EnableDoubleBuffering(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(control, true, null);
        }

        private string GetRulesText()
        {
            return @"🎮 ПРАВИЛА ИГРЫ 'MEMORY' 🎮

                ЦЕЛЬ ИГРЫ:
                Найти все парные карточки на игровом поле.

                ОСНОВНЫЕ ПРАВИЛА:
                • Игровое поле состоит из карточек, расположенных рубашкой вверх
                • За один ход можно открыть две карточки
                • Если карточки совпадают - они остаются открытыми
                • Если не совпадают - переворачиваются обратно
                • Игра продолжается пока все пары не будут найдены

                СПЕЦИАЛЬНЫЕ КАРТОЧКИ:

                🌟 КАРТА-ПОДСКАЗКА (HINT):
                • Открывает подходящую пару для выбранной карточки
                • Считается за один ход
                • Исчезает с поля после использования

                🔄 КАРТА ПЕРЕМЕШИВАНИЯ (SHUFFLE):
                • Перемешивает все не найденные карточки
                • Не считается за ход
                • Исчезает с поля до перемешивания

                УРОВНИ СЛОЖНОСТИ:

                🎯 ЛЕГКИЙ (2x2):
                • 2 пары обычных карточек
                • Без специальных карточек

                🎯 СРЕДНИЙ (3x3):
                • 4 пары обычных карточек
                • 1 карта-подсказка

                🎯 СЛОЖНЫЙ (4x4):
                • 8 пар обычных карточек
                • 1 карта-подсказка
                • 1 карта перемешивания

                🎯 ЭКСПЕРТ (5x5):
                • 12 пар обычных карточек
                • 2 карты-подсказки
                • 1 карта перемешивания

                🌟 СИСТЕМА ЗВЕЗД:
                • 3 звезды - отличный результат
                • 2 звезды - хороший результат  
                • 1 звезда - можно лучше

                УДАЧИ В ИГРЕ! 🍀";
            }


        private void backButton_Click(object sender, EventArgs e)
        {
            this.Close(); // этого достаточно!
        }
    }
}