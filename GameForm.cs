using MemoryGame.GameLogic;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace MemoryGame
{
    public partial class GameForm : BufferedForm
    {
        // === ПОЛЯ ДЛЯ ХРАНЕНИЯ СОСТОЯНИЯ ИГРЫ ===
        private GameBoard gameBoard;
        private GameTimer gameTimer;
        private int moves;
        private int stars;
        private Card firstSelectedCard;
        private Card secondSelectedCard;
        private bool isProcessing;
        private bool isPaused;
        private string currentTheme;
        private string currentLevel;
        private bool rulesOpenedFromGame;

        // === СВОЙСТВА ДЛЯ ДОСТУПА К UI ЭЛЕМЕНТАМ (ОБЪЯВЛЕНЫ В DESIGNER) ===
        // Эти элементы будут инициализированы в InitializeComponent()
        private Panel topPanel;
        private Panel rightPanel;
        private Panel gamePanel;
        private Panel pauseOverlay;
        private Label timerLabel;
        private Label movesLabel;
        private Label levelLabel;
        private FlowLayoutPanel starsPanel;
        private Button pauseButton;

        // === СОБЫТИЯ ФОРМЫ ===
        public event EventHandler ReturnToMenuRequested;
        public event EventHandler ShowRulesRequested;

        // === КОНСТРУКТОРЫ ===

        // Конструктор для стандартных уровней
        public GameForm(string theme, string level) : this(theme, level, 0, 0) { }

        /// Основной конструктор игры
        public GameForm(string theme, string level, int customRows, int customColumns)
        {
            currentTheme = theme;
            currentLevel = level;

            // Инициализация игровой логики
            InitializeGame(customRows, customColumns);

            // Инициализация пользовательского интерфейса
            InitializeComponent();
        }

        // === МЕТОДЫ ИНИЦИАЛИЗАЦИИ ИГРЫ ===

        // Инициализирует игровую логику с заданными параметрами
        private void InitializeGame(int rows = 0, int columns = 0)
        {
            // Получение размеров игрового поля
            if (rows == 0 || columns == 0)
            {
                (rows, columns) = LevelManager.GetLevelDimensions(currentLevel);
            }

            // Создание игровой доски
            gameBoard = new GameBoard(rows, columns, GetThemeFolderName(currentTheme), currentLevel);

            // Инициализация таймера
            gameTimer = new GameTimer();
            gameTimer.OnTick += UpdateTimer;

            // Сброс счетчиков и флагов
            ResetGameState();
        }

        /// Сбрасывает состояние игры к начальным значениям
        private void ResetGameState()
        {
            moves = 0;
            stars = 3;
            firstSelectedCard = null;
            secondSelectedCard = null;
            isProcessing = false;
            isPaused = false;
            rulesOpenedFromGame = false;
        }

        // === ОСНОВНЫЕ МЕТОДЫ ИГРОВОГО ПРОЦЕССА ===

        // Обработчик клика по карте
        private void CardButton_Click(object sender, EventArgs e)
        {
            // Проверка возможности обработки клика
            if (!CanProcessCardClick()) return;

            // Получение карты из кнопки
            Button clickedButton = (Button)sender;
            Card clickedCard = (Card)clickedButton.Tag;

            // Проверка состояния карты
            if (!IsCardClickable(clickedCard)) return;

            // Обработка выбора карты
            ProcessCardSelection(clickedCard, clickedButton);
        }

        // Обработка выбора карты
        private void ProcessCardSelection(Card card, Button button)
        {
            // Переворот карты
            FlipCard(card, button);

            // Запоминание первой или второй карты
            if (firstSelectedCard == null)
            {
                firstSelectedCard = card;
            }
            else
            {
                secondSelectedCard = card;
                ProcessCardPair();
            }
        }

        // Обработка пары выбранных карт
        private void ProcessCardPair()
        {
            // Увеличение счетчика ходов (если не shuffle)
            if (!IsShuffleCardInvolved())
            {
                IncrementMoveCounter();
            }

            // Блокировка дальнейших кликов
            isProcessing = true;

            // Определение типа хода
            ProcessTurn();
        }

        // Определяет тип хода и вызывает соответствующий метод обработки
        private void ProcessTurn()
        {
            // Проверка карты перемешивания
            if (IsShuffleCardInvolved())
            {
                ProcessShuffleCard();
                return;
            }

            // Проверка карты подсказки
            if (IsHintCardInvolved())
            {
                ProcessHintCard();
                return;
            }

            // Обычный ход: проверка совпадения
            if (firstSelectedCard.Id == secondSelectedCard.Id)
            {
                ProcessMatch();
            }
            else
            {
                ProcessMismatch();
            }
        }

        // Обработка совпадения карт
        private void ProcessMatch()
        {
            firstSelectedCard.IsMatched = true;
            secondSelectedCard.IsMatched = true;

            Timer timer = new Timer { Interval = 500 };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                HideMatchedCards();
                ResetSelection();
                CheckGameEnd();
            };
            timer.Start();
        }

        // Обработка несовпадения карт
        private void ProcessMismatch()
        {
            Timer timer = new Timer { Interval = 1000 };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                FlipCardsBack();
                ResetSelection();
            };
            timer.Start();
        }

        // Обработка карты-подсказки
        private void ProcessHintCard()
        {
            // Определение карты-подсказки и второй карты
            Card hintCard = firstSelectedCard.Type == CardType.Hint ? firstSelectedCard : secondSelectedCard;
            Card otherCard = firstSelectedCard.Type == CardType.Hint ? secondSelectedCard : firstSelectedCard;

            // Поиск парной карты для обычной карты
            if (otherCard.Type == CardType.Regular)
            {
                Card matchingCard = FindMatchingCard(otherCard);
                if (matchingCard != null)
                {
                    ProcessHintWithMatch(hintCard, otherCard, matchingCard);
                    return;
                }
            }

            // Обработка подсказки без найденной пары
            ProcessSimpleHint(hintCard);
        }

        // Обработка карты перемешивания
        private void ProcessShuffleCard()
        {
            Card shuffleCard = firstSelectedCard.Type == CardType.Shuffle ? firstSelectedCard : secondSelectedCard;
            Card otherCard = firstSelectedCard.Type == CardType.Shuffle ? secondSelectedCard : firstSelectedCard;

            // Подготовка к перемешиванию
            PrepareForShuffle(shuffleCard, otherCard);

            // Выполнение перемешивания с задержкой
            Timer timer = new Timer { Interval = 1000 };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                ExecuteShuffle(shuffleCard);
            };
            timer.Start();
        }

        // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ИГРОВОЙ ЛОГИКИ ===

        // Проверяет, может ли быть обработан клик по карте
        private bool CanProcessCardClick()
        {
            return !isProcessing && !isPaused;
        }

        // Проверяет, можно ли кликнуть по карте
        private bool IsCardClickable(Card card)
        {
            return !card.IsFlipped && !card.IsMatched;
        }

        // Проверяет, является ли одна из выбранных карт картой перемешивания
        private bool IsShuffleCardInvolved()
        {
            return firstSelectedCard?.Type == CardType.Shuffle || secondSelectedCard?.Type == CardType.Shuffle;
        }

        // Проверяет, является ли одна из выбранных карт картой-подсказкой
        private bool IsHintCardInvolved()
        {
            return firstSelectedCard?.Type == CardType.Hint || secondSelectedCard?.Type == CardType.Hint;
        }

        // Переворачивает карту лицевой стороной вверх
        private void FlipCard(Card card, Button button)
        {
            card.IsFlipped = true;
            button.BackgroundImage = card.Image;
        }

        // Переворачивает карты обратно рубашкой вверх
        private void FlipCardsBack()
        {
            firstSelectedCard.IsFlipped = false;
            secondSelectedCard.IsFlipped = false;

            Button firstButton = FindButtonForCard(firstSelectedCard);
            Button secondButton = FindButtonForCard(secondSelectedCard);

            if (firstButton != null) firstButton.BackgroundImage = firstSelectedCard.GetBackImage();
            if (secondButton != null) secondButton.BackgroundImage = secondSelectedCard.GetBackImage();
        }

        // Скрывает совпавшие карты
        private void HideMatchedCards()
        {
            Button firstButton = FindButtonForCard(firstSelectedCard);
            Button secondButton = FindButtonForCard(secondSelectedCard);

            if (firstButton != null) firstButton.Visible = false;
            if (secondButton != null) secondButton.Visible = false;
        }

        // Находит кнопку, связанную с указанной картой
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

        // Находит парную карту для указанной карты
        private Card FindMatchingCard(Card card)
        {
            return gameBoard.Cards.FirstOrDefault(c =>
                c.Id == card.Id && c != card && !c.IsMatched && !c.IsFlipped);
        }

        // Сбрасывает выбор карт
        private void ResetSelection()
        {
            firstSelectedCard = null;
            secondSelectedCard = null;
            isProcessing = false;
        }

        // Увеличивает счетчик ходов и обновляет отображение
        private void IncrementMoveCounter()
        {
            moves++;
            movesLabel.Text = $"Ходы: {moves}";
            UpdateStars();
        }

        // Подготавливает игру к перемешиванию
        private void PrepareForShuffle(Card shuffleCard, Card otherCard)
        {
            Button shuffleButton = FindButtonForCard(shuffleCard);
            Button otherButton = FindButtonForCard(otherCard);

            // Переворот shuffle карты
            if (shuffleButton != null && !shuffleCard.IsFlipped)
            {
                shuffleCard.IsFlipped = true;
                shuffleButton.BackgroundImage = shuffleCard.Image;
            }

            // Закрытие обычной карты
            if (otherButton != null && otherCard.Type == CardType.Regular)
            {
                otherCard.IsFlipped = false;
                otherButton.BackgroundImage = otherCard.GetBackImage();
            }

            // Отключение всех кнопок на время анимации
            DisableAllCardButtons();
        }

        // Выполняет перемешивание карт
        private void ExecuteShuffle(Card shuffleCard)
        {
            // Включение кнопок (если не на паузе)
            EnableAllCardButtons(!isPaused);

            // Скрытие карты перемешивания
            Button shuffleButton = FindButtonForCard(shuffleCard);
            shuffleCard.IsMatched = true;
            shuffleButton?.Hide();

            // Перемешивание и обновление игрового поля
            gameBoard.ShuffleCards();
            RecreateCardButtons();

            ResetSelection();
            CheckGameEnd();
        }

        // Обработка подсказки с найденной парной картой
        private void ProcessHintWithMatch(Card hintCard, Card otherCard, Card matchingCard)
        {
            Button matchingButton = FindButtonForCard(matchingCard);
            FlipCard(matchingCard, matchingButton);

            Timer delayTimer = new Timer { Interval = 1500 };
            delayTimer.Tick += (s, e) =>
            {
                delayTimer.Stop();
                CompleteHintWithMatch(hintCard, otherCard, matchingCard, matchingButton);
            };
            delayTimer.Start();
        }

        // Завершает обработку подсказки с найденной парой
        private void CompleteHintWithMatch(Card hintCard, Card otherCard, Card matchingCard, Button matchingButton)
        {
            otherCard.IsMatched = true;
            matchingCard.IsMatched = true;
            hintCard.IsMatched = true;

            Button firstButton = FindButtonForCard(firstSelectedCard);
            Button secondButton = FindButtonForCard(secondSelectedCard);

            if (firstButton != null) firstButton.Visible = false;
            if (secondButton != null) secondButton.Visible = false;
            if (matchingButton != null) matchingButton.Visible = false;

            ResetSelection();
            CheckGameEnd();
        }

        // Обработка простой подсказки (без найденной пары)
        private void ProcessSimpleHint(Card hintCard)
        {
            Timer simpleTimer = new Timer { Interval = 1000 };
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

        // === МЕТОДЫ УПРАВЛЕНИЯ ИГРОЙ ===

        // Пересоздает кнопки карт (например, после перемешивания)
        private void RecreateCardButtons()
        {
            InitializeCardButtons();
        }

        // Отключает все кнопки карт
        private void DisableAllCardButtons()
        {
            foreach (Button btn in gamePanel.Controls.OfType<Button>())
            {
                btn.Enabled = false;
            }
        }

        // Включает или отключает все кнопки карт
        private void EnableAllCardButtons(bool enabled)
        {
            foreach (Button btn in gamePanel.Controls.OfType<Button>())
            {
                btn.Enabled = enabled;
            }
        }

        // Проверяет завершение игры
        private void CheckGameEnd()
        {
            if (gameBoard.AllCardsMatched())
            {
                gameTimer.Stop();
                ShowGameCompletionDialog();
            }
        }

        // Показывает диалог завершения уровня
        private void ShowGameCompletionDialog()
        {
            Form completionForm = new Form
            {
                Text = "Поздравляем!",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                Size = new Size(400, 250),
                BackColor = Color.LightBlue
            };

            // Создание интерфейса диалога
            CreateCompletionDialog(completionForm);

            // Обработка результата диалога
            if (completionForm.ShowDialog() == DialogResult.OK)
            {
                StartNewGame();
            }
            else
            {
                this.Close();
            }
        }

        // Создает интерфейс диалога завершения
        private void CreateCompletionDialog(Form dialog)
        {
            TableLayoutPanel tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4
            };

            // Настройка строк
            for (int i = 0; i < 4; i++)
            {
                tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            }

            // Создание элементов диалога
            CreateDialogLabels(tableLayout);
            CreateDialogButtons(tableLayout);

            dialog.Controls.Add(tableLayout);
        }

        // Создает метки для диалога завершения
        private void CreateDialogLabels(TableLayoutPanel tableLayout)
        {
            Label congratsLabel = new Label
            {
                Text = "Уровень пройден!",
                Font = new Font("Times New Roman", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            Label statsLabel = new Label
            {
                Text = $"Ходы: {moves}\nВремя: {gameTimer.GetFormattedTime()}",
                Font = new Font("Times New Roman", 14, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            tableLayout.Controls.Add(congratsLabel, 0, 0);
            tableLayout.Controls.Add(statsLabel, 0, 1);
        }

        // Создает кнопки для диалога завершения
        private void CreateDialogButtons(TableLayoutPanel tableLayout)
        {
            Button playAgainButton = new Button
            {
                Text = "Играть ещё",
                Font = new Font("Times New Roman", 12, FontStyle.Bold),
                BackColor = Color.Gold
            };
            playAgainButton.Click += (s, e) =>
            {
                ((Form)playAgainButton.Parent.Parent).DialogResult = DialogResult.OK;
                ((Form)playAgainButton.Parent.Parent).Close();
            };

            Button menuButton = new Button
            {
                Text = "В меню",
                Font = new Font("Times New Roman", 12, FontStyle.Bold),
                BackColor = Color.LightSteelBlue
            };
            menuButton.Click += (s, e) =>
            {
                ((Form)menuButton.Parent.Parent).DialogResult = DialogResult.Cancel;
                ((Form)menuButton.Parent.Parent).Close();
            };

            tableLayout.Controls.Add(playAgainButton, 0, 2);
            tableLayout.Controls.Add(menuButton, 0, 3);
        }

        // Начинает новую игру с теми же параметрами
        private void StartNewGame()
        {
            InitializeGame();
            InitializeCardButtons();

            moves = 0;
            stars = 3;
            movesLabel.Text = "Ходы: 0";
            UpdateStars();

            gameTimer.Reset();
            gameTimer.Start();
        }

        // === МЕТОДЫ УПРАВЛЕНИЯ ПАУЗОЙ ===

        // Обработчик кнопки паузы
        private void PauseButton_Click(object sender, EventArgs e)
        {
            if (!isPaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }

        // Ставит игру на паузу
        private void PauseGame()
        {
            gameTimer.Stop();
            pauseOverlay.Visible = true;
            pauseButton.Text = "▶️ Продолжить";
            isPaused = true;
            DisableAllCardButtons();
        }

        /// Возобновляет игру после паузы
        private void ResumeGame()
        {
            gameTimer.Start();
            pauseOverlay.Visible = false;
            pauseButton.Text = "⏸️ Пауза";
            isPaused = false;
            EnableAllCardButtons(true);
        }

        /// Обработчик кнопки возврата в меню
        private async void MenuButton_Click(object sender, EventArgs e)
        {
            // Если игра не завершена, запрашиваем подтверждение
            if (!gameBoard.AllCardsMatched())
            {
                if (!ConfirmExitToMenu()) return;
            }

            // Закрытие игровой формы
            this.Close();
        }

        // Запрашивает подтверждение выхода в меню
        private bool ConfirmExitToMenu()
        {
            this.Enabled = false;

            DialogResult result = MessageBox.Show(
                "Вы точно хотите выйти в меню? Текущий прогресс будет потерян.",
                "Подтверждение выхода",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            this.Enabled = true;

            return result == DialogResult.Yes;
        }

        // === МЕТОДЫ ДЛЯ РАБОТЫ С ПРАВИЛАМИ ===

        // Ставит игру на паузу при открытии правил
        public void PauseGameForRules()
        {
            if (!isPaused)
            {
                gameTimer.Stop();
                isPaused = true;
                rulesOpenedFromGame = true;
                DisableAllCardButtons();
            }
        }

        // Возобновляет игру после закрытия правил
        public void ResumeGameAfterRules()
        {
            if (rulesOpenedFromGame)
            {
                gameTimer.Start();
                isPaused = false;
                rulesOpenedFromGame = false;
                EnableAllCardButtons(true);
                pauseButton.Text = "⏸️ Пауза";
            }
        }

        // Показывает форму правил из игры
        private void ShowRulesFromGame()
        {
            // Сохранение состояния таймера
            bool wasRunning = gameTimer.IsRunning;

            // Постановка на паузу
            gameTimer.Stop();
            isPaused = true;
            DisableAllCardButtons();

            // Показ правил
            using (RulesForm rules = new RulesForm())
            {
                rules.ShowDialog(this);
            }

            // Возобновление игры
            if (wasRunning)
            {
                gameTimer.Start();
                isPaused = false;
                EnableAllCardButtons(true);
            }
        }

        // === МЕТОДЫ ОБНОВЛЕНИЯ ИНТЕРФЕЙСА ===
        // Обновляет отображение звезд в зависимости от эффективности
        private void UpdateStars()
        {
            starsPanel.Controls.Clear();

            // Расчет эффективности
            int maxMoves = gameBoard.Rows * gameBoard.Columns * 2;
            double efficiency = (double)moves / maxMoves;

            // Определение количества звезд
            if (efficiency > 0.8) stars = 1;
            else if (efficiency > 0.6) stars = 2;
            else stars = 3;

            // Создание звезд
            CreateStars();
        }

        // Создает отображение звезд
        private void CreateStars()
        {
            for (int i = 0; i < 3; i++)
            {
                PictureBox star = new PictureBox
                {
                    Size = new Size(30, 30),
                    SizeMode = PictureBoxSizeMode.StretchImage
                };

                // Загрузка изображений звезд или создание цветных квадратов
                try
                {
                    star.Image = i < stars ?
                        Image.FromFile("img/ui/star_filled.png") :
                        Image.FromFile("img/ui/star_empty.png");
                }
                catch
                {
                    star.BackColor = i < stars ? Color.Gold : Color.Gray;
                }

                starsPanel.Controls.Add(star);
            }
        }

        // Обновляет отображение таймера
        private void UpdateTimer(int seconds)
        {
            timerLabel.Text = gameTimer.GetFormattedTime();
        }

        // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===
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
    }
}