using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks; // Добавьте в начале файла

namespace MemoryGame
{
    public class MainMenuForm : Form
    {
        // По умолчанию
        private string selectedTheme = "Животные";
        private Panel mainPanel;
        private bool isInitialized = false; // Флаг, указывающий на завершение инициализации формы

        private bool isFirstActivation = true; // Флаг первого запуска

        public MainMenuForm()
        {
            // Двойная буферизация - предотвращает мерцание
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |  //для снижения мерцания
                         ControlStyles.UserPaint |  //отображение элемента управления выполняет сам элемент, а не операционная система
                         ControlStyles.DoubleBuffer, true); //сначла рисует в буфере памяти,  затем за раз выводится все на экран
            this.DoubleBuffered = true; // Дополнительная двойная буферизация

            // Устанавливаем черный фон для предотвращения мигания рабочего стола при переключении
            this.BackColor = Color.Black;

            // Инициализируем компоненты формы (создаем интерфейс)
            InitializeCustomComponents();

            //когда форма загрузится. вызовет сама форма, пусто
            this.Load += (s, e) =>
            {
                isInitialized = true; // Устанавливаем флаг, что инициализация завершена
                // Теперь форма готова к показу
            };

            // когда онко станет активным
            this.Activated += async (s, e) =>
            {
                isFirstActivation = false; // После первого срабатывания - флаг сбрасываем
            };

            this.Shown += (s, e) =>
            {
                // Когда форма первый раз показывается - сбрасываем флаг
                isFirstActivation = false; // После первого показа
            };
        }

