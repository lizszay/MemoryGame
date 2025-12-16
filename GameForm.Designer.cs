using MemoryGame.GameLogic;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MemoryGame
{
    public partial class GameForm : BufferedForm
    {
        private const int PANEL_MARGIN = 20; // отступ от краёв панели

        // контейнер для невидимых элементов
        private System.ComponentModel.IContainer components = null;

        // UI элементы - интеркативные
        private Panel topPanel = null!; // null! - значение будет позже
        private Panel rightPanel = null!;
        private Panel gamePanel = null!;
        private Panel pauseOverlay = null!;
        private Label timerLabel = null!;
        private Label movesLabel = null!;
        private Label levelLabel = null!;
        private FlowLayoutPanel starsPanel = null!;
        private System.Windows.Forms.Button pauseButton = null!;

        // Освобождение ресурсов
        protected override void Dispose(bool disposing)
        {
            //если компонент создан
            if (disposing && (components != null))
            {
                // Освобождаем все компоненты (таймеры, списки картинок и т.д.)
                components.Dispose();
            }
            // Вызываем родительский Dispose для освобождения базовых ресурсов
            base.Dispose(disposing);
        }


        // Инициализация компонентов формы
        private void InitializeComponent()
        {
            // Временное отключение перерисовки для предотвращения мерцания
            this.SuspendLayout();

            // Настройка свойств формы
            ConfigureFormProperties();

            // Создание всех панелей интерфейса
            CreateInterfacePanels();

            // Подписка на события загрузки формы
            WireFormEvents();

            // Возобновление перерисовки
            this.ResumeLayout(true);
            //принудительное применение логики макета к дочерним элементам
            this.PerformLayout();
        }

        // Настройка основных свойств формы
        private void ConfigureFormProperties()
        {
            this.Text = "Memory Game - Игра";
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackgroundImage = Image.FromFile(Path.Combine(
                    Application.StartupPath, "img", "ui", "background.jpg"));
            this.BackgroundImageLayout = ImageLayout.Zoom;  //масштабируется
        }
        // Создание всех панелей интерфейса
        private void CreateInterfacePanels()
        {
            
            CreateGamePanel();

            CreateTopPanel();

            CreateRightPanel();
            // Создание оверлея(элемент, располгаетсяпо верх для какой-то информации) паузы
            CreatePauseOverlay();
        }

        // Подписка на события загрузки формы
        private void WireFormEvents()
        {
            // Инициализация карт после загрузки формы
            this.Load += (s, e) => InitializeCardButtons();
        }

        // Создание верхней информационной панели
        private void CreateTopPanel()
        {
            // Основная панель фиксированной высоты
            topPanel = new Panel
            {
                Height = 100,
                Dock = DockStyle.Top,
                BackColor = Color.DarkBlue
            };

            // Таблица для равномерного распределения элементов
            TableLayoutPanel infoTable = CreateInfoTableLayout();

            // Добавление элементов информации в таблицу
            AddInfoTableElements(infoTable);

            topPanel.Controls.Add(infoTable);
            this.Controls.Add(topPanel);
        }

        // Создание таблицы для информационных элементов
        private TableLayoutPanel CreateInfoTableLayout()
        {
            TableLayoutPanel table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                BackColor = Color.Transparent
            };

            // Настройка ширины колонок (по 25% каждая)
            for (int i = 0; i < 4; i++)
            {
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            }

            return table;
        }

        // Добавление элементов в информационную таблицу
        private void AddInfoTableElements(TableLayoutPanel table)
        {
            // Метка таймера
            timerLabel = CreateInfoLabel("00:00", 20);
            table.Controls.Add(timerLabel, 0, 0);

            // Метка счетчика ходов
            movesLabel = CreateInfoLabel("Ходы: 0", 20);
            table.Controls.Add(movesLabel, 1, 0);

            // Контейнер для звезд
            Panel starsContainer = CreateStarsContainer();
            table.Controls.Add(starsContainer, 2, 0);

            // Метка уровня сложности
            levelLabel = CreateInfoLabel($"Уровень: {currentLevel}", 18);
            table.Controls.Add(levelLabel, 3, 0);
        }

        // Создание стилизованной информационной метки
        private Label CreateInfoLabel(string text, int fontSize)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Times New Roman", fontSize, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
        }

        private Panel CreateStarsContainer()
        {
            Panel container = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // Панель для горизонтального расположения звезд
            starsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            container.Controls.Add(starsPanel);
            return container;
        }

        private void CreateRightPanel()
        {
            // Основная панель фиксированной ширины
            rightPanel = new Panel
            {
                Width = 200,
                Dock = DockStyle.Right,
                BackColor = Color.DarkBlue
            };

            // Таблица для вертикального расположения кнопок
            TableLayoutPanel buttonTable = CreateButtonTableLayout();

            // Создание кнопок управления
            CreateControlButtons(buttonTable);

            rightPanel.Controls.Add(buttonTable);
            this.Controls.Add(rightPanel);
        }

        // Создание таблицы для кнопок управления
        private TableLayoutPanel CreateButtonTableLayout()
        {
            TableLayoutPanel table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                BackColor = Color.Transparent
            };

            // Настройка высоты строк (равные части)
            for (int i = 0; i < 3; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
            }

            return table;
        }

        // Создание кнопок управления
        private void CreateControlButtons(TableLayoutPanel table)
        {
            // Кнопка паузы
            pauseButton = CreateSideButton("⏸️ Пауза");
            pauseButton.Click += PauseButton_Click;

            // Кнопка возврата в меню
            System.Windows.Forms.Button menuButton = CreateSideButton("В меню");
            menuButton.Click += MenuButton_Click;

            // Кнопка показа правил
            System.Windows.Forms.Button rulesButton = CreateSideButton("📖 Правила");
            rulesButton.Click += (s, e) => ShowRulesFromGame();

            // Добавление кнопок в таблицу
            table.Controls.Add(pauseButton, 0, 0);
            table.Controls.Add(menuButton, 0, 1);
            table.Controls.Add(rulesButton, 0, 2);
        }

        // Создание стилизованной боковой кнопки
        private System.Windows.Forms.Button CreateSideButton(string text)
        {
            System.Windows.Forms.Button button = new System.Windows.Forms.Button
            {
                Text = text,
                Font = new Font("Times New Roman", 14, FontStyle.Bold),
                BackColor = Color.Gold,
                ForeColor = Color.DarkBlue,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 4, 8, 4),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 2;
            button.FlatAppearance.BorderColor = Color.White;

            return button;
        }

        // Создание центральной игровой панели
        private void CreateGamePanel()
        {
            gamePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent, // Сделать прозрачным

               // Padding = new Padding(20) // ← 10 пикселей достаточно

            };

            this.Controls.Add(gamePanel);
        }

        // Создание оверлея паузы
        private void CreatePauseOverlay()
        {
            // Полупрозрачная панель
            pauseOverlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(150, Color.Black),
                Visible = false
            };

            // Текст "ИГРА НА ПАУЗЕ"
            Label pauseLabel = new Label
            {
                Text = "ИГРА НА ПАУЗЕ",
                Font = new Font("Times New Roman", 48, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            pauseOverlay.Controls.Add(pauseLabel);
            this.Controls.Add(pauseOverlay);

            // Переместить оверлей на передний план
            pauseOverlay.BringToFront();
        }

        // Инициализация кнопок-карт на игровом поле
        private void InitializeCardButtons()
        {
            // Очистка предыдущих кнопок
            gamePanel.Controls.Clear();

            // Проверка готовности панели
            if (gamePanel.Width <= 0 || gamePanel.Height <= 0)
                return;

            // Расчет размеров и позиционирования карт
            int cardSize = CalculateCardSize();
            int spacing = 10;
            Point startPosition = CalculateCardStartPosition(cardSize, spacing);

            // Создание кнопок для каждой карты
            CreateCardButtons(cardSize, spacing, startPosition);
        }

        // Расчет оптимального размера карты
        private int CalculateCardSize()
        {
            int spacing = 10;
            int horizontalGaps = (gameBoard.Columns - 1) * spacing;
            int verticalGaps = (gameBoard.Rows - 1) * spacing;

            // Доступная область = вся панель минус отступы
            int availableWidth = gamePanel.ClientSize.Width - 2 * PANEL_MARGIN;
            int availableHeight = gamePanel.ClientSize.Height - 2 * PANEL_MARGIN;

            if (gameBoard.Columns == 0 || gameBoard.Rows == 0)
                return 50;

            int maxWidth = (availableWidth - horizontalGaps) / gameBoard.Columns;
            int maxHeight = (availableHeight - verticalGaps) / gameBoard.Rows;

            return Math.Max(40, Math.Min(maxWidth, maxHeight));
        }

        // Расчет стартовой позиции для сетки карт
        private Point CalculateCardStartPosition(int cardSize, int spacing)
        {
            int horizontalGaps = (gameBoard.Columns - 1) * spacing;
            int totalWidth = gameBoard.Columns * cardSize + horizontalGaps;
            int startX = PANEL_MARGIN + (gamePanel.ClientSize.Width - 2 * PANEL_MARGIN - totalWidth) / 2;

            int verticalGaps = (gameBoard.Rows - 1) * spacing;
            int totalHeight = gameBoard.Rows * cardSize + verticalGaps;
            int startY = PANEL_MARGIN + (gamePanel.ClientSize.Height - 2 * PANEL_MARGIN - totalHeight) / 2;

            return new Point(startX, startY);
        }

        // Создание кнопок для всех карт
        private void CreateCardButtons(int cardSize, int spacing, Point startPosition)
        {
            for (int i = 0; i < gameBoard.Cards.Count; i++)
            {
                // Расчет позиции в сетке
                int row = i / gameBoard.Columns;
                int col = i % gameBoard.Columns;

                // Создание кнопки-карты
                System.Windows.Forms.Button cardButton = CreateCardButton(i, cardSize, startPosition, spacing, row, col);
                gamePanel.Controls.Add(cardButton);
            }
        }

        // Создание отдельной кнопки-карты
        private System.Windows.Forms.Button CreateCardButton(int index, int cardSize, Point startPosition, int spacing, int row, int col)
        {
            Card card = gameBoard.Cards[index];

            // Создание кнопки
            System.Windows.Forms.Button cardButton = new System.Windows.Forms.Button
            {
                Size = new Size(cardSize, cardSize),
                Location = new Point(
                    startPosition.X + col * (cardSize + spacing),
                    startPosition.Y + row * (cardSize + spacing)
                ),
                Tag = card,
                BackgroundImageLayout = ImageLayout.Stretch,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            // Настройка внешнего вида
            cardButton.FlatAppearance.BorderSize = 2;
            //cardButton.FlatAppearance.BorderColor = Color.DarkBlue;

            // Установка изображения в зависимости от состояния карты
            if (card.IsMatched)
            {
                cardButton.Visible = false;
            }
            else
            {
                cardButton.BackgroundImage = card.IsFlipped ? card.Image : card.GetBackImage();
            }

            // Подписка на событие клика
            cardButton.Click += CardButton_Click;

            return cardButton;
        }
    }
}