using MemoryGame.GameLogic;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer; // чтобы каждый раз не прописывть
using System.Threading.Tasks;

namespace MemoryGame
{
    public class GameForm : Form
    {
        // === ПОЛЯ, ХРАНЯЩИЕ СОСТОЯНИЕ ИГРЫ ===

        // Игровая доска, содержащая все карты и логику их размещения
        private GameBoard gameBoard;

        // Таймер для отслеживания времени игры
        private GameTimer gameTimer;

        // Счётчик ходов игрока
        private int moves;

        // Количество звёзд (от 1 до 3), отражающее эффективность прохождения
        private int stars;

        // Первая и вторая выбранные карты для сравнения
        private Card firstSelectedCard;
        private Card secondSelectedCard;

        // Флаг, указывающий, что идёт обработка хода (например, анимация)
        private bool isProcessing;

        // Флаг, указывающий, что игра на паузе
        private bool isPaused;

        private string currentTheme;    // Текущая выбранная тема 

        private string currentLevel;    // Текущий уровень сложности 

        // === UI-ЭЛЕМЕНТЫ ИНТЕРФЕЙСА - интерактивные визуальные компоненты ===

        // Верхняя панель с таймером, ходами, звёздами и названием уровня
        private Panel topPanel;

        // Правая панель с кнопками "Пауза" и "В меню"
        private Panel rightPanel;

        // Центральная панель, где отображаются карты
        private Panel gamePanel;

        // Полупрозрачный оверлей(элемент поверх основного контента), показываемый при паузе
        private Panel pauseOverlay;

        // Метки для отображения времени, количества ходов и уровня
        private Label timerLabel;
        private Label movesLabel;
        private Label levelLabel;

        // Панель для отображения звёзд 
        private FlowLayoutPanel starsPanel;

        // Кнопка "Пауза", расположенная на правой панели
        private Button pauseButton;

        // === КОНСТРУКТОРЫ ===

        // Конструктор для стандартных уровней: вызывает перегрузку с нулевыми размерами
        public GameForm(string theme, string level) : this(theme, level, 0, 0) { }

