using System;
using System.Drawing;
using System.Threading.Tasks; // Добавить
using System.Windows.Forms;

namespace MemoryGame
{
    public class RulesForm : Form
    {
        public RulesForm()
        {
            // Двойная буферизация - предотвращает мерцание
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |  //для снижения мерцания
                         ControlStyles.UserPaint |  //отображение элемента управления выполняет сам элемент, а не операционная система
                         ControlStyles.DoubleBuffer, true); //сначла рисует в буфере памяти,  затем за раз выводится все на экран
            this.DoubleBuffered = true; // Дополнительная двойная буферизация

            // Настраиваем форму для плавной анимации
            this.Opacity = 0; // Начинаем прозрачной
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black; // Основной фон черный

            InitializeForm();

            // Анимация появления при загрузке
            this.Shown += async (s, e) =>
            {
                await FadeIn(this, 300);
            };
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

            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.Transparent;

            // Включаем двойную буферизацию для панели
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(mainPanel, true, null);

            try
            {
                string bgPath = System.IO.Path.Combine(Application.StartupPath, "img", "ui", "background.jpg");
                if (System.IO.File.Exists(bgPath))
                {
                    mainPanel.BackgroundImage = Image.FromFile(bgPath);
                    mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch { }

            TableLayoutPanel tableLayout = new TableLayoutPanel();

            // Двойная буферизация для таблицы
            typeof(TableLayoutPanel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(tableLayout, true, null);

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

            // Двойная буферизация для панели правил
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(rulesPanel, true, null);

            Label rulesLabel = new Label();
            rulesLabel.Text = GetRulesText();
            rulesLabel.Font = new Font("Times New Roman", 20, FontStyle.Regular); // Исправлено: New New Roman → New Roman
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
            backButton.Cursor = Cursors.Hand;

            // Асинхронное закрытие с анимацией
            backButton.Click += async (sender, e) =>
            {

                // Блокируем кнопку и форму, чтобы предотвратить повторные нажатия
                backButton.Enabled = false;
                this.Enabled = false;

                // Создаем черную форму-оверлей для плавного перехода без видимости рабочего стола
                Form blackOverlay = new Form
                {
                    FormBorderStyle = FormBorderStyle.None,
                    WindowState = FormWindowState.Maximized,
                    BackColor = Color.Black,
                    Opacity = 0,
                    TopMost = true,
                    ShowInTaskbar = false,
                    ControlBox = false
                };

                // Показываем черный оверлей
                blackOverlay.Show();
                blackOverlay.BringToFront();

                // Плавно увеличиваем прозрачность черного оверлея, одновременно уменьшая прозрачность формы правил
                for (double opacity = 0; opacity <= 1.0; opacity += 0.1)
                {
                    if (blackOverlay.IsDisposed) break;
                    blackOverlay.Opacity = opacity;
                    this.Opacity = 1.0 - opacity; // Форма правил исчезает по мере появления черного экрана
                    await Task.Delay(15);
                    Application.DoEvents();
                }

                // Закрываем черный оверлей
                if (!blackOverlay.IsDisposed)
                    blackOverlay.Close();

                // Закрываем форму правил - это вызовет событие FormClosed,
                // на которое подписан MainMenuForm для своего появления
                this.Close();
            };

            tableLayout.Controls.Add(backButton, 0, 2);

            mainPanel.Controls.Add(tableLayout);
            this.Controls.Add(mainPanel);

            this.FormClosing += RulesForm_FormClosing;

            this.ResumeLayout(true);
            this.PerformLayout();
        }

        // Метод плавного закрытия формы
        private async Task CloseWithAnimation()
        {
            await FadeOut(this, 150);
            this.Close();
        }

        // Метод плавного появления
        private async Task FadeIn(Form form, int duration)
        {
            for (double opacity = 0; opacity <= 1.0; opacity += 0.1)
            {
                if (form.IsDisposed) return;
                form.Opacity = opacity;
                await Task.Delay(duration / 10);
                Application.DoEvents();
            }
        }

        // Метод плавного исчезновения
        private async Task FadeOut(Form form, int duration)
        {
            for (double opacity = 1.0; opacity > 0; opacity -= 0.1)
            {
                if (form.IsDisposed) return;
                form.Opacity = opacity;
                await Task.Delay(duration / 10);
                Application.DoEvents();
            }
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
                // Убираем DialogResult, так как форма не модальная
                // this.DialogResult = DialogResult.OK; // УДАЛИТЬ ЭТУ СТРОКУ
            }
        }
    }
}