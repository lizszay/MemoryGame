using MemoryGame.GameLogic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MemoryGame
{
    public partial class GameForm : BufferedForm
    {
        // Компоненты формы
        private System.ComponentModel.IContainer components = null;

        // Защищенный метод для освобождения ресурсов
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
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

            // Возобновление перерисовки и применение разметки
            this.ResumeLayout(true);
            this.PerformLayout();
        }

        // Настройка основных свойств формы
        private void ConfigureFormProperties()
        {
            this.Text = "Memory Game - Игра";
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.LightBlue;
        }

        // Создание всех панелей интерфейса
        private void CreateInterfacePanels()
        {
            // Создание верхней панели с информацией
            CreateTopPanel();

            // Создание правой панели с кнопками управления
            CreateRightPanel();

            // Создание центральной панели для карт
            CreateGamePanel();

            // Создание оверлея паузы
            CreatePauseOverlay();
        }

        // Подписка на события формы
        private void WireFormEvents()
        {
            // Инициализация карт после загрузки формы
            this.Load += (s, e) => InitializeCardButtons();

            // Вывод оверлея паузы на передний план
            pauseOverlay.BringToFront();
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

            // Компоновка панели
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

        // Создание контейнера для звезд
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

            // Инициализация звезд (пустые при создании)
            UpdateStars();

            container.Controls.Add(starsPanel);
            return container;
        }

        // Создание правой панели с кнопками управления
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

            // Компоновка панели
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
            Button menuButton = CreateSideButton("В меню");
            menuButton.Click += MenuButton_Click;

            // Кнопка показа правил
            Button rulesButton = CreateSideButton("📖 Правила");
            rulesButton.Click += (s, e) => ShowRulesFromGame();

            // Добавление кнопок в таблицу
            table.Controls.Add(pauseButton, 0, 0);
            table.Controls.Add(menuButton, 0, 1);
            table.Controls.Add(rulesButton, 0, 2);
        }

        // Создание стилизованной боковой кнопки
        private Button CreateSideButton(string text)
        {
            Button button = new Button
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

            // Настройка внешнего вида рамки
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
                BackColor = Color.LightGreen,
                Padding = new Padding(20, 120, 20, 20) // Отступ сверху для верхней панели
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
                BackColor = Color.FromArgb(150, Color.Black), // Полупрозрачный черный
                Visible = false // Скрыта по умолчанию
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
            // Доступное пространство с учетом отступов
            int availableWidth = gamePanel.Width - 50;
            int availableHeight = gamePanel.Height - 50;

            // Максимальные размеры для одной карты
            int maxWidth = availableWidth / gameBoard.Columns - 50;
            int maxHeight = availableHeight / gameBoard.Rows - 50;

            // Выбор меньшего размера для гарантии размещения
            return Math.Min(maxWidth, maxHeight);
        }

        // Расчет стартовой позиции для сетки карт
        private Point CalculateCardStartPosition(int cardSize, int spacing)
        {
            // Центрирование по горизонтали
            int gridWidth = gameBoard.Columns * (cardSize + spacing);
            int startX = (gamePanel.Width - gridWidth) / 2;

            // Центрирование по вертикали с учетом верхней панели
            int topPanelHeight = 100;
            int availableHeight = gamePanel.Height - topPanelHeight;
            int gridHeight = gameBoard.Rows * (cardSize + spacing);
            int startY = topPanelHeight + (availableHeight - gridHeight) / 2;

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
                Button cardButton = CreateCardButton(i, cardSize, startPosition, spacing, row, col);
                gamePanel.Controls.Add(cardButton);
            }
        }

        // Создание отдельной кнопки-карты
        private Button CreateCardButton(int index, int cardSize, Point startPosition, int spacing, int row, int col)
        {
            Card card = gameBoard.Cards[index];

            // Создание кнопки
            Button cardButton = new Button
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
            cardButton.FlatAppearance.BorderColor = Color.DarkBlue;

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