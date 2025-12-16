using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MemoryGame
{
    public partial class GameResultForm : Form
    {
        private int stars;
        private int moves;
        private int elapsedSeconds;
        private string levelName;
        private int rows;
        private int columns;

        public GameResultForm(int stars, int moves, int elapsedSeconds, string levelName, int rows, int columns)
        {
            this.stars = stars;
            this.moves = moves;
            this.elapsedSeconds = elapsedSeconds;
            this.levelName = levelName;
            this.rows = rows;
            this.columns = columns;

            InitializeComponent();
            SetupForm();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Настройка формы - увеличиваем размер
            this.Text = "Результат игры";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(500, 700); // Увеличили высоту
            //this.BackColor = Color.LightBlue;
            this.Font = new Font("Times New Roman", 12, FontStyle.Regular);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Padding = new Padding(20);

            try
            {
                //Application.StartupPath - расположение папки программы
                // Path.Combine аргументы - продолжение, вернёт строку целого пути к файлу
                string bgPath = Path.Combine(Application.StartupPath, "img", "ui", "background2.jpg");
                if (File.Exists(bgPath))    //существввует ли файл
                {
                    this.BackgroundImage = Image.FromFile(bgPath); //создаёт объект Image из указанного файла
                    this.BackgroundImageLayout = ImageLayout.Stretch;  //изображение растягиввается
                }
            }
            catch { }

            this.ResumeLayout(false);
        }

        private void SetupForm()
        {
            // Создаем основной контейнер с вертикальной прокруткой
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            // Контейнер для содержимого
            TableLayoutPanel contentTable = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                Dock = DockStyle.Top, // Важно: Dock = Top для авторазмера
                Padding = new Padding(10),
                BackColor = Color.Transparent
            };

            // Задаем фиксированные высоты строк для лучшего контроля
            contentTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));  // Заголовок
            contentTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // Уровень
            contentTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 15));  // Отступ
            contentTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // Заголовок звезд
            contentTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));  // Звезды
            contentTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Текст звезд
            contentTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 15));  // Отступ
            contentTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // Заголовок статистики
            contentTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 140)); // Статистика
            contentTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Отступ
            contentTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Кнопки

            // Общая высота: 70+40+15+40+70+60+15+40+140+80+60 = 630

            // 1. Заголовок
            Label titleLabel = new Label
            {
                Text = "🎉 УРОВЕНЬ ПРОЙДЕН! 🎉",
                Font = new Font("Times New Roman", 22, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Height = 60,
                Margin = new Padding(0)
            };

            // 2. Уровень
            Label levelLabel = new Label
            {
                Text = $"Уровень: {levelName} ({rows}×{columns})",
                Font = new Font("Times New Roman", 14, FontStyle.Italic),
                ForeColor = Color.DarkSlateBlue,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Height = 35,
                Margin = new Padding(0)
            };

            // 3. Заголовок звезд
            Label starsTitleLabel = new Label
            {
                Text = "Ваш результат:",
                Font = new Font("Times New Roman", 16, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Height = 35,
                Margin = new Padding(0, 5, 0, 5)
            };

            // 4. Панель со звездами
            Panel starsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 60,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            // Центрируем звезды
            int starSize = 50;
            int totalStarsWidth = 3 * starSize + 2 * 20; // 3 звезды + отступы
            int startX = (contentTable.Width - totalStarsWidth) / 2;

            for (int i = 0; i < 3; i++)
            {
                PictureBox star = new PictureBox
                {
                    Size = new Size(starSize, starSize),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Location = new Point(startX + i * (starSize + 20), 5)
                };

                SetStarImage(star, i < stars);
                starsPanel.Controls.Add(star);
            }

            // 5. Текстовый результат звезд
            string starsText = GetStarsText(stars);
            Label starsResultLabel = new Label
            {
                Text = $"{stars} из 3 звезд\n{starsText}",
                Font = new Font("Times New Roman", 14, FontStyle.Regular),
                ForeColor = Color.DarkRed,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Height = 50,
                Margin = new Padding(0)
            };

            // 6. Заголовок статистики
            Label statsTitleLabel = new Label
            {
                Text = "📊 Статистика игры:",
                Font = new Font("Times New Roman", 16, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Height = 35,
                Margin = new Padding(0, 5, 0, 5)
            };

            // 7. Статистика в отдельной панели
            Panel statsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 130,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15),
                Margin = new Padding(10, 5, 10, 5)
            };

            string timeFormatted = FormatTime(elapsedSeconds);
            int totalCards = rows * columns;
            int pairs = totalCards / 2;
            double movesPerPair = pairs > 0 ? (double)moves / pairs : moves;
            double timePerCard = totalCards > 0 ? (double)elapsedSeconds / totalCards : 0;
            double efficiency = CalculateEfficiency(totalCards, pairs, moves, elapsedSeconds);

            Label statsLabel = new Label
            {
                Text = $"• Ходы: {moves} ({movesPerPair:F1} на пару)\n" +
                       $"• Время: {timeFormatted} ({timePerCard:F1} сек. на карту)\n" +
                       $"• Карты: {totalCards} ({pairs} пар)\n" +
                       $"• Эффективность: {efficiency:F1}%",
                Font = new Font("Times New Roman", 14),
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoSize = false,
                Height = 100
            };

            statsPanel.Controls.Add(statsLabel);

            // 8. Панель кнопок
            Panel buttonsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 50,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 10, 0, 0)
            };

            Button playAgainButton = new Button
            {
                Text = "🔄 Играть ещё",
                Font = new Font("Times New Roman", 12, FontStyle.Bold),
                BackColor = Color.Gold,
                ForeColor = Color.DarkBlue,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 40),
                Location = new Point(30, 5),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            playAgainButton.FlatAppearance.BorderSize = 2;
            playAgainButton.FlatAppearance.BorderColor = Color.DarkOrange;

            Button menuButton = new Button
            {
                Text = "🏠 В меню",
                Font = new Font("Times New Roman", 12, FontStyle.Bold),
                BackColor = Color.LightSteelBlue,
                ForeColor = Color.DarkBlue,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 40),
                Location = new Point(240, 5),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            menuButton.FlatAppearance.BorderSize = 2;
            menuButton.FlatAppearance.BorderColor = Color.DarkBlue;

            buttonsPanel.Controls.Add(playAgainButton);
            buttonsPanel.Controls.Add(menuButton);

            // Добавляем все элементы в таблицу
            contentTable.Controls.Add(titleLabel, 0, 0);
            contentTable.Controls.Add(levelLabel, 0, 1);
            contentTable.Controls.Add(new Panel(), 0, 2); // Отступ
            contentTable.Controls.Add(starsTitleLabel, 0, 3);
            contentTable.Controls.Add(starsPanel, 0, 4);
            contentTable.Controls.Add(starsResultLabel, 0, 5);
            contentTable.Controls.Add(new Panel(), 0, 6); // Отступ
            contentTable.Controls.Add(statsTitleLabel, 0, 7);
            contentTable.Controls.Add(statsPanel, 0, 8);
            contentTable.Controls.Add(new Panel(), 0, 9); // Отступ
            contentTable.Controls.Add(buttonsPanel, 0, 10);

            // Добавляем контент в основной Panel
            mainPanel.Controls.Add(contentTable);
            this.Controls.Add(mainPanel);

            // Подписываемся на событие изменения размера для центрирования звезд
            this.Load += (s, e) => CenterStars(starsPanel);
            this.Resize += (s, e) => CenterStars(starsPanel);
        }

        private void CenterStars(Panel starsPanel)
        {
            int starSize = 50;
            int totalStarsWidth = 3 * starSize + 2 * 20;

            // Получаем ширину родительского контейнера
            int parentWidth = starsPanel.Parent is TableLayoutPanel table ? table.Width : starsPanel.Parent.Width;
            int startX = (parentWidth - totalStarsWidth) / 2;

            // Обновляем позицию звезд
            for (int i = 0; i < starsPanel.Controls.Count; i++)
            {
                starsPanel.Controls[i].Location = new Point(startX + i * (starSize + 20), 5);
            }
        }

        private void SetStarImage(PictureBox star, bool filled)
        {
            try
            {
                string starPath = filled ? "img/ui/star_filled.png" : "img/ui/star_empty.png";
                if (File.Exists(starPath))
                {
                    star.Image = Image.FromFile(starPath);
                }
                else
                {
                    // Создаем простую звезду
                    Bitmap bmp = new Bitmap(star.Width, star.Height);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.Transparent);
                        if (filled)
                        {
                            g.FillPolygon(Brushes.Gold, new Point[]
                            {
                                new Point(25, 5),
                                new Point(30, 20),
                                new Point(45, 20),
                                new Point(32, 30),
                                new Point(38, 45),
                                new Point(25, 35),
                                new Point(12, 45),
                                new Point(18, 30),
                                new Point(5, 20),
                                new Point(20, 20)
                            });
                        }
                        else
                        {
                            g.DrawPolygon(Pens.Gray, new Point[]
                            {
                                new Point(25, 5),
                                new Point(30, 20),
                                new Point(45, 20),
                                new Point(32, 30),
                                new Point(38, 45),
                                new Point(25, 35),
                                new Point(12, 45),
                                new Point(18, 30),
                                new Point(5, 20),
                                new Point(20, 20)
                            });
                        }
                    }
                    star.Image = bmp;
                }
            }
            catch
            {
                star.BackColor = filled ? Color.Gold : Color.LightGray;
                star.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        private string GetStarsText(int starsCount)
        {
            return starsCount switch
            {
                0 => "Попробуйте ещё раз!",
                1 => "Неплохо, но можно лучше!",
                2 => "Хороший результат!",
                3 => "Отлично! Идеальная игра!",
                _ => "Завершено!"
            };
        }

        private string FormatTime(int seconds)
        {
            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;
            return $"{minutes:D2}:{remainingSeconds:D2}";
        }

        private double CalculateEfficiency(int totalCards, int pairs, int moves, int elapsedSeconds)
        {
            if (pairs == 0 || totalCards == 0) return 0;

            double movesPerPair = (double)moves / pairs;
            double timePerCard = (double)elapsedSeconds / totalCards;

            double idealMovesPerPair = 1.2;
            double idealTimePerCard = 4.0;

            double movesEfficiency = Math.Max(0, 100 - (movesPerPair - idealMovesPerPair) * 25);
            double timeEfficiency = Math.Max(0, 100 - (timePerCard - idealTimePerCard) * 10);

            return (movesEfficiency * 0.6 + timeEfficiency * 0.4);
        }
    }
}