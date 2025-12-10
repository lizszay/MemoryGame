using System;
using System.Drawing;
using System.Windows.Forms;

namespace MemoryGame
{
    public partial class CustomLevelForm : BufferedForm
    {
        // === ОБЪЯВЛЕНИЕ UI КОМПОНЕНТОВ ===
        private System.ComponentModel.IContainer components = null;

        // Элементы управления формы
        private NumericUpDown rowsNumeric;
        private NumericUpDown columnsNumeric;
        private Label infoLabel;
        private Button startButton;
        private Button cancelButton;

        /// Освобождает используемые ресурсы
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// Инициализирует все компоненты формы
        private void InitializeComponent()
        {
            // Инициализация компонентов формы
            InitializeFormComponents();

            // Привязка событий
            WireEvents();

            // Начальное обновление информационной метки
            UpdateInfoLabel();
        }

        /// Создает и настраивает все компоненты формы
        private void InitializeFormComponents()
        {
            // Временное отключение перерисовки для предотвращения мерцания
            this.SuspendLayout();

            // === СОЗДАНИЕ ГЛАВНОЙ ПАНЕЛИ ===
            Panel mainPanel = CreateMainPanel();

            // === СОЗДАНИЕ ОСНОВНОЙ ТАБЛИЦЫ КОМПОНЕНТОВ ===
            TableLayoutPanel mainTable = CreateMainTableLayout();

            // === ДОБАВЛЕНИЕ КОМПОНЕНТОВ В ТАБЛИЦУ ===
            AddComponentsToTable(mainTable);

            // === ФИНАЛЬНАЯ КОМПОНОВКА ===
            mainPanel.Controls.Add(mainTable);
            this.Controls.Add(mainPanel);

            // Возобновление перерисовки и применение разметки
            this.ResumeLayout(true);
            this.PerformLayout();
        }

        /// Создает главную панель формы с фоновым изображением
        private Panel CreateMainPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightSteelBlue,
                Padding = new Padding(10)
            };

            // Включение двойной буферизации для панели
            EnableDoubleBuffering(panel);

            // Попытка загрузки фонового изображения
            try
            {
                string bgPath = "img/ui/background2.jpg";
                if (System.IO.File.Exists(bgPath))
                {
                    panel.BackgroundImage = Image.FromFile(bgPath);
                    panel.BackgroundImageLayout = ImageLayout.Stretch;
                    panel.BackColor = Color.Transparent;
                }
            }
            catch
            {
                // Игнорируем ошибки загрузки фона - остаемся с цветным фоном
            }

