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
        private GameBoard gameBoard = null!;
        private GameTimer gameTimer = null!;
        private int moves;
        private int stars;
        private Card? firstSelectedCard;
        private Card? secondSelectedCard;
        private bool isProcessing;
        private bool isPaused;
        private string currentTheme = null!;
        private string currentLevel = null!;

        // Конструктор для стандартных уровней, вызываетс ядруго конструктор
        public GameForm(string theme, string level) : this(theme, level, 0, 0) { }

        // Основной конструктор игры
        public GameForm(string theme, string level, int customRows, int customColumns)
        {
            currentTheme = theme;
            currentLevel = level;

            // Инициализация игровой логики
            InitializeGame(customRows, customColumns);

            // Инициализация пользовательского интерфейса
            InitializeComponent();

            // Запуск таймера после инициализации компонентов
            gameTimer.Start();
        }


        // Инициализирует игровую логику с заданными параметрами
        private void InitializeGame(int rows = 0, int columns = 0)
        {
            // Получение размеров игрового поля
            if (rows == 0 || columns == 0)
            {
                (rows, columns) = LevelManager.GetLevelDimensions(currentLevel);
            }

            // 👇 ОСТАНОВКА И ОТПИСКА ОТ СТАРОГО ТАЙМЕРА
            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer.OnTick -= UpdateTimer; // ← ВАЖНО: отписка!
                                                 // Если GameTimer IDisposable — вызовите Dispose()
            }

            gameBoard = new GameBoard(rows, columns, GetThemeFolderName(currentTheme), currentLevel);

            // Создание НОВОГО таймера
            gameTimer = new GameTimer();
            gameTimer.OnTick += UpdateTimer; // ← новая подписка

            ResetGameState();
        }

        // Сбрасывает состояние игры к начальным значениям
        private void ResetGameState()
        {
            moves = 0;
            stars = 3;
            firstSelectedCard = null;
            secondSelectedCard = null;
            isProcessing = false;
            isPaused = false;
        }

        // === ОСНОВНЫЕ МЕТОДЫ ИГРОВОГО ПРОЦЕССА ===

        // Обработчик клика по карте
        private void CardButton_Click(object sender, EventArgs e)
        {
            // Проверка возможности обработки клика
            if (!CanProcessCardClick()) return;

            // Получение карты из кнопки
            System.Windows.Forms.Button clickedButton = (System.Windows.Forms.Button)sender;
            Card clickedCard = (Card)clickedButton.Tag!;

            // Проверка состояния карты
            if (!IsCardClickable(clickedCard)) return;

            // Обработка выбора карты
            ProcessCardSelection(clickedCard, clickedButton);
        }

        // Обработка выбора карты
        private void ProcessCardSelection(Card card, System.Windows.Forms.Button button)
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
            if (firstSelectedCard!.Id == secondSelectedCard!.Id)
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
            firstSelectedCard!.IsMatched = true;
            secondSelectedCard!.IsMatched = true;

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
            Card hintCard = firstSelectedCard!.Type == CardType.Hint ? firstSelectedCard : secondSelectedCard!;
            Card otherCard = firstSelectedCard!.Type == CardType.Hint ? secondSelectedCard! : firstSelectedCard;

            // Поиск парной карты для обычной карты
            if (otherCard.Type == CardType.Regular)
            {
                Card? matchingCard = FindMatchingCard(otherCard);
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
            Card shuffleCard = firstSelectedCard!.Type == CardType.Shuffle ? firstSelectedCard : secondSelectedCard!;
            Card otherCard = firstSelectedCard!.Type == CardType.Shuffle ? secondSelectedCard! : firstSelectedCard;

            // Блокируем все карты на время анимации
            EnableAllCardButtons(false);

            // Если вторая карта обычная - переворачиваем ее обратно
            if (otherCard.Type == CardType.Regular)
            {
                System.Windows.Forms.Button? otherButton = FindButtonForCard(otherCard);
                if (otherButton != null)
                {
                    otherCard.IsFlipped = false;
                    otherButton.BackgroundImage = otherCard.GetBackImage();
                }
            }

            // Показываем карту перемешивания 1.5 секунды, затем перемешиваем
            Timer shuffleTimer = new Timer { Interval = 1500 };
            shuffleTimer.Tick += (s, e) =>
            {
                shuffleTimer.Stop();

                // Скрываем карту перемешивания
                System.Windows.Forms.Button? shuffleButton = FindButtonForCard(shuffleCard);
                if (shuffleButton != null)
                {
                    shuffleCard.IsMatched = true;
                    shuffleButton.Visible = false;
                }

                // Перемешиваем карты
                gameBoard.ShuffleCards();

                // Пересоздаем кнопки карт
                InitializeCardButtons();

                // Включаем карты (если не на паузе)
                EnableAllCardButtons(!isPaused);

                // Сбрасываем выбор
                ResetSelection();

                // Проверяем завершение игры
                CheckGameEnd();
            };

            shuffleTimer.Start();
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
        private void FlipCard(Card card, System.Windows.Forms.Button button)
        {
            card.IsFlipped = true;
            button.BackgroundImage = card.Image;
        }

        // Переворачивает карты обратно рубашкой вверх
        private void FlipCardsBack()
        {
            firstSelectedCard!.IsFlipped = false;
            secondSelectedCard!.IsFlipped = false;

            System.Windows.Forms.Button? firstButton = FindButtonForCard(firstSelectedCard);
            System.Windows.Forms.Button? secondButton = FindButtonForCard(secondSelectedCard);

            if (firstButton != null) firstButton.BackgroundImage = firstSelectedCard.GetBackImage();
            if (secondButton != null) secondButton.BackgroundImage = secondSelectedCard.GetBackImage();
        }

        // Скрывает совпавшие карты
        private void HideMatchedCards()
        {
            System.Windows.Forms.Button? firstButton = FindButtonForCard(firstSelectedCard!);
            System.Windows.Forms.Button? secondButton = FindButtonForCard(secondSelectedCard!);

            if (firstButton != null) firstButton.Visible = false;
            if (secondButton != null) secondButton.Visible = false;
        }

        // Находит кнопку, связанную с указанной картой
        private System.Windows.Forms.Button? FindButtonForCard(Card card)
        {
            foreach (Control control in gamePanel.Controls)
            {
                if (control is System.Windows.Forms.Button button && button.Tag as Card == card)
                {
                    return button;
                }
            }
            return null;
        }

        // Находит парную карту для указанной карты
        private Card? FindMatchingCard(Card card)
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
            UpdateMoveLabel();
            UpdateStars();
        }

        // Подготавливает игру к перемешиванию
        private void PrepareForShuffle(Card shuffleCard, Card otherCard)
        {
            System.Windows.Forms.Button? shuffleButton = FindButtonForCard(shuffleCard);
            System.Windows.Forms.Button? otherButton = FindButtonForCard(otherCard);

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
            EnableAllCardButtons(false);
        }

        // Выполняет перемешивание карт
        private void ExecuteShuffle(Card shuffleCard)
        {
            // Включение кнопок (если не на паузе)
            EnableAllCardButtons(!isPaused);

            // Скрытие карты перемешивания
            System.Windows.Forms.Button? shuffleButton = FindButtonForCard(shuffleCard);
            shuffleCard.IsMatched = true;
            shuffleButton?.Hide();

            // Перемешивание и обновление игрового поля
            gameBoard.ShuffleCards();
            InitializeCardButtons();

            ResetSelection();
            CheckGameEnd();
        }

        // Обработка подсказки с найденной парной картой
        private void ProcessHintWithMatch(Card hintCard, Card otherCard, Card matchingCard)
        {
            System.Windows.Forms.Button? matchingButton = FindButtonForCard(matchingCard);
            if (matchingButton != null)
            {
                FlipCard(matchingCard, matchingButton);
            }

            Timer delayTimer = new Timer { Interval = 1000 };
            delayTimer.Tick += (s, e) =>
            {
                delayTimer.Stop();
                CompleteHintWithMatch(hintCard, otherCard, matchingCard, matchingButton);
            };
            delayTimer.Start();
        }

        // Завершает обработку подсказки с найденной парой
        private void CompleteHintWithMatch(Card hintCard, Card otherCard, Card matchingCard, System.Windows.Forms.Button? matchingButton)
        {
            otherCard.IsMatched = true;
            matchingCard.IsMatched = true;
            hintCard.IsMatched = true;

            System.Windows.Forms.Button? firstButton = FindButtonForCard(firstSelectedCard!);
            System.Windows.Forms.Button? secondButton = FindButtonForCard(secondSelectedCard!);

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

                System.Windows.Forms.Button? hintButton = FindButtonForCard(hintCard);
                if (hintButton != null) hintButton.Visible = false;

                ResetSelection();
                CheckGameEnd();
            };
            simpleTimer.Start();
        }

        // === МЕТОДЫ УПРАВЛЕНИЯ ИГРОЙ ===

        // Включает или отключает все кнопки карт
        private void EnableAllCardButtons(bool enabled)
        {
            if (gamePanel == null || gamePanel.IsDisposed) return;

            // Если вызываем из другого потока
            if (gamePanel.InvokeRequired)
            {
                gamePanel.Invoke(new Action<bool>(EnableAllCardButtons), enabled);
                return;
            }

            foreach (Control control in gamePanel.Controls)
            {
                if (control is System.Windows.Forms.Button button)
                {
                    button.Enabled = enabled;
                }
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
            // Останавливаем таймер
            gameTimer.Stop();

            // Создаем и показываем форму результатов
            using (GameResultForm resultForm = new GameResultForm(
                stars: stars,
                moves: moves,
                elapsedSeconds: gameTimer.ElapsedSeconds,
                levelName: currentLevel,
                rows: gameBoard.Rows,
                columns: gameBoard.Columns))
            {
                // Показываем как диалоговое окно
                if (resultForm.ShowDialog(this) == DialogResult.OK)
                {
                    // Играть снова
                    StartNewGame();
                }
                else
                {
                    // Выход в меню
                    this.Close();
                }
            }
        }

        // Метод для расчета эффективности в процентах
        private double CalculateEfficiency()
        {
            if (gameBoard == null || gameTimer == null) return 0;

            int totalCards = gameBoard.Rows * gameBoard.Columns;
            int pairs = totalCards / 2;

            if (pairs == 0 || totalCards == 0) return 0;

            double movesPerPair = (double)moves / pairs;
            double timePerCard = (double)gameTimer.ElapsedSeconds / totalCards;

            // Идеальные значения
            double idealMovesPerPair = 1.2;
            double idealTimePerCard = 4.0;

            // Эффективность в процентах (чем ближе к идеалу, тем выше)
            double movesEfficiency = Math.Max(0, 100 - (movesPerPair - idealMovesPerPair) * 25);
            double timeEfficiency = Math.Max(0, 100 - (timePerCard - idealTimePerCard) * 10);

            return (movesEfficiency * 0.6 + timeEfficiency * 0.4);
        }

        // Начинает новую игру с теми же параметрами
        private void StartNewGame()
        {
            // Сохраняем текущие размеры доски и тему
            int rows = gameBoard.Rows;
            int columns = gameBoard.Columns;
            string theme = currentTheme;
            string level = currentLevel;

            // Переинициализируем игровую логику
            if (level.ToLower() == "пользовательский")
            {
                // Для пользовательского уровня используем сохраненные размеры
                InitializeGame(rows, columns);
            }
            else
            {
                // Для стандартных уровней используем дефолтные размеры
                InitializeGame();
            }

            // Инициализируем кнопки карт
            InitializeCardButtons();

            // Сбрасываем счетчики
            moves = 0;
            stars = 3;
            UpdateMoveLabel();
            UpdateStars();

            // Перезапускаем таймер
            gameTimer.Reset();
            gameTimer.Start();
        }

        // Перезапускает игру для пользовательского уровня
        private void RestartGameWithCustomLevel(int rows, int columns)
        {
            // Сохраняем тему
            string theme = currentTheme;

            // Закрываем текущую форму
            this.Close();

            // Создаем новую форму с теми же параметрами
            GameForm newGameForm = new GameForm(theme, "пользовательский", rows, columns);
            newGameForm.Show();
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
            EnableAllCardButtons(false);
        }

        // Возобновляет игру после паузы
        private void ResumeGame()
        {
            gameTimer.Start();
            pauseOverlay.Visible = false;
            pauseButton.Text = "⏸️ Пауза";
            isPaused = false;
            EnableAllCardButtons(true);
        }

        // Обработчик кнопки возврата в меню
        private void MenuButton_Click(object sender, EventArgs e)
        {
            // Немедленно ставим игру на паузу — НОВОЕ ПОВЕДЕНИЕ
            if (!isPaused)
            {
                PauseGame(); // Останавливает таймер и блокирует карты
            }

            // Если игра не завершена, запрашиваем подтверждение
            if (!gameBoard.AllCardsMatched())
            {
                if (!ConfirmExitToMenu())
                {
                    // Если пользователь отменил выход — возобновляем игру
                    ResumeGame();
                    return;
                }
            }

            // Закрытие игровой формы
            this.Close();
        }

        // Запрашивает подтверждение выхода в меню
        private bool ConfirmExitToMenu()
        {
            this.Enabled = false;

            DialogResult result = MessageBox.Show(
                "Вы точно хотите выйти в меню?",
                "Подтверждение выхода",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            this.Enabled = true;

            return result == DialogResult.Yes;
        }

        // Показывает форму правил из игры
        private void ShowRulesFromGame()
        {
            // Сохраняем состояние игры
            bool wasRunning = gameTimer.IsRunning;

            // Ставим на паузу если игра была запущена
            if (wasRunning && !isPaused)
            {
                PauseGame();
            }

            // Показываем правила
            using (RulesForm rules = new RulesForm())
            {
                rules.ShowDialog(this);
            }

            // Возобновляем игру ТОЛЬКО если она была запущена и сейчас на паузе
            if (wasRunning && isPaused)
            {
                ResumeGame();
            }
        }

        // === МЕТОДЫ ОБНОВЛЕНИЯ ИНТЕРФЕЙСА ===

        // Обновляет отображение звезд в зависимости от эффективности
        private void UpdateStars()
        {
            if (starsPanel == null || gameBoard == null)
                return;

            starsPanel.Controls.Clear();

            int maxMoves = gameBoard.Rows * gameBoard.Columns * 2;
            double efficiency = maxMoves > 0 ? (double)moves / maxMoves : 0;

            // ⭐⭐⭐ ПРОСТЫЕ КРИТЕРИИ ПО ХОДАМ ⭐⭐⭐
            if (efficiency > 1.0) // Более 100% использованных ходов = 0 звезд
            {
                stars = 0;
            }
            else if (efficiency > 0.85) // 85-100% = 1 звезда
            {
                stars = 1;
            }
            else if (efficiency > 0.70) // 70-85% = 2 звезды
            {
                stars = 2;
            }
            else // <70% = 3 звезды
            {
                stars = 3;
            }

            // Создание звезд
            for (int i = 0; i < 3; i++)
            {
                PictureBox star = new PictureBox
                {
                    Size = new Size(80, 80),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Margin = new Padding(5, 0, 5, 0)
                };

                try
                {
                    if (i < stars)
                    {
                        star.Image = Image.FromFile("img/ui/star_filled.png");
                    }
                    else if (stars == 0)
                    {
                        star.Image = Image.FromFile("img/ui/star_empty.png");
                        star.BackColor = Color.DarkGray;
                    }
                    else
                    {
                        star.Image = Image.FromFile("img/ui/star_empty.png");
                    }
                }
                catch
                {
                    if (i < stars)
                    {
                        star.BackColor = Color.Gold;
                    }
                    else if (stars == 0)
                    {
                        star.BackColor = Color.DarkGray;
                    }
                    else
                    {
                        star.BackColor = Color.LightGray;
                    }
                }

                starsPanel.Controls.Add(star);
            }
        }

        // Рассчитывает максимальное время для текущего уровня
        private int CalculateMaxTimeForLevel()
        {
            if (gameBoard == null) return 180;

            int totalCards = gameBoard.Rows * gameBoard.Columns;

            // ⭐⭐⭐ БОЛЕЕ КОРОТКИЕ ВРЕМЕННЫЕ ЛИМИТЫ ⭐⭐⭐
            switch (currentLevel.ToLower())
            {
                case "легкий":    // 2x2 = 4 карты
                    return 90;    // 1.5 минуты (было 2)
                case "средний":   // 3x3 = 9 карт
                    return 135;   // 2.25 минуты (было 3)
                case "сложный":   // 4x4 = 16 карт
                    return 180;   // 3 минуты (было 4)
                case "эксперт":   // 5x5 = 25 карт
                    return 225;   // 3.75 минуты (было 5)
                case "пользовательский":
                    // Формула: 8 секунд на карту + 30 секунд базовое время (было 10+60)
                    return totalCards * 8 + 30;
                default:
                    return 135;   // По умолчанию 2.25 минуты
            }
        }

        // Обновляет отображение таймера
        private void UpdateTimer(int seconds)
        {
            if (timerLabel.InvokeRequired)
            {
                timerLabel.Invoke(new Action<int>(UpdateTimer), seconds);
            }
            else
            {
                timerLabel.Text = gameTimer.GetFormattedTime();

                // ⭐⭐⭐ ОБНОВЛЯЕМ ЗВЕЗДЫ КАЖДУЮ СЕКУНДУ ⭐⭐⭐
                // Обновляем звезды каждые 10 секунд или при изменении минут
                if (seconds % 10 == 0 || seconds % 60 == 0)
                {
                    UpdateStars();
                }
            }
        }

        // Обновляет отображение счетчика ходов
        private void UpdateMoveLabel()
        {
            if (movesLabel.InvokeRequired)
            {
                movesLabel.Invoke(new Action(UpdateMoveLabel));
            }
            else
            {
                movesLabel.Text = $"Ходы: {moves}";
            }
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