            private async System.Threading.Tasks.Task SwitchToForm(Form nextForm)
            {
                // 1. Создаём и показываем затычку
                CoverForm cover = new CoverForm();
                cover.Show();
                cover.BringToFront();

                // 2. Даём системе время отрисовать затычку
                await System.Threading.Tasks.Task.Yield();

                // 3. Скрываем текущую форму (меню)
                this.Hide();

                // 4. Подписываемся на закрытие новой формы — чтобы вернуть меню
                nextForm.FormClosed += (sender, args) =>
                {
                    if (!this.IsDisposed)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            this.Show();
                            this.BringToFront();
                        });
                    }
                    if (!cover.IsDisposed)
                    {
                        cover.Invoke((MethodInvoker)delegate
                        {
                            cover.Close();
                        });
                    }
                };

                // 5. Показываем новую форму
                nextForm.Show();
                nextForm.BringToFront();

                if (!cover.IsDisposed)
                {
                    cover.Close();
                }
            }

        // Переопределяем свойство CreateParams для настройки параметров создания окна
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams; // Получаем базовые параметры
                // Добавляем флаг двойной буферизации на уровне Windows API
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED - полная двойная буферизация
                return cp;
            }
        }

        private void InitializeCustomComponents()
        {
            // ОТКЛЮЧАЕМ перерисовку на время создания элементов - предотвращает мерцание
            this.SuspendLayout();

            // Настраиваем базовые свойства формы
            this.FormBorderStyle = FormBorderStyle.None;    // Убираем стандартную рамку окна
            this.WindowState = FormWindowState.Maximized;   // Разворачиваем окно на весь экран
            this.BackColor = Color.FromArgb(30, 40, 30);    // Устанавливаем темно-зеленый фон (запасной вариант)
            this.Font = new Font("Times New Roman", 14, FontStyle.Regular); // Устанавливаем шрифт

            // Создаем главную панель для размещения всех элементов интерфейса
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;                // Растягиваем панель на всё окно
            mainPanel.BackColor = Color.Transparent;        // Делаем панель прозрачной

            // Включаем двойную буферизацию для панели через рефлексию (скрытое свойство)
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(mainPanel, true, null);

            // Пытаемся загрузить фоновое изображение
            try
            {
                // Формируем путь к файлу фона
                string bgPath = System.IO.Path.Combine(Application.StartupPath, "img", "ui", "background2.jpg");
                if (System.IO.File.Exists(bgPath))  // Проверяем существует ли файл
                {
                    // Загружаем изображение и устанавливаем как фон
                    mainPanel.BackgroundImage = Image.FromFile(bgPath);
                    mainPanel.BackgroundImageLayout = ImageLayout.Stretch; // Растягиваем на всю панель
                }
            }
            catch (Exception ex)
            {
                // Если не удалось загрузить фон, остаемся с цветным фоном
                // Можно добавить логирование ошибки: MessageBox.Show(ex.Message);
            }

            // Создаем интерфейс меню (все кнопки, метки и т.д.)
            CreateMenuInterface();

            // Добавляем главную панель на форму
            this.Controls.Add(mainPanel);

            // Подписываемся на событие закрытия формы
            this.FormClosing += MainMenuForm_FormClosing;

            // ВКЛЮЧАЕМ перерисовку после создания всех элементов
            this.ResumeLayout(true);    // Возобновляем перерисовку с обновлением разметки
            this.PerformLayout();       // Применяем вычисленные размеры и позиции
        }

        // Переопределяем метод, который вызывается когда форма впервые показывается
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e); // Вызываем базовый метод

            // Если форма еще не инициализирована, ждем завершения
            if (!isInitialized)
            {
                // Ожидаем установки флага isInitialized = true
                while (!isInitialized)
                {
                    
                    System.Threading.Thread.Sleep(10); // Небольшая пауза
                }
            }
        }

        private void CreateMenuInterface()
        {
            // Временное отключение перерисовки панели - предотвращает мерцание
            mainPanel.SuspendLayout();

            // Очищаем панель от старых элементов (если они были)
            mainPanel.Controls.Clear();

            // Создаем основную таблицу для размещения элементов меню
            TableLayoutPanel mainTable = new TableLayoutPanel();

            // Включаем двойную буферизацию для таблицы
            typeof(TableLayoutPanel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(mainTable, true, null);

            // Настраиваем таблицу
            mainTable.Dock = DockStyle.Fill;               // Растягиваем на всю панель
            mainTable.RowCount = 5;                        // 4 строки
            mainTable.ColumnCount = 1;                     // 1 столбец
            mainTable.BackColor = Color.Transparent;       // Прозрачный фон

            // Устанавливаем размеры строк (в процентах от высоты таблицы)
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 15f)); // Заголовок - 15%
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 25f)); // Темы - 25%
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 40f)); // Уровни - 40%
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 20f)); // Нижние кнопки - 20%

            // === 1. СОЗДАЕМ ЗАГОЛОВОК ===
            Label titleLabel = new Label();
            titleLabel.Text = "MEMORY GAME";
            titleLabel.Font = new Font("Times New Roman", 52, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(255, 255, 255); // Белый текст
            titleLabel.TextAlign = ContentAlignment.MiddleCenter; // Выравнивание по центру
            titleLabel.Dock = DockStyle.Fill;                     // Заполняет всю ячейку
            titleLabel.BackColor = Color.Transparent;             // Прозрачный фон
            titleLabel.Margin = new Padding(10);                  // Отступы вокруг
            mainTable.Controls.Add(titleLabel, 0, 0);             // Добавляем в таблицу (столбец 0, строка 0)

            // === 2. СОЗДАЕМ ПАНЕЛЬ ВЫБОРА ТЕМЫ ===
            Panel themePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(20),
                Padding = new Padding(10)
            };

            Label themeLabel = new Label
            {
                Text = "ВЫБЕРИТЕ ТЕМУ КАРТОЧЕК:",
                Font = new Font("Times New Roman", 24, FontStyle.Bold),
                ForeColor = Color.White,
                Height = 60,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Создаём контейнер для центрирования
            Panel centeredContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // FlowLayoutPanel будет содержать радиокнопки
            FlowLayoutPanel themeFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true, // ← важно для центрирования
                BackColor = Color.Transparent,
                Margin = new Padding(0), // отступы снаружи
                Padding = new Padding(10) // отступы внутри
            };

            // Создаём радиокнопки с меньшим шрифтом
            RadioButton rbAnimals = CreateThemeRadioButton("🐾 ЖИВОТНЫЕ", "Животные", true, Color.FromArgb(255, 200, 80));
            RadioButton rbGeometry = CreateThemeRadioButton("🔷 ГЕОМЕТРИЯ", "Геометрические фигуры", false, Color.FromArgb(100, 150, 255));
            RadioButton rbPlants = CreateThemeRadioButton("🌿 РАСТЕНИЯ", "Растения", false, Color.FromArgb(150, 255, 100));

            themeFlow.Controls.Add(rbAnimals);
            themeFlow.Controls.Add(rbGeometry);
            themeFlow.Controls.Add(rbPlants);

            // Центрируем FlowLayoutPanel по горизонтали
            centeredContainer.Controls.Add(themeFlow);
            themeFlow.Location = new Point(
                (centeredContainer.ClientSize.Width - themeFlow.Width) / 2,
                (centeredContainer.ClientSize.Height - themeFlow.Height) / 2
            );

            // Обновляем позицию при изменении размера
            centeredContainer.Resize += (s, e) =>
            {
                themeFlow.Location = new Point(
                    Math.Max(0, (centeredContainer.ClientSize.Width - themeFlow.Width) / 2),
                    Math.Max(0, (centeredContainer.ClientSize.Height - themeFlow.Height) / 2)
                );
            };

            // Добавляем всё на панель темы
            themePanel.Controls.Add(centeredContainer);
            themePanel.Controls.Add(themeLabel);

            mainTable.Controls.Add(themePanel, 0, 1);

            // === 3. СОЗДАЕМ ПАНЕЛЬ ВЫБОРА УРОВНЯ ===
            Panel levelPanel = new Panel();
            levelPanel.Dock = DockStyle.Fill;
            levelPanel.BackColor = Color.Transparent;
            levelPanel.Margin = new Padding(20);
            levelPanel.Padding = new Padding(10);

            Label levelLabel = new Label();
            levelLabel.Text = "ВЫБЕРИТЕ УРОВЕНЬ СЛОЖНОСТИ:";
            levelLabel.Font = new Font("Times New Roman", 24, FontStyle.Bold);
            levelLabel.ForeColor = Color.FromArgb(255, 255, 255);
            levelLabel.TextAlign = ContentAlignment.MiddleCenter;
            levelLabel.Dock = DockStyle.Top;
            levelLabel.Height = 60;
            levelLabel.BackColor = Color.Transparent;

            // Создаем таблицу для кнопок уровней
            TableLayoutPanel levelTable = new TableLayoutPanel();
            levelTable.Dock = DockStyle.Fill;
            levelTable.RowCount = 5;        // 5 строк для 5 уровней
            levelTable.ColumnCount = 1;     // 1 столбец
            levelTable.BackColor = Color.Transparent;
            //levelTable.Padding = new Padding(100, 10, 100, 10); // Большие отступы по бокам
            levelTable.Padding = new Padding(0, 10, 0, 10); // Только сверху/снизу

            // Настраиваем высоту строк (равномерно по 20%)
            for (int i = 0; i < 5; i++)
            {
                levelTable.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
            }

            // Создаем кнопки уровней с разными цветами
            Button btnEasy = CreateLevelButton("⭐ ЛЕГКИЙ (2×2)", "Легкий", Color.FromArgb(255, 200, 80), Color.DarkBlue);
            Button btnMedium = CreateLevelButton("⭐⭐ СРЕДНИЙ (3×3)", "Средний", Color.FromArgb(255, 170, 50), Color.DarkBlue);
            Button btnHard = CreateLevelButton("⭐⭐⭐ СЛОЖНЫЙ (4×4)", "Сложный", Color.FromArgb(255, 140, 30), Color.White);
            Button btnExpert = CreateLevelButton("⭐⭐⭐⭐ ЭКСПЕРТ (5×5)", "Эксперт", Color.FromArgb(255, 110, 10), Color.White);
            Button btnCustom = CreateLevelButton("🎮 ПОЛЬЗОВАТЕЛЬСКИЙ", "Пользовательский", Color.FromArgb(200, 150, 255), Color.DarkBlue);

            // Добавляем кнопки в таблицу уровней
            levelTable.Controls.Add(btnEasy, 0, 0);     // Легкий - строка 0
            levelTable.Controls.Add(btnMedium, 0, 1);   // Средний - строка 1
            levelTable.Controls.Add(btnHard, 0, 2);     // Сложный - строка 2
            levelTable.Controls.Add(btnExpert, 0, 3);   // Эксперт - строка 3
            levelTable.Controls.Add(btnCustom, 0, 4);   // Пользовательский - строка 4

            // Добавляем элементы на панель уровня
            levelPanel.Controls.Add(levelTable);
            levelPanel.Controls.Add(levelLabel);

            // Добавляем панель уровня в основную таблицу (столбец 0, строка 2)
            mainTable.Controls.Add(levelPanel, 0, 2);

            // === 4. СОЗДАЕМ НИЖНЮЮ ПАНЕЛЬ С КНОПКАМИ ===
            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Fill;
            bottomPanel.BackColor = Color.Transparent;
            bottomPanel.Margin = new Padding(20);
            bottomPanel.Padding = new Padding(20);

            // Таблица для размещения кнопок (Правила | Пусто | Выход)
            TableLayoutPanel buttonTable = new TableLayoutPanel();
            buttonTable.Dock = DockStyle.Fill;
            buttonTable.RowCount = 1;        // 1 строка
            buttonTable.ColumnCount = 3;     // 3 столбца
            buttonTable.BackColor = Color.Transparent;

            // Настраиваем ширину столбцов
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f)); // Правила - 30%
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f)); // Пустое место - 40%
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f)); // Выход - 30%

            // Создаем кнопку "Правила игры"
            Button btnRules = new Button();
            btnRules.Text = "📖 ПРАВИЛА ИГРЫ";
            btnRules.Font = new Font("Times New Roman", 16, FontStyle.Bold);
            btnRules.BackColor = Color.LightSteelBlue;
            btnRules.ForeColor = Color.DarkBlue;
            btnRules.FlatStyle = FlatStyle.Flat;                     // Плоский стиль
            btnRules.FlatAppearance.BorderSize = 3;                  // Толщина рамки
            btnRules.FlatAppearance.BorderColor = Color.DarkBlue;    // Цвет рамки
            btnRules.Dock = DockStyle.Fill;                         // Заполняет ячейку
            btnRules.Margin = new Padding(20, 10, 10, 10);          // Отступы
            btnRules.Cursor = Cursors.Hand;                         // Курсор-рука при наведении
            btnRules.Click += (s, e) => ShowRules();               // Обработчик клика

            // Создаем пустую панель-разделитель
            Panel spacer = new Panel();
            spacer.Dock = DockStyle.Fill;
            spacer.BackColor = Color.Transparent;

            // Создаем кнопку "Выход"
            Button btnExit = new Button();
            btnExit.Text = "❌ ВЫЙТИ ИЗ ИГРЫ";
            btnExit.Font = new Font("Times New Roman", 16, FontStyle.Bold);
            btnExit.BackColor = Color.IndianRed;
            btnExit.ForeColor = Color.White;
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderSize = 3;
            btnExit.FlatAppearance.BorderColor = Color.DarkRed;
            btnExit.Dock = DockStyle.Fill;
            btnExit.Margin = new Padding(10, 10, 20, 10);
            btnExit.Cursor = Cursors.Hand;
            btnExit.Click += ExitButton_Click;

            // Добавляем элементы в таблицу кнопок
            buttonTable.Controls.Add(btnRules, 0, 0);   // Правила - столбец 0
            buttonTable.Controls.Add(spacer, 1, 0);     // Разделитель - столбец 1
            buttonTable.Controls.Add(btnExit, 2, 0);    // Выход - столбец 2

            // Добавляем таблицу кнопок на нижнюю панель
            bottomPanel.Controls.Add(buttonTable);

            // Добавляем нижнюю панель в основную таблицу (столбец 0, строка 3)
            mainTable.Controls.Add(bottomPanel, 0, 3);

            // Добавляем основную таблицу на главную панель
            mainPanel.Controls.Add(mainTable);

            // Включаем перерисовку панели после добавления всех элементов
            mainPanel.ResumeLayout(true);
        }

        // Метод создания радиокнопки для выбора темы
        private RadioButton CreateThemeRadioButton(string text, string value, bool isChecked, Color bgColor)
        {
            RadioButton rb = new RadioButton
            {
                Text = "  " + text, // Три пробела для отступа от кружочка
                Font = new Font("Times New Roman", 14, FontStyle.Bold), 
                Size = new Size(230, 60), // ← чуть меньше
                Margin = new Padding(10), // ← уменьшен с 15 до 10
                //TextAlign = ContentAlignment.MiddleCenter,
                Checked = isChecked,
                Tag = value,
                BackColor = bgColor,
                ForeColor = IsDarkColor(bgColor) ? Color.White : Color.DarkBlue,
                Appearance = Appearance.Normal, // ← делает как кнопку (опционально)
                FlatStyle = FlatStyle.Flat,
                 TextAlign = ContentAlignment.MiddleLeft, // Текст слева, чтобы кружочек был виден
                Padding = new Padding(30, 0, 0, 0) // Отступ слева для кружочка
            };

            // Обработчик изменения состояния (выбрана/не выбрана)
            rb.CheckedChanged += (s, e) =>
            {
                if (rb.Checked) // Если кнопка выбрана
                {
                    selectedTheme = value;  // Сохраняем выбранную тему
                    
                }
            };


            return rb;
        }

        // Вспомогательная функция: определяет, тёмный ли цвет
        private bool IsDarkColor(Color c)
        {
            // Вычисление яркости цвета по формуле восприятия человеком
            double luminance = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255;
            // Если яркость меньше 50%, считаем цвет темным
            return luminance < 0.5;
        }

        // Метод создания кнопки для выбора уровня
        private Button CreateLevelButton(string text, string levelTag, Color bgColor, Color textColor)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Times New Roman", 18, FontStyle.Bold);
            btn.BackColor = bgColor;
            btn.ForeColor = textColor;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 3;
            btn.FlatAppearance.BorderColor = Color.DarkBlue;
            //btn.Dock = DockStyle.Fill; // Заполняет всю ячейку таблицы
            btn.Size = new Size(500, 60); // Ширина 500px, высота 60px

            // Устанавливаем Anchor для центрирования
            btn.Anchor = AnchorStyles.None; // Это центрирует кнопку в ячейке


            btn.Margin = new Padding(10, 5, 10, 5); // Отступы сверху/снизу меньше чем по бокам
            btn.Tag = levelTag; // Сохраняем название уровня в Tag
            btn.Cursor = Cursors.Hand; // Курсор-рука
            btn.TextAlign = ContentAlignment.MiddleCenter; // Текст по центру

            // Подписываемся на событие клика
            btn.Click += LevelButton_Click;
            return btn;
        }

        // Обработчик клика по кнопке уровня
        private void LevelButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;  // Получаем кнопку, по которой кликнули
            string level = btn?.Tag?.ToString();    // Получаем текст уровня из Tag

            if (level == "Пользовательский")   // Если выбран пользовательский уровень
            {
                // Создаем форму для настройки пользовательского уровня
                using (CustomLevelForm customForm = new CustomLevelForm())
                {
                    // Показываем форму как диалоговое окно
                    if (customForm.ShowDialog() == DialogResult.OK)
                    {
                        // Если пользователь нажал "OK", запускаем игру с выбранными параметрами
                        StartGame(selectedTheme, level, customForm.SelectedRows, customForm.SelectedColumns);
                    }
                }
            }
            // Иначе запускаем игру с выбранным стандартным уровнем
            else
            {
                StartGame(selectedTheme, level);
            }
        }

        // Метод запуска игры (упрощенный)
        private async void StartGame(string theme, string level, int rows = 0, int cols = 0)
        {
            GameForm gameForm;
            if (rows > 0 && cols > 0)
            {
                gameForm = new GameForm(theme, level, rows, cols);
            }
            else
            {
                gameForm = new GameForm(theme, level);
            }
            await this.SwitchToForm(gameForm);

            gameForm.ShowRulesRequested += async (s, e) =>
            {
                // Ставим игру на паузу
                gameForm.PauseGameForRules();

                RulesForm rulesForm = new RulesForm();
                await this.SwitchToForm(rulesForm);

                // После возврата — возобновляем игру
                gameForm.ResumeGameAfterRules();
            };
        }

        // Метод показа формы с правилами
        private async void ShowRules()
        {
            using (RulesForm rules = new RulesForm())
            {
                rules.ShowDialog(this); // ← модально, поверх меню
            }
            // После закрытия — меню уже на месте, ничего делать не надо
        }

        // Обработчик клика по кнопке "Выход"
        private void ExitButton_Click(object sender, EventArgs e)
        {
            ExitApplication(); // Вызываем метод выхода
        }

        // Метод выхода из приложения
        private void ExitApplication()
        {
            if (MessageBox.Show(
                "Вы действительно хотите выйти из игры?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit(); // Закрываем приложение
            }
        }

        // Обработчик события закрытия формы
        private void MainMenuForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Если причина закрытия формы = пользовательский выбор (крестик)
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Отменяем закрытие
                ExitApplication(); // Вызываем метод выхода с подтверждением
            }
        }
    }
}