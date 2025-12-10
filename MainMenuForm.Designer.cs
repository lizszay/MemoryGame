using System;
using System.Drawing;
using System.Windows.Forms;

namespace MemoryGame
{
    public partial class MainMenuForm : BufferedForm
    {
        // Компоненты формы
        private System.ComponentModel.IContainer components = null;

        // Основные элементы управления
        private Panel mainPanel;
        private TableLayoutPanel mainTable;
        private Label titleLabel;

        // Элементы выбора темы
        private Panel themePanel;
        private Label themeLabel;
        private FlowLayoutPanel themeFlow;
        private RadioButton rbAnimals;
        private RadioButton rbGeometry;
        private RadioButton rbPlants;

        // Элементы выбора уровня
        private Panel levelPanel;
        private Label levelLabel;
        private TableLayoutPanel levelTable;
        private Button btnEasy;
        private Button btnMedium;
        private Button btnHard;
        private Button btnExpert;
        private Button btnCustom;

        // Нижние кнопки
        private Panel bottomPanel;
        private TableLayoutPanel buttonTable;
        private Button btnRules;
        private Panel spacer;
        private Button btnExit;

        // Освобождение ресурсов
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        // Инициализация компонентов
        private void InitializeComponent()
        {
            // Временное отключение перерисовки
            this.SuspendLayout();

            // Настройка основных свойств формы
            ConfigureFormProperties();

            // Создание главной панели
            CreateMainPanel();

            // Создание интерфейса меню
            CreateMenuInterface();

            // Возобновление перерисовки
            this.ResumeLayout(true);
            this.PerformLayout();
        }

        // Настройка свойств формы
        private void ConfigureFormProperties()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(30, 40, 30);
            this.Font = new Font("Times New Roman", 14, FontStyle.Regular);
        }

        // Создание главной панели с фоном
        private void CreateMainPanel()
        {
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // Включение двойной буферизации для плавной анимации
            EnableDoubleBuffering(mainPanel);

            // Загрузка фонового изображения
            LoadBackgroundImage();

            // Добавление панели на форму
            this.Controls.Add(mainPanel);
        }

        // Включение двойной буферизации для элемента управления
        private void EnableDoubleBuffering(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(control, true, null);
        }