        // Основной конструктор: принимает тему, уровень и (опционально) размеры для пользовательского уровня
        public GameForm(string theme, string level, int customRows, int customColumns)
        {
            currentTheme = theme;
            currentLevel = level;

            // Настраиваем форму для поддержки анимации
            //this.Opacity = 0; // Начинаем прозрачной
            // this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;   //форму на весь экран
            this.BackColor = Color.Black; // Основной фон черный

            // Инициализируем игровую логику
            InitializeGame(customRows, customColumns);

            // Инициализируем пользовательский интерфейс
            InitializeForm();

            // Двойная буферизация - предотвращает мерцание
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |  //для снижения мерцания
                         ControlStyles.UserPaint |  //отображение элемента управления выполняет сам элемент, а не операционная система
                         ControlStyles.DoubleBuffer, true); //сначла рисует в буфере памяти,  затем за раз выводится все на экран
            this.DoubleBuffered = true; // Дополнительная двойная буферизация


        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED - двойная буферизация на уровне Windows
                return cp;
            }
        }

        // === ИНИЦИАЛИЗАЦИЯ ИГРОВОЙ ЛОГИКИ ===

        // Создаёт новое игровое поле с заданными параметрами и сбрасывает счётчики
        private void InitializeGame(int rows = 0, int columns = 0)
        {
            // Если размеры не заданы — получаем стандартные для текущего уровня
            if (rows == 0 || columns == 0)
            {
                (rows, columns) = LevelManager.GetLevelDimensions(currentLevel);
            }

            // Создаём новую игровую доску
            gameBoard = new GameBoard(rows, columns, GetThemeFolderName(currentTheme), currentLevel);

            // Создаём и настраиваем таймер
            gameTimer = new GameTimer();
            gameTimer.OnTick += UpdateTimer; // Подписываемся на событие тика

            // Сбрасываем все счётчики и флаги
            moves = 0;
            stars = 3;
            isProcessing = false;
            isPaused = false;
        }

        // === ИНИЦИАЛИЗАЦИЯ ПОЛЬЗОВАТЕЛЬСКОГО ИНТЕРФЕЙСА ===

        // Настраивает внешний вид формы и создаёт все панели интерфейса
        private void InitializeForm()
        {
            // Устанавливаем заголовок окна
            this.Text = "Memory Game - Игра";

            // Убираем стандартную рамку окна
            this.FormBorderStyle = FormBorderStyle.None;

            // Разворачиваем окно на весь экран
            this.WindowState = FormWindowState.Maximized;

            // Устанавливаем временный фон (до загрузки фона из изображения)
            this.BackColor = Color.LightBlue;

            // Создаём все панели интерфейса
            CreateTopPanel();        // Верхняя панель с информацией
            CreateRightPanel();      // Правая панель с кнопками
            CreateGamePanel();       // Центральная панель с картами
            CreatePauseOverlay();    // Оверлей паузы


            // Запускаем таймер игры
            gameTimer.Start();
            this.Load += (s, e) => InitializeCardButtons(); // ← вызовется, когда форма готова
        }

        // === СОЗДАНИЕ ВЕРХНЕЙ ПАНЕЛИ ===

        // Создаёт верхнюю панель с таймером, ходами, звёздами и названием уровня
        private void CreateTopPanel()
        {
            // Создаём панель фиксированной высоты
            topPanel = new Panel();
            topPanel.Height = 100;
            topPanel.Dock = DockStyle.Top; // Прикрепляем к верху формы
            topPanel.BackColor = Color.DarkBlue; // Тёмно-синий фон

            // Таблица для равномерного размещения четырёх элементов
            TableLayoutPanel tableLayout = new TableLayoutPanel();
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.ColumnCount = 4;
            // Каждая колонка занимает 25% ширины
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            // Метка для отображения времени
            timerLabel = CreateInfoLabel("00:00", 20);
            tableLayout.Controls.Add(timerLabel, 0, 0);

            // Метка для отображения количества ходов
            movesLabel = CreateInfoLabel("Ходы: 0", 20);
            tableLayout.Controls.Add(movesLabel, 1, 0);

            // Контейнер для звёзд (FlowLayoutPanel внутри Panel для лучшего выравнивания)
            Panel starsContainer = new Panel();
            starsContainer.Dock = DockStyle.Fill;
            starsPanel = new FlowLayoutPanel();
            starsPanel.FlowDirection = FlowDirection.LeftToRight; // Звёзды в ряд
            starsPanel.Dock = DockStyle.Fill;
            starsPanel.WrapContents = false; // Не переносим на новую строку
            UpdateStars(); // Инициализируем отображение звёзд
            starsContainer.Controls.Add(starsPanel);
            tableLayout.Controls.Add(starsContainer, 2, 0);

            // Метка с названием текущего уровня
            levelLabel = CreateInfoLabel($"Уровень: {currentLevel}", 18);
            tableLayout.Controls.Add(levelLabel, 3, 0);

            // Добавляем таблицу на панель, а панель — на форму
            topPanel.Controls.Add(tableLayout);
            this.Controls.Add(topPanel);
        }

        // Вспомогательный метод для создания стилизованной информационной метки
        private Label CreateInfoLabel(string text, int fontSize)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = new Font("Times New Roman", fontSize, FontStyle.Bold);
            label.ForeColor = Color.White; // Белый текст на тёмном фоне
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Dock = DockStyle.Fill; // Заполняет ячейку таблицы
            return label;
        }

        // === СОЗДАНИЕ ПРАВОЙ ПАНЕЛИ ===

        // Создаёт правую панель с кнопками "Пауза" и "В меню"
        private void CreateRightPanel()
        {
            rightPanel = new Panel();
            rightPanel.Width = 200; // Фиксированная ширина
            rightPanel.Dock = DockStyle.Right; // Прикрепляем к правому краю
            rightPanel.BackColor = Color.DarkBlue;

            // Таблица для вертикального размещения двух кнопок
            TableLayoutPanel buttonPanel = new TableLayoutPanel();
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.RowCount = 2;
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); // Каждая кнопка — 50% высоты
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            // Кнопка "Пауза"
            pauseButton = CreateSideButton("⏸️ Пауза");
            pauseButton.Click += PauseButton_Click; // Подписываемся на клик

            // Кнопка "В меню"
            Button menuButton = CreateSideButton("В меню");
            menuButton.Click += MenuButton_Click;

            // Добавляем кнопки в таблицу
            buttonPanel.Controls.Add(pauseButton, 0, 0);
            buttonPanel.Controls.Add(menuButton, 0, 1);

            rightPanel.Controls.Add(buttonPanel);
            this.Controls.Add(rightPanel);
        }

        // Вспомогательный метод для создания кнопок на боковой панели
        private Button CreateSideButton(string text)
        {
            Button button = new Button();
            button.Text = text;
            button.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            button.BackColor = Color.Gold; // Золотой фон
            button.ForeColor = Color.DarkBlue;
            button.FlatStyle = FlatStyle.Flat; // Плоский стиль
            button.FlatAppearance.BorderSize = 2; // Толщина рамки
            button.FlatAppearance.BorderColor = Color.White; // Цвет рамки
            button.Dock = DockStyle.Fill; // Заполняет ячейку
            button.Margin = new Padding(10); // Отступы
            button.Cursor = Cursors.Hand; // Курсор-рука при наведении
            return button;
        }

        // === СОЗДАНИЕ ЦЕНТРАЛЬНОЙ ПАНЕЛИ ===

        // Создаёт центральную панель, где будут отображаться карты
        private void CreateGamePanel()
        {
            gamePanel = new Panel();
            gamePanel.Dock = DockStyle.Fill;
            gamePanel.BackColor = Color.LightGreen;
            gamePanel.Padding = new Padding(20);

            // Устанавливаем отступ сверху, равный высоте верхней панели
            gamePanel.Padding = new Padding(20, 120, 20, 20); // 120 = 100(высота панели) + 20(отступ)

            this.Controls.Add(gamePanel);
        }

        // === СОЗДАНИЕ ОВЕРЛЕЯ ПАУЗЫ ===

        // Создаёт полупрозрачный экран с надписью "ИГРА НА ПАУЗЕ"
        private void CreatePauseOverlay()
        {
            pauseOverlay = new Panel();
            pauseOverlay.Dock = DockStyle.Fill;
            pauseOverlay.BackColor = Color.FromArgb(180, Color.Black); // Полупрозрачный чёрный
            pauseOverlay.Visible = false; // Скрыт по умолчанию

            // Надпись "ИГРА НА ПАУЗЕ"
            Label pauseLabel = new Label();
            pauseLabel.Text = "ИГРА НА ПАУЗЕ";
            pauseLabel.Font = new Font("Times New Roman", 48, FontStyle.Bold);
            pauseLabel.ForeColor = Color.White;
            pauseLabel.TextAlign = ContentAlignment.MiddleCenter;
            pauseLabel.Dock = DockStyle.Fill;

            pauseOverlay.Controls.Add(pauseLabel);
            this.Controls.Add(pauseOverlay);
        }

        // === ИНИЦИАЛИЗАЦИЯ КНОПОК КАРТ ===

        // Очищает панель и создаёт новые кнопки для всех карт на игровой доске
        private void InitializeCardButtons()
        {
            gamePanel.Controls.Clear(); // Удаляем старые кнопки

            // Рассчитываем оптимальный размер и позицию карт
            int cardSize = CalculateCardSize();
            int spacing = 10; // Промежуток между картами

            // Учитываем верхнюю панель при расчете начальной позиции
            // topPanel.Height = 100 (как указано в CreateTopPanel())
            int topPanelHeight = 100;

            // Центрируем сетку по горизонтали
            int startX = (gamePanel.Width - (gameBoard.Columns * (cardSize + spacing))) / 2;

            // Центрируем сетку по вертикали, учитывая высоту верхней панели
            // Вычитаем topPanelHeight из доступной высоты для карт
            int availableHeight = gamePanel.Height - topPanelHeight;
            int startY = topPanelHeight + (availableHeight - (gameBoard.Rows * (cardSize + spacing))) / 2;

            if (gamePanel.Width <= 0 || gamePanel.Height <= 0)
                return; // не готово — выходим

            // Проходим по всем картам на доске
            for (int i = 0; i < gameBoard.Cards.Count; i++)
            {
                int row = i / gameBoard.Columns; // Номер строки
                int col = i % gameBoard.Columns; // Номер столбца

                // Создаём кнопку-карту
                Button cardButton = new Button();
                cardButton.Size = new Size(cardSize, cardSize);
                cardButton.Location = new Point(
                    startX + col * (cardSize + spacing),
                    startY + row * (cardSize + spacing)
                );
                cardButton.Tag = gameBoard.Cards[i]; // Сохраняем ссылку на объект карты

                // Если карта уже сопоставлена — скрываем кнопку
                if (gameBoard.Cards[i].IsMatched)
                {
                    cardButton.Visible = false;
                }
                else
                {
                    // Устанавливаем изображение: лицевую сторону, если перевёрнута, иначе рубашку
                    cardButton.BackgroundImage = gameBoard.Cards[i].IsFlipped ?
                        gameBoard.Cards[i].Image : gameBoard.Cards[i].GetBackImage();
                }

                cardButton.BackgroundImageLayout = ImageLayout.Stretch; // Растягиваем изображение
                cardButton.FlatStyle = FlatStyle.Flat;
                cardButton.FlatAppearance.BorderSize = 2;
                cardButton.FlatAppearance.BorderColor = Color.DarkBlue;
                cardButton.Cursor = Cursors.Hand; // Курсор-рука

                // Подписываемся на клик по карте
                cardButton.Click += CardButton_Click;
                gamePanel.Controls.Add(cardButton);
            }
        }

        // Рассчитывает оптимальный размер карты в зависимости от размера панели и сетки
        private int CalculateCardSize()
        {
            int maxWidth = (gamePanel.Width - 50) / gameBoard.Columns - 50;
            int maxHeight = (gamePanel.Height - 50) / gameBoard.Rows - 50;
            return Math.Min(maxWidth, maxHeight); // Берём меньший размер, чтобы всё поместилось
        }

        // === ОБРАБОТКА КЛИКА ПО КАРТЕ ===

        // Главный обработчик клика по карте
        private void CardButton_Click(object sender, EventArgs e)
        {
            // Игнорируем клик, если идёт обработка хода или игра на паузе
            if (isProcessing || isPaused) return;

            // Получаем кнопку и связанную с ней карту
            Button clickedButton = (Button)sender;
            Card clickedCard = (Card)clickedButton.Tag;

            // Игнорируем, если карта уже перевёрнута или сопоставлена
            if (clickedCard.IsFlipped || clickedCard.IsMatched) return;

            // Переворачиваем карту
            FlipCard(clickedCard, clickedButton);

            // Если первая карта — запоминаем её
            if (firstSelectedCard == null)
            {
                firstSelectedCard = clickedCard;
            }
            else
            {
                // Вторая карта — запоминаем
                secondSelectedCard = clickedCard;

                // Увеличиваем счётчик ходов, только если ни одна из карт не является Shuffle
                if (firstSelectedCard.Type != CardType.Shuffle && secondSelectedCard.Type != CardType.Shuffle)
                {
                    moves++;
                    movesLabel.Text = $"Ходы: {moves}";
                    UpdateStars(); // Обновляем отображение звёзд
                }

                isProcessing = true; // Блокируем дальнейшие клики
                ProcessTurn();       // Обрабатываем ход
            }
        }

        // Вспомогательный метод: переворачивает карту (показывает её изображение)
        private void FlipCard(Card card, Button button)
        {
            card.IsFlipped = true;
            button.BackgroundImage = card.Image;
        }

        // === ОБРАБОТКА ХОДА ===

        // Определяет тип хода (Shuffle, Hint, совпадение, несовпадение) и вызывает соответствующий метод
        private void ProcessTurn()
        {
            // Если одна из карт — Shuffle
            if (firstSelectedCard.Type == CardType.Shuffle || secondSelectedCard.Type == CardType.Shuffle)
            {
                ProcessShuffleCard();
                return;
            }

            // Если одна из карт — Hint
            if (firstSelectedCard.Type == CardType.Hint || secondSelectedCard.Type == CardType.Hint)
            {
                ProcessHintCard();
                return;
            }

            // Обычный ход: проверяем совпадение по ID
            if (firstSelectedCard.Id == secondSelectedCard.Id)
            {
                ProcessMatch();
            }
            else
            {
                ProcessMismatch();
            }
        }

        // === ОБРАБОТКА КАРТЫ "ПОДСКАЗКА" ===

        private void ProcessHintCard()
        {
            // Определяем, какая карта — Hint, а какая — другая
            Card hintCard = firstSelectedCard.Type == CardType.Hint ? firstSelectedCard : secondSelectedCard;
            Card otherCard = firstSelectedCard.Type == CardType.Hint ? secondSelectedCard : firstSelectedCard;

            // Если вторая карта — обычная
            if (otherCard.Type == CardType.Regular)
            {
                // Ищем парную карту (с таким же ID, не сопоставленную и не перевёрнутую)
                Card matchingCard = gameBoard.Cards.FirstOrDefault(c =>
                    c.Id == otherCard.Id && c != otherCard && !c.IsMatched && !c.IsFlipped);

                if (matchingCard != null)
                {
                    // Переворачиваем найденную карту
                    Button matchingButton = FindButtonForCard(matchingCard);
                    FlipCard(matchingCard, matchingButton);

                    // Через 1.5 секунды: скрываем все три карты
                    Timer delayTimer = new Timer();
                    delayTimer.Interval = 1500;
                    delayTimer.Tick += (s, e) =>
                    {
                        delayTimer.Stop();
                        otherCard.IsMatched = true;
                        matchingCard.IsMatched = true;
                        hintCard.IsMatched = true;

                        // Скрываем кнопки
                        Button firstButton = FindButtonForCard(firstSelectedCard);
                        Button secondButton = FindButtonForCard(secondSelectedCard);
                        if (firstButton != null) firstButton.Visible = false;
                        if (secondButton != null) secondButton.Visible = false;
                        if (matchingButton != null) matchingButton.Visible = false;

                        ResetSelection();   // Сбрасываем выбор
                        CheckGameEnd();    // Проверяем завершение игры
                    };
                    delayTimer.Start();
                    return;
                }
            }

            // Если парная карта не найдена или вторая карта — не Regular
            Timer simpleTimer = new Timer();
            simpleTimer.Interval = 1000;
            simpleTimer.Tick += (s, e) =>
            {
                simpleTimer.Stop();
                hintCard.IsMatched = true;
                Button hintButton = FindButtonForCard(hintCard);
                if (hintButton != null) hintButton.Visible = false;
                ResetSelection();
                CheckGameEnd();
            };
            simpleTimer.Start();
        }

        // === ОБРАБОТКА КАРТЫ "ПЕРЕМЕШИВАНИЕ" ===

        private void ProcessShuffleCard()
        {
            // Определяем Shuffle и вторую карту
            Card shuffleCard = firstSelectedCard.Type == CardType.Shuffle ? firstSelectedCard : secondSelectedCard;
            Card otherCard = firstSelectedCard.Type == CardType.Shuffle ? secondSelectedCard : firstSelectedCard;

            Button shuffleButton = FindButtonForCard(shuffleCard);
            Button otherButton = FindButtonForCard(otherCard);

            // Переворачиваем Shuffle, если он ещё не перевёрнут
            if (shuffleButton != null && !shuffleCard.IsFlipped)
            {
                shuffleCard.IsFlipped = true;
                shuffleButton.BackgroundImage = shuffleCard.Image;
            }

            // Если вторая карта — обычная, закрываем её
            if (otherButton != null && otherCard.Type == CardType.Regular)
            {
                otherCard.IsFlipped = false;
                otherButton.BackgroundImage = otherCard.GetBackImage();
            }

            // Отключаем все кнопки на время анимации
            foreach (Button btn in gamePanel.Controls.OfType<Button>())
            {
                btn.Enabled = false;
            }

            // Через 1 секунду: скрываем Shuffle, перемешиваем и обновляем поле
            Timer timer = new Timer { Interval = 1000 };
            timer.Tick += (s, e) =>
            {
                timer.Stop();

                // Включаем кнопки (если не на паузе)
                foreach (Button btn in gamePanel.Controls.OfType<Button>())
                {
                    btn.Enabled = !isPaused;
                }

                // Помечаем Shuffle как сопоставленную и скрываем
                shuffleCard.IsMatched = true;
                shuffleButton?.Hide();

                // Перемешиваем все карты в игре
                gameBoard.ShuffleCards();

                // Перерисовываем поле (новая позиция карт)
                RecreateCardButtons();

                ResetSelection();
                CheckGameEnd();
            };
            timer.Start();
        }

        // === ОБРАБОТКА СОВПАДЕНИЯ ===

        private void ProcessMatch()
        {
            // Помечаем карты как сопоставленные
            firstSelectedCard.IsMatched = true;
            secondSelectedCard.IsMatched = true;

            // Через 0.5 секунды скрываем их
            Timer timer = new Timer();
            timer.Interval = 500;
            timer.Tick += (s, e) =>
            {
                timer.Stop();

                Button firstButton = FindButtonForCard(firstSelectedCard);
                Button secondButton = FindButtonForCard(secondSelectedCard);
                if (firstButton != null) firstButton.Visible = false;
                if (secondButton != null) secondButton.Visible = false;

                ResetSelection();
                CheckGameEnd();
            };
            timer.Start();
        }

        // === ОБРАБОТКА НЕСОВПАДЕНИЯ ===

        private void ProcessMismatch()
        {
            // Через 1 секунду закрываем обе карты
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                firstSelectedCard.IsFlipped = false;
                secondSelectedCard.IsFlipped = false;

                Button firstButton = FindButtonForCard(firstSelectedCard);
                Button secondButton = FindButtonForCard(secondSelectedCard);

                if (firstButton != null) firstButton.BackgroundImage = firstSelectedCard.GetBackImage();
                if (secondButton != null) secondButton.BackgroundImage = secondSelectedCard.GetBackImage();

                ResetSelection();
            };
            timer.Start();
        }

        // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===

        // Находит кнопку, связанную с заданной картой
        private Button FindButtonForCard(Card card)
        {
            foreach (Control control in gamePanel.Controls)
            {
                if (control is Button button && button.Tag == card)
                {
                    return button;
                }
            }
            return null;
        }

        // Сбрасывает выбор карт и снимает флаг обработки
        private void ResetSelection()
        {
            firstSelectedCard = null;
            secondSelectedCard = null;
            isProcessing = false;
        }

        // Пересоздаёт кнопки карт (например, после перемешивания)
        private void RecreateCardButtons()
        {
            InitializeCardButtons();
        }

        // Проверяет, завершена ли игра (все карты сопоставлены)
        private void CheckGameEnd()
        {
            if (gameBoard.AllCardsMatched())
            {
                gameTimer.Stop(); // Останавливаем таймер
                ShowGameCompletionDialog(); // Показываем диалог победы
            }
        }

        // === ДИАЛОГ ЗАВЕРШЕНИЯ УРОВНЯ ===

        private void ShowGameCompletionDialog()
        {
            // Создаём модальное окно "Поздравляем!"
            Form completionForm = new Form();
            completionForm.Text = "Поздравляем!";
            completionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            completionForm.StartPosition = FormStartPosition.CenterScreen;
            completionForm.Size = new Size(400, 250);
            completionForm.BackColor = Color.LightBlue;

            // Таблица для размещения элементов
            TableLayoutPanel tableLayout = new TableLayoutPanel();
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.RowCount = 4;
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

            // Надпись "Уровень пройден!"
            Label congratsLabel = new Label();
            congratsLabel.Text = "Уровень пройден!";
            congratsLabel.Font = new Font("Times New Roman", 18, FontStyle.Bold);
            congratsLabel.TextAlign = ContentAlignment.MiddleCenter;
            congratsLabel.Dock = DockStyle.Fill;

            // Статистика: ходы и время
            Label statsLabel = new Label();
            statsLabel.Text = $"Ходы: {moves}\nВремя: {gameTimer.GetFormattedTime()}";
            statsLabel.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            statsLabel.TextAlign = ContentAlignment.MiddleCenter;
            statsLabel.Dock = DockStyle.Fill;

            // Кнопка "Играть ещё"
            Button playAgainButton = new Button();
            playAgainButton.Text = "Играть ещё";
            playAgainButton.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            playAgainButton.BackColor = Color.Gold;
            playAgainButton.Click += (s, e) =>
            {
                completionForm.DialogResult = DialogResult.OK;
                completionForm.Close();
                StartNewGame(); // Начинаем новую игру
            };

            // Кнопка "В меню"
            Button menuButton = new Button();
            menuButton.Text = "В меню";
            menuButton.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            menuButton.BackColor = Color.LightSteelBlue;
            menuButton.Click += (s, e) =>
            {
                completionForm.DialogResult = DialogResult.Cancel;
                completionForm.Close();
                this.Close(); // Закрываем форму игры
            };

            // Добавляем элементы в таблицу
            tableLayout.Controls.Add(congratsLabel, 0, 0);
            tableLayout.Controls.Add(statsLabel, 0, 1);
            tableLayout.Controls.Add(playAgainButton, 0, 2);
            tableLayout.Controls.Add(menuButton, 0, 3);

            completionForm.Controls.Add(tableLayout);

            // Показываем диалог и реагируем на результат
            if (completionForm.ShowDialog() == DialogResult.OK)
            {
                // Играем заново (уже обработано в обработчике кнопки)
            }
            else
            {
                this.Close();
            }
        }

        // Начинает новую игру с теми же параметрами (вызывается из диалога победы)
        private void StartNewGame()
        {
            InitializeGame(); // Пересоздаём доску
            InitializeCardButtons(); // Обновляем интерфейс
            moves = 0;
            stars = 3;
            movesLabel.Text = "Ходы: 0";
            UpdateStars();
            gameTimer.Reset(); // Сбрасываем таймер
            gameTimer.Start();
        }

        // === ОБРАБОТКА ПАУЗЫ ===

        private void PauseButton_Click(object sender, EventArgs e)
        {
            if (!isPaused)
            {
                // Ставим игру на паузу
                gameTimer.Stop();
                pauseOverlay.Visible = true;
                pauseButton.Text = "▶️ Продолжить";
                isPaused = true;

                // Отключаем все кнопки карт
                foreach (Control control in gamePanel.Controls)
                {
                    if (control is Button button)
                    {
                        button.Enabled = false;
                    }
                }
            }
            else
            {
                // Возобновляем игру
                gameTimer.Start();
                pauseOverlay.Visible = false;
                pauseButton.Text = "⏸️ Пауза";
                isPaused = false;

                // Включаем кнопки
                foreach (Control control in gamePanel.Controls)
                {
                    if (control is Button button)
                    {
                        button.Enabled = true;
                    }
                }
            }
        }

        // Обработчик кнопки "В меню"
        private async void MenuButton_Click(object sender, EventArgs e)
        {
            // Если игра не завершена, спрашиваем подтверждение
            if (!gameBoard.AllCardsMatched())
            {
                // Блокируем форму, чтобы предотвратить ее закрытие во время показа диалога
                this.Enabled = false;

                // Создаем диалог подтверждения выхода
                DialogResult result = MessageBox.Show(
                    "Вы точно хотите выйти в меню? Текущий прогресс будет потерян.",
                    "Подтверждение выхода",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                // Включаем форму обратно
                this.Enabled = true;

                // Если пользователь отказался от выхода, прерываем операцию
                if (result == DialogResult.No)
                {
                    return;
                }
            }

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

            // Блокируем форму игры во время анимации
            this.Enabled = false;

            // Плавно увеличиваем прозрачность черного оверлея, одновременно уменьшая прозрачность игры
            for (double opacity = 0; opacity <= 1.0; opacity += 0.1)
            {
                if (blackOverlay.IsDisposed) break;
                blackOverlay.Opacity = opacity;
                this.Opacity = 1.0 - opacity; // Игра исчезает по мере появления черного экрана
                await Task.Delay(15);
                Application.DoEvents();
            }

            // Закрываем черный оверлей
            if (!blackOverlay.IsDisposed)
                blackOverlay.Close();

            // Закрываем игровую форму - это вызовет событие FormClosed,
            // на которое подписан MainMenuForm для своего появления
            this.Close();
        }

        // Метод плавного исчезновения (добавьте в GameForm)
        private async Task FadeOut(Form form, int duration)
        {
            for (double opacity = 1.0; opacity > 0; opacity -= 0.1)
            {
                form.Opacity = opacity;
                await Task.Delay(duration / 10);
                Application.DoEvents(); // Обрабатываем сообщения во время анимации
            }
        }

        // === ОБНОВЛЕНИЕ ОТОБРАЖЕНИЯ ЗВЁЗД ===

        private void UpdateStars()
        {
            starsPanel.Controls.Clear(); // Удаляем старые звёзды

            // Рассчитываем максимальное допустимое количество ходов (2 на каждую карту)
            int maxMoves = gameBoard.Rows * gameBoard.Columns * 2;
            double efficiency = (double)moves / maxMoves;

            // Определяем количество звёзд по эффективности
            if (efficiency > 0.8) stars = 1;
            else if (efficiency > 0.6) stars = 2;
            else stars = 3;

            // Создаём 3 звезды (заполненные и пустые)
            for (int i = 0; i < 3; i++)
            {
                PictureBox star = new PictureBox();
                star.Size = new Size(30, 30);
                star.SizeMode = PictureBoxSizeMode.StretchImage;

                try
                {
                    // Пытаемся загрузить изображения звёзд из файла
                    star.Image = i < stars ?
                        Image.FromFile("img/ui/star_filled.png") :
                        Image.FromFile("img/ui/star_empty.png");
                }
                catch
                {
                    // Если файлы не найдены — используем цветные квадраты
                    star.BackColor = i < stars ? Color.Gold : Color.Gray;
                }
                starsPanel.Controls.Add(star);
            }
        }

        // Обновляет отображение времени на основе таймера
        private void UpdateTimer(int seconds)
        {
            timerLabel.Text = gameTimer.GetFormattedTime();
        }

        // Преобразует название темы в имя папки с изображениями
        private string GetThemeFolderName(string theme)
        {
            switch (theme)
            {
                case "Животные": return "animals";
                case "Геометрические фигуры": return "geometry";
                case "Растения": return "plants";
                default: return "animals";
            }
        }

        // Метод плавного закрытия формы с анимацией
        private async void CloseWithAnimationAsync()
        {
            // Создаем черную форму-оверлей для плавного перехода
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

            // Блокируем форму игры во время анимации
            this.Enabled = false;

            // Плавно увеличиваем прозрачность черного оверлея
            for (double opacity = 0; opacity <= 1.0; opacity += 0.1)
            {
                if (blackOverlay.IsDisposed) break;
                blackOverlay.Opacity = opacity;
                this.Opacity = 1.0 - opacity;
                await Task.Delay(15);
                Application.DoEvents();
            }

            // Закрываем черный оверлей
            if (!blackOverlay.IsDisposed)
                blackOverlay.Close();

            // Закрываем форму игры
            this.Close();
        }
    }
}