            return panel;
        }

        /// Создает основную таблицу для размещения компонентов
        private TableLayoutPanel CreateMainTableLayout()
        {
            TableLayoutPanel table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 7,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };

            // Настройка относительных размеров строк
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 10));   // Заголовок
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 8));    // Информация
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 12));   // Метка строк
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 12));   // Поле ввода строк
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 12));   // Метка столбцов
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 12));   // Поле ввода столбцов
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 34));   // Кнопки

            return table;
        }

        /// Добавляет все компоненты в таблицу
        private void AddComponentsToTable(TableLayoutPanel table)
        {
            // Заголовок формы
            table.Controls.Add(CreateTitleLabel(), 0, 0);

            // Информационная метка
            table.Controls.Add(CreateInfoLabel(), 0, 1);

            // Настройка количества строк
            table.Controls.Add(CreateRowsLabel(), 0, 2);
            table.Controls.Add(CreateRowsNumeric(), 0, 3);

            // Настройка количества столбцов
            table.Controls.Add(CreateColumnsLabel(), 0, 4);
            table.Controls.Add(CreateColumnsNumeric(), 0, 5);

            // Панель с кнопками
            table.Controls.Add(CreateButtonPanel(), 0, 6);
        }

        /// Создает метку-заголовок формы
        private Label CreateTitleLabel()
        {
            Label titleLabel = new Label
            {
                Text = "НАСТРОЙКА УРОВНЯ",
                Font = new Font("Times New Roman", 30, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            return titleLabel;
        }

        // Создает информационную метку
        private Label CreateInfoLabel()
        {
            infoLabel = new Label
            {
                Text = "Выберите количество строк и столбцов",
                Font = new Font("Times New Roman", 12, FontStyle.Italic),
                ForeColor = Color.DarkSlateGray,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            return infoLabel;
        }

        // Создает метку для настройки количества строк
        private Label CreateRowsLabel()
        {
            Label rowsLabel = new Label
            {
                Text = "Количество строк (1-5):",
                Font = new Font("Times New Roman", 16, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            return rowsLabel;
        }

        // Создает числовое поле для ввода количества строк
        private NumericUpDown CreateRowsNumeric()
        {
            rowsNumeric = new NumericUpDown
            {
                Font = new Font("Times New Roman", 16, FontStyle.Regular),
                Minimum = 1,
                Maximum = 5,
                Value = 3,
                Dock = DockStyle.Fill,
                TextAlign = HorizontalAlignment.Center,
                BackColor = Color.White,
                Margin = new Padding(150, 5, 150, 5)
            };

            return rowsNumeric;
        }

        // Создает метку для настройки количества столбцов
        private Label CreateColumnsLabel()
        {
            Label columnsLabel = new Label
            {
                Text = "Количество столбцов (1-5):",
                Font = new Font("Times New Roman", 16, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            return columnsLabel;
        }

        // Создает числовое поле для ввода количества столбцов
        private NumericUpDown CreateColumnsNumeric()
        {
            columnsNumeric = new NumericUpDown
            {
                Font = new Font("Times New Roman", 16, FontStyle.Regular),
                Minimum = 1,
                Maximum = 5,
                Value = 3,
                Dock = DockStyle.Fill,
                TextAlign = HorizontalAlignment.Center,
                BackColor = Color.White,
                Margin = new Padding(150, 5, 150, 5)
            };

            return columnsNumeric;
        }

        // Создает панель с кнопками управления
        private TableLayoutPanel CreateButtonPanel()
        {
            TableLayoutPanel buttonTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            // Настройка ширины столбцов
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));  // Кнопка "Начать"
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));  // Разделитель
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));  // Кнопка "Отмена"

            // Создание кнопок и разделителя
            startButton = CreateStartButton();
            Panel spacer = CreateSpacerPanel();
            cancelButton = CreateCancelButton();

            // Добавление элементов в таблицу кнопок
            buttonTable.Controls.Add(startButton, 0, 0);
            buttonTable.Controls.Add(spacer, 1, 0);
            buttonTable.Controls.Add(cancelButton, 2, 0);

            return buttonTable;
        }

        // Создает кнопку "Начать игру"
        private Button CreateStartButton()
        {
            Button button = new Button
            {
                Text = "▶️ НАЧАТЬ ИГРУ",
                Font = new Font("Times New Roman", 16, FontStyle.Bold),
                BackColor = Color.FromArgb(60, 179, 113),  // Зеленый цвет
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 5, 10, 5),
                Cursor = Cursors.Hand
            };

            // Настройка внешнего вида рамки
            button.FlatAppearance.BorderSize = 3;
            button.FlatAppearance.BorderColor = Color.DarkGreen;

            // Эффекты при наведении мыши
            button.MouseEnter += (s, e) =>
                button.BackColor = ControlPaint.Light(Color.FromArgb(60, 179, 113), 0.2f);
            button.MouseLeave += (s, e) =>
                button.BackColor = Color.FromArgb(60, 179, 113);

            return button;
        }

        // Создает пустую панель-разделитель между кнопками
        private Panel CreateSpacerPanel()
        {
            Panel spacer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            return spacer;
        }

        // Создает кнопку "Отмена"
        private Button CreateCancelButton()
        {
            Button button = new Button
            {
                Text = "❌ ОТМЕНА",
                Font = new Font("Times New Roman", 16, FontStyle.Bold),
                BackColor = Color.IndianRed,  // Красный цвет
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 5, 10, 5),
                Cursor = Cursors.Hand
            };

            // Настройка внешнего вида рамки
            button.FlatAppearance.BorderSize = 3;
            button.FlatAppearance.BorderColor = Color.DarkRed;

            // Эффекты при наведении мыши
            button.MouseEnter += (s, e) =>
                button.BackColor = ControlPaint.Light(Color.IndianRed, 0.2f);
            button.MouseLeave += (s, e) =>
                button.BackColor = Color.IndianRed;

            return button;
        }

        // Привязывает события к элементам управления
        private void WireEvents()
        {
            // Привязка событий изменения значений числовых полей
            rowsNumeric.ValueChanged += (s, e) => UpdateInfoLabel();
            columnsNumeric.ValueChanged += (s, e) => UpdateInfoLabel();

            // Привязка событий клика по кнопкам
            startButton.Click += StartButton_Click;
            cancelButton.Click += CancelButton_Click;
        }

        // Включает двойную буферизацию для указанного элемента управления
        private void EnableDoubleBuffering(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(control, true, null);
        }
    }
}