        // Загрузка фонового изображения
        private void LoadBackgroundImage()
        {
            try
            {
                // Формирование пути к файлу фона
                string bgPath = System.IO.Path.Combine(
                    Application.StartupPath, "img", "ui", "background2.jpg");

                if (System.IO.File.Exists(bgPath))
                {
                    mainPanel.BackgroundImage = Image.FromFile(bgPath);
                    mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch (Exception)
            {
                // Ошибка загрузки фона - остаемся с цветным фоном
            }
        }

        // Создание интерфейса главного меню
        private void CreateMenuInterface()
        {
            // Временное отключение перерисовки панели
            mainPanel.SuspendLayout();
            mainPanel.Controls.Clear();

            // Создание основной таблицы для размещения элементов
            CreateMainTable();

            // Создание и добавление секций меню
            CreateTitleSection();
            CreateThemeSection();
            CreateLevelSection();
            CreateBottomSection();

            // Добавление таблицы на главную панель
            mainPanel.Controls.Add(mainTable);

            // Возобновление перерисовки панели
            mainPanel.ResumeLayout(true);
        }

        // Создание основной таблицы компоновки
        private void CreateMainTable()
        {
            mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };

            // Настройка относительных размеров строк
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));  // Заголовок
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));  // Тема
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 40f));  // Уровень
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));  // Кнопки

            // Включение двойной буферизации для таблицы
            EnableDoubleBuffering(mainTable);
        }

        // Создание секции заголовка
        private void CreateTitleSection()
        {
            titleLabel = new Label
            {
                Text = "MEMORY GAME",
                Font = new Font("Times New Roman", 52, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(10)
            };

            mainTable.Controls.Add(titleLabel, 0, 0);
        }

        // Создание секции выбора темы
        private void CreateThemeSection()
        {
            // Панель для секции темы
            themePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(20),
                Padding = new Padding(10),
                BackColor = Color.Transparent
            };

            // Заголовок секции темы
            themeLabel = new Label
            {
                Text = "ВЫБЕРИТЕ ТЕМУ КАРТОЧЕК:",
                Font = new Font("Times New Roman", 24, FontStyle.Bold),
                ForeColor = Color.White,
                Height = 60,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // Контейнер для центрирования элементов темы
            Panel centeredContainer = CreateCenteredContainer();

            // Панель для радиокнопок тем
            CreateThemeRadioButtons(centeredContainer);

            // Компоновка элементов секции темы
            themePanel.Controls.Add(centeredContainer);
            themePanel.Controls.Add(themeLabel);

            // Добавление секции в основную таблицу
            mainTable.Controls.Add(themePanel, 0, 1);
        }

        // Создание контейнера для центрирования элементов
        private Panel CreateCenteredContainer()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
        }

        // Создание радиокнопок для выбора темы
        private void CreateThemeRadioButtons(Panel container)
        {
            // Панель с потоковым расположением для радиокнопок
            themeFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(10)
            };

            // Создание радиокнопок для каждой темы
            CreateThemeRadioButtonControls();

            // Добавление радиокнопок в потоковую панель
            themeFlow.Controls.Add(rbAnimals);
            themeFlow.Controls.Add(rbGeometry);
            themeFlow.Controls.Add(rbPlants);

            // Центрирование потоковой панели в контейнере
            CenterThemeFlowPanel(container);

            // Добавление потоковой панели в контейнер
            container.Controls.Add(themeFlow);
        }

        // Создание контролов радиокнопок тем
        private void CreateThemeRadioButtonControls()
        {
            // Радиокнопка "Животные" (выбрана по умолчанию)
            rbAnimals = CreateThemeRadioButton(
                "🐾 ЖИВОТНЫЕ",
                "Животные",
                true,
                Color.FromArgb(255, 200, 80)
            );

            // Радиокнопка "Геометрия"
            rbGeometry = CreateThemeRadioButton(
                "🔷 ГЕОМЕТРИЯ",
                "Геометрические фигуры",
                false,
                Color.FromArgb(100, 150, 255)
            );

            // Радиокнопка "Растения"
            rbPlants = CreateThemeRadioButton(
                "🌿 РАСТЕНИЯ",
                "Растения",
                false,
                Color.FromArgb(150, 255, 100)
            );
        }

        // Центрирование панели с радиокнопками
        private void CenterThemeFlowPanel(Panel container)
        {
            container.Resize += (s, e) =>
            {
                themeFlow.Location = new Point(
                    Math.Max(0, (container.ClientSize.Width - themeFlow.Width) / 2),
                    Math.Max(0, (container.ClientSize.Height - themeFlow.Height) / 2)
                );
            };
        }

        // Создание отдельной радиокнопки для темы
        private RadioButton CreateThemeRadioButton(string text, string value, bool isChecked, Color bgColor)
        {
            RadioButton rb = new RadioButton
            {
                Text = "  " + text, // Пробелы для отступа от кружочка
                Font = new Font("Times New Roman", 14, FontStyle.Bold),
                Size = new Size(230, 60),
                Margin = new Padding(10),
                Checked = isChecked,
                Tag = value,
                BackColor = bgColor,
                ForeColor = IsDarkColor(bgColor) ? Color.White : Color.DarkBlue,
                Appearance = Appearance.Normal,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(30, 0, 0, 0) // Отступ слева для кружочка
            };

            // Подписка на событие изменения состояния
            rb.CheckedChanged += ThemeRadioButton_CheckedChanged;

            return rb;
        }

        // Проверка, является ли цвет темным
        private bool IsDarkColor(Color color)
        {
            // Вычисление яркости цвета по формуле восприятия
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
            return luminance < 0.5; // Если яркость меньше 50%, считаем цвет темным
        }

        // Создание секции выбора уровня
        private void CreateLevelSection()
        {
            // Панель для секции уровня
            levelPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(20),
                Padding = new Padding(10)
            };

            // Заголовок секции уровня
            levelLabel = new Label
            {
                Text = "ВЫБЕРИТЕ УРОВЕНЬ СЛОЖНОСТИ:",
                Font = new Font("Times New Roman", 24, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent
            };

            // Таблица для кнопок уровней
            CreateLevelButtonsTable();

            // Компоновка элементов секции уровня
            levelPanel.Controls.Add(levelTable);
            levelPanel.Controls.Add(levelLabel);

            // Добавление секции в основную таблицу
            mainTable.Controls.Add(levelPanel, 0, 2);
        }

        // Создание таблицы с кнопками уровней
        private void CreateLevelButtonsTable()
        {
            levelTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 10) // Отступы сверху и снизу
            };

            // Настройка равномерных строк
            for (int i = 0; i < 5; i++)
            {
                levelTable.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
            }

            // Создание кнопок уровней
            CreateLevelButtonControls();

            // Добавление кнопок в таблицу
            AddLevelButtonsToTable();
        }

        // Создание контролов кнопок уровней
        private void CreateLevelButtonControls()
        {
            // Кнопка "Легкий" (желтый)
            btnEasy = CreateLevelButton(
                "⭐ ЛЕГКИЙ (2×2)",
                "Легкий",
                Color.FromArgb(255, 200, 80),
                Color.DarkBlue
            );

            // Кнопка "Средний" (оранжевый)
            btnMedium = CreateLevelButton(
                "⭐⭐ СРЕДНИЙ (3×3)",
                "Средний",
                Color.FromArgb(255, 170, 50),
                Color.DarkBlue
            );

            // Кнопка "Сложный" (темно-оранжевый)
            btnHard = CreateLevelButton(
                "⭐⭐⭐ СЛОЖНЫЙ (4×4)",
                "Сложный",
                Color.FromArgb(255, 140, 30),
                Color.White
            );

            // Кнопка "Эксперт" (красно-оранжевый)
            btnExpert = CreateLevelButton(
                "⭐⭐⭐⭐ ЭКСПЕРТ (5×5)",
                "Эксперт",
                Color.FromArgb(255, 110, 10),
                Color.White
            );

            // Кнопка "Пользовательский" (фиолетовый)
            btnCustom = CreateLevelButton(
                "🎮 ПОЛЬЗОВАТЕЛЬСКИЙ",
                "Пользовательский",
                Color.FromArgb(200, 150, 255),
                Color.DarkBlue
            );
        }

        // Добавление кнопок в таблицу уровней
        private void AddLevelButtonsToTable()
        {
            levelTable.Controls.Add(btnEasy, 0, 0);
            levelTable.Controls.Add(btnMedium, 0, 1);
            levelTable.Controls.Add(btnHard, 0, 2);
            levelTable.Controls.Add(btnExpert, 0, 3);
            levelTable.Controls.Add(btnCustom, 0, 4);
        }

        // Создание отдельной кнопки уровня
        private Button CreateLevelButton(string text, string levelTag, Color bgColor, Color textColor)
        {
            Button button = new Button
            {
                Text = text,
                Font = new Font("Times New Roman", 18, FontStyle.Bold),
                BackColor = bgColor,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(500, 60),
                Anchor = AnchorStyles.None, // Для центрирования в ячейке
                Margin = new Padding(10, 5, 10, 5),
                Tag = levelTag,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Настройка внешнего вида рамки
            button.FlatAppearance.BorderSize = 3;
            button.FlatAppearance.BorderColor = Color.DarkBlue;

            // Подписка на событие клика
            button.Click += LevelButton_Click;

            return button;
        }

        // Создание нижней секции с кнопками
        private void CreateBottomSection()
        {
            // Панель для нижней секции
            bottomPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(20),
                Padding = new Padding(20)
            };

            // Таблица для размещения кнопок
            CreateBottomButtonsTable();

            // Добавление таблицы на панель
            bottomPanel.Controls.Add(buttonTable);

            // Добавление секции в основную таблицу
            mainTable.Controls.Add(bottomPanel, 0, 3);
        }

        // Создание таблицы для нижних кнопок
        private void CreateBottomButtonsTable()
        {
            buttonTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 3,
                BackColor = Color.Transparent
            };

            // Настройка ширины столбцов
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f)); // Правила
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f)); // Разделитель
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f)); // Выход

            // Создание кнопок
            CreateBottomButtonControls();

            // Добавление кнопок в таблицу
            AddBottomButtonsToTable();
        }

        // Создание контролов нижних кнопок
        private void CreateBottomButtonControls()
        {
            // Кнопка "Правила игры"
            btnRules = new Button
            {
                Text = "📖 ПРАВИЛА ИГРЫ",
                Font = new Font("Times New Roman", 16, FontStyle.Bold),
                BackColor = Color.LightSteelBlue,
                ForeColor = Color.DarkBlue,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(20, 10, 10, 10),
                Cursor = Cursors.Hand
            };

            // Настройка внешнего вида кнопки правил
            btnRules.FlatAppearance.BorderSize = 3;
            btnRules.FlatAppearance.BorderColor = Color.DarkBlue;

            // Подписка на событие клика по кнопке правил
            btnRules.Click += (s, e) => ShowRules();

            // Пустая панель-разделитель
            spacer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // Кнопка "Выход"
            btnExit = new Button
            {
                Text = "❌ ВЫЙТИ ИЗ ИГРЫ",
                Font = new Font("Times New Roman", 16, FontStyle.Bold),
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 10, 20, 10),
                Cursor = Cursors.Hand
            };

            // Настройка внешнего вида кнопки выхода
            btnExit.FlatAppearance.BorderSize = 3;
            btnExit.FlatAppearance.BorderColor = Color.DarkRed;

            // Подписка на событие клика по кнопке выхода
            btnExit.Click += ExitButton_Click;
        }

        // Добавление нижних кнопок в таблицу
        private void AddBottomButtonsToTable()
        {
            buttonTable.Controls.Add(btnRules, 0, 0);
            buttonTable.Controls.Add(spacer, 1, 0);
            buttonTable.Controls.Add(btnExit, 2, 0);
        }
    }
}