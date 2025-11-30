using System;
using System.Drawing;
using System.Windows.Forms;

namespace MemoryGame
{
    public partial class RulesForm : Form
    {
        private Form previousForm;

        public RulesForm(Form previous)
        {
            previousForm = previous;
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Memory Game - Правила";
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.LightYellow;

            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackgroundImage = Image.FromFile("img/ui/background.jpg");
            mainPanel.BackgroundImageLayout = ImageLayout.Stretch;

            TableLayoutPanel tableLayout = new TableLayoutPanel();
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.RowCount = 3;
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 15));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 15));

            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = "ПРАВИЛА ИГРЫ";
            titleLabel.Font = new Font("Times New Roman", 36, FontStyle.Bold);
            titleLabel.ForeColor = Color.DarkRed;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Dock = DockStyle.Fill;
            tableLayout.Controls.Add(titleLabel, 0, 0);

            // Текст правил
            Panel rulesPanel = new Panel();
            rulesPanel.Dock = DockStyle.Fill;
            rulesPanel.AutoScroll = true;
            rulesPanel.BackColor = Color.FromArgb(200, Color.White);

            Label rulesLabel = new Label();
            rulesLabel.Text = GetRulesText();
            rulesLabel.Font = new Font("Times New Roman", 20, FontStyle.Regular);
            rulesLabel.ForeColor = Color.Black;
            rulesLabel.Dock = DockStyle.Fill;
            rulesLabel.Padding = new Padding(50);
            rulesLabel.TextAlign = ContentAlignment.MiddleCenter;

            rulesPanel.Controls.Add(rulesLabel);
            tableLayout.Controls.Add(rulesPanel, 0, 1);

            // Кнопка возврата
            Button backButton = new Button();
            backButton.Text = "Вернуться назад";
            backButton.Font = new Font("Times New Roman", 24, FontStyle.Bold);
            backButton.BackColor = Color.Gold;
            backButton.ForeColor = Color.DarkRed;
            backButton.FlatStyle = FlatStyle.Flat;
            backButton.FlatAppearance.BorderSize = 3;
            backButton.FlatAppearance.BorderColor = Color.DarkRed;
            backButton.Dock = DockStyle.Fill;
            backButton.Margin = new Padding(200, 10, 200, 10);
            backButton.Click += (sender, e) => this.Close();

            tableLayout.Controls.Add(backButton, 0, 2);

            mainPanel.Controls.Add(tableLayout);
            this.Controls.Add(mainPanel);

            this.FormClosing += RulesForm_FormClosing;
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

        private void RulesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}