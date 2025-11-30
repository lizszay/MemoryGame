using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MemoryGame.GameLogic;
using Timer = System.Windows.Forms.Timer;

namespace MemoryGame
{
    public partial class GameForm : Form
    {
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

        private Panel topPanel;
        private Panel rightPanel;
        private Panel gamePanel;
        private Panel pauseOverlay;
        private Label timerLabel;
        private Label movesLabel;
        private Label levelLabel;
        private FlowLayoutPanel starsPanel;
        private Button pauseButton;

        public GameForm(string theme, string level) : this(theme, level, 0, 0) { }

        public GameForm(string theme, string level, int customRows, int customColumns)
        {
            currentTheme = theme;
            currentLevel = level;
            InitializeGame(customRows, customColumns);
            InitializeForm();
        }

        private void InitializeGame(int rows = 0, int columns = 0)
        {
            if (rows == 0 || columns == 0)
            {
                (rows, columns) = LevelManager.GetLevelDimensions(currentLevel);
            }

            gameBoard = new GameBoard(rows, columns, GetThemeFolderName(currentTheme), currentLevel);
            gameTimer = new GameTimer();
            gameTimer.OnTick += UpdateTimer;
            moves = 0;
            stars = 3;
            isProcessing = false;
            isPaused = false;
        }

        private void InitializeForm()
        {
            this.Text = "Memory Game - Игра";
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.LightBlue;

            CreateTopPanel();
            CreateRightPanel();
            CreateGamePanel();
            CreatePauseOverlay();

            this.FormClosing += GameForm_FormClosing;
            gameTimer.Start();
        }

        private void CreateTopPanel()
        {
            topPanel = new Panel();
            topPanel.Height = 100;
            topPanel.Dock = DockStyle.Top;
            topPanel.BackColor = Color.DarkBlue;

            TableLayoutPanel tableLayout = new TableLayoutPanel();
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.ColumnCount = 4;
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            timerLabel = CreateInfoLabel("00:00", 20);
            tableLayout.Controls.Add(timerLabel, 0, 0);

            movesLabel = CreateInfoLabel("Ходы: 0", 20);
            tableLayout.Controls.Add(movesLabel, 1, 0);

            Panel starsContainer = new Panel();
            starsContainer.Dock = DockStyle.Fill;
            starsPanel = new FlowLayoutPanel();
            starsPanel.FlowDirection = FlowDirection.LeftToRight;
            starsPanel.Dock = DockStyle.Fill;
            starsPanel.WrapContents = false;
            UpdateStars();
            starsContainer.Controls.Add(starsPanel);
            tableLayout.Controls.Add(starsContainer, 2, 0);

            levelLabel = CreateInfoLabel($"Уровень: {currentLevel}", 18);
            tableLayout.Controls.Add(levelLabel, 3, 0);

            topPanel.Controls.Add(tableLayout);
            this.Controls.Add(topPanel);
        }

        private Label CreateInfoLabel(string text, int fontSize)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = new Font("Times New Roman", fontSize, FontStyle.Bold);
            label.ForeColor = Color.White;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Dock = DockStyle.Fill;
            return label;
        }

        private void CreateRightPanel()
        {
            rightPanel = new Panel();
            rightPanel.Width = 200;
            rightPanel.Dock = DockStyle.Right;
            rightPanel.BackColor = Color.DarkBlue;

            TableLayoutPanel buttonPanel = new TableLayoutPanel();
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.RowCount = 2;
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            pauseButton = CreateSideButton("⏸️ Пауза");
            pauseButton.Click += PauseButton_Click;

            Button menuButton = CreateSideButton("В меню");
            menuButton.Click += MenuButton_Click;

            buttonPanel.Controls.Add(pauseButton, 0, 0);
            buttonPanel.Controls.Add(menuButton, 0, 1);

            rightPanel.Controls.Add(buttonPanel);
            this.Controls.Add(rightPanel);
        }

        private Button CreateSideButton(string text)
        {
            Button button = new Button();
            button.Text = text;
            button.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            button.BackColor = Color.Gold;
            button.ForeColor = Color.DarkBlue;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 2;
            button.FlatAppearance.BorderColor = Color.White;
            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(10);
            button.Cursor = Cursors.Hand;
            return button;
        }

        private void CreateGamePanel()
        {
            gamePanel = new Panel();
            gamePanel.Dock = DockStyle.Fill;
            gamePanel.BackColor = Color.LightGreen;
            gamePanel.Padding = new Padding(20);

            InitializeCardButtons();
            this.Controls.Add(gamePanel);
        }

        private void CreatePauseOverlay()
        {
            pauseOverlay = new Panel();
            pauseOverlay.Dock = DockStyle.Fill;
            pauseOverlay.BackColor = Color.FromArgb(180, Color.Black);
            pauseOverlay.Visible = false;

            Label pauseLabel = new Label();
            pauseLabel.Text = "ИГРА НА ПАУЗЕ";
            pauseLabel.Font = new Font("Times New Roman", 48, FontStyle.Bold);
            pauseLabel.ForeColor = Color.White;
            pauseLabel.TextAlign = ContentAlignment.MiddleCenter;
            pauseLabel.Dock = DockStyle.Fill;

            pauseOverlay.Controls.Add(pauseLabel);
            this.Controls.Add(pauseOverlay);
        }

        private void InitializeCardButtons()
        {
            gamePanel.Controls.Clear();

            int cardSize = CalculateCardSize();
            int spacing = 10;
            int startX = (gamePanel.Width - (gameBoard.Columns * (cardSize + spacing))) / 2;
            int startY = (gamePanel.Height - (gameBoard.Rows * (cardSize + spacing))) / 2;

            for (int i = 0; i < gameBoard.Cards.Count; i++)
            {
                int row = i / gameBoard.Columns;
                int col = i % gameBoard.Columns;

                Button cardButton = new Button();
                cardButton.Size = new Size(cardSize, cardSize);
                cardButton.Location = new Point(startX + col * (cardSize + spacing),
                                              startY + row * (cardSize + spacing));
                cardButton.Tag = gameBoard.Cards[i];

                if (gameBoard.Cards[i].IsMatched)
                {
                    cardButton.Visible = false;
                }
                else
                {
                    cardButton.BackgroundImage = gameBoard.Cards[i].IsFlipped ?
                        gameBoard.Cards[i].Image : gameBoard.Cards[i].GetBackImage();
                }

                cardButton.BackgroundImageLayout = ImageLayout.Stretch;
                cardButton.FlatStyle = FlatStyle.Flat;
                cardButton.FlatAppearance.BorderSize = 2;
                cardButton.FlatAppearance.BorderColor = Color.DarkBlue;
                cardButton.Cursor = Cursors.Hand;

                cardButton.Click += CardButton_Click;
                gamePanel.Controls.Add(cardButton);
            }
        }

        private int CalculateCardSize()
        {
            int maxWidth = (gamePanel.Width - 40) / gameBoard.Columns - 10;
            int maxHeight = (gamePanel.Height - 40) / gameBoard.Rows - 10;
            return Math.Min(maxWidth, maxHeight);
        }

        private void CardButton_Click(object sender, EventArgs e)
        {
            if (isProcessing || isPaused) return;

            Button clickedButton = (Button)sender;
            Card clickedCard = (Card)clickedButton.Tag;

            if (clickedCard.IsFlipped || clickedCard.IsMatched) return;

            // 🔥 Проверка: если это карта Shuffle — обрабатываем мгновенно
            if (clickedCard.Type == CardType.Shuffle)
            {
                // Переворачиваем карту, чтобы игрок увидел
                clickedCard.IsFlipped = true;
                clickedButton.BackgroundImage = clickedCard.Image;

                // Запускаем задержку → потом перемешиваем
                var timer = new Timer { Interval = 1000 };
                timer.Tick += (s, ev) =>
                {
                    timer.Stop();

                    // Убираем карту
                    clickedCard.IsMatched = true;
                    clickedButton.Visible = false;

                    // Перемешиваем и обновляем поле
                    gameBoard.ShuffleCards();
                    RecreateCardButtons();

                    // Не увеличиваем moves, не вызываем ResetSelection() — просто сброс
                    firstSelectedCard = null;
                    secondSelectedCard = null;
                    isProcessing = false;
                };
                timer.Start();

                return; // ❗ Важно: выходим, не продолжаем логику обычного хода
            }

            // === Обычная логика для НЕ-спецкарт ===
            FlipCard(clickedCard, clickedButton);

            if (firstSelectedCard == null)
            {
                firstSelectedCard = clickedCard;
            }
            else
            {
                // Вторая карта — проверим, не является ли она Hint или Shuffle
                if (clickedCard.Type == CardType.Hint)
                {
                    // Обработка Hint как обычного хода (с ходом)
                    secondSelectedCard = clickedCard;
                    moves++;
                    movesLabel.Text = $"Ходы: {moves}";
                    UpdateStars();
                    isProcessing = true;
                    ProcessTurn();
                }
                else
                {
                    // Обычный ход (Regular ↔ Regular)
                    secondSelectedCard = clickedCard;
                    moves++;
                    movesLabel.Text = $"Ходы: {moves}";
                    UpdateStars();
                    isProcessing = true;
                    ProcessTurn();
                }
            }
        }

        private void FlipCard(Card card, Button button)
        {
            card.IsFlipped = true;
            button.BackgroundImage = card.Image;
        }

        private void ProcessTurn()
        {
            // Теперь сюда попадают только Hint или Regular карты
            if (firstSelectedCard.Type == CardType.Hint || secondSelectedCard.Type == CardType.Hint)
            {
                ProcessHintCard();
            }
            else if (firstSelectedCard.Id == secondSelectedCard.Id && firstSelectedCard.Type == CardType.Regular)
            {
                ProcessMatch();
            }
            else
            {
                ProcessMismatch();
            }
        }

        private void ProcessHintCard()
        {
            Card hintCard = firstSelectedCard.Type == CardType.Hint ? firstSelectedCard : secondSelectedCard;
            Card otherCard = firstSelectedCard.Type == CardType.Hint ? secondSelectedCard : firstSelectedCard;

            if (otherCard.Type == CardType.Regular)
            {
                Card matchingCard = gameBoard.Cards.FirstOrDefault(c =>
                    c.Id == otherCard.Id && c != otherCard && !c.IsMatched && !c.IsFlipped);

                if (matchingCard != null)
                {
                    Button matchingButton = FindButtonForCard(matchingCard);
                    FlipCard(matchingCard, matchingButton);

                    Timer delayTimer = new Timer();
                    delayTimer.Interval = 1500;
                    delayTimer.Tick += (s, e) =>
                    {
                        delayTimer.Stop();
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
                    };
                    delayTimer.Start();
                    return;
                }
            }

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

        private void ProcessShuffleCard()
        {
            // Определяем, какая из двух карт — Shuffle
            Card shuffleCard = firstSelectedCard.Type == CardType.Shuffle ? firstSelectedCard : secondSelectedCard;
            Button shuffleButton = FindButtonForCard(shuffleCard);

            // Убеждаемся, что карта перевёрнута (на случай, если вторая не была показана)
            if (shuffleButton != null && !shuffleCard.IsFlipped)
            {
                shuffleCard.IsFlipped = true;
                shuffleButton.BackgroundImage = shuffleCard.Image;
            }

            // Временная задержка, чтобы игрок увидел карту
            var delayTimer = new Timer { Interval = 1000 }; // 1 секунда
            delayTimer.Tick += (s, e) =>
            {
                delayTimer.Stop();

                // Убираем карту Shuffle
                shuffleCard.IsMatched = true;
                shuffleButton?.Hide(); // или Visible = false

                // Перемешиваем оставшиеся НЕСОПОСТАВЛЕННЫЕ карты
                gameBoard.ShuffleCards();

                // Сбрасываем выбор и обновляем поле
                ResetSelection();
                RecreateCardButtons();
            };

            delayTimer.Start();
        }

        private void ProcessMatch()
        {
            firstSelectedCard.IsMatched = true;
            secondSelectedCard.IsMatched = true;

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

        private void ProcessMismatch()
        {
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

        private void ResetSelection()
        {
            firstSelectedCard = null;
            secondSelectedCard = null;
            isProcessing = false;
        }

        private void RecreateCardButtons()
        {
            InitializeCardButtons();
        }

        private void CheckGameEnd()
        {
            if (gameBoard.AllCardsMatched())
            {
                gameTimer.Stop();
                ShowGameCompletionDialog();
            }
        }

        private void ShowGameCompletionDialog()
        {
            Form completionForm = new Form();
            completionForm.Text = "Поздравляем!";
            completionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            completionForm.StartPosition = FormStartPosition.CenterScreen;
            completionForm.Size = new Size(400, 250);
            completionForm.BackColor = Color.LightBlue;

            TableLayoutPanel tableLayout = new TableLayoutPanel();
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.RowCount = 4;
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

            Label congratsLabel = new Label();
            congratsLabel.Text = "Уровень пройден!";
            congratsLabel.Font = new Font("Times New Roman", 18, FontStyle.Bold);
            congratsLabel.TextAlign = ContentAlignment.MiddleCenter;
            congratsLabel.Dock = DockStyle.Fill;

            Label statsLabel = new Label();
            statsLabel.Text = $"Ходы: {moves}\nВремя: {gameTimer.GetFormattedTime()}";
            statsLabel.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            statsLabel.TextAlign = ContentAlignment.MiddleCenter;
            statsLabel.Dock = DockStyle.Fill;

            Button playAgainButton = new Button();
            playAgainButton.Text = "Играть ещё";
            playAgainButton.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            playAgainButton.BackColor = Color.Gold;
            playAgainButton.Click += (s, e) =>
            {
                completionForm.DialogResult = DialogResult.OK;
                completionForm.Close();
                StartNewGame();
            };

            Button menuButton = new Button();
            menuButton.Text = "В меню";
            menuButton.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            menuButton.BackColor = Color.LightSteelBlue;
            menuButton.Click += (s, e) =>
            {
                completionForm.DialogResult = DialogResult.Cancel;
                completionForm.Close();
                this.Close();
            };

            tableLayout.Controls.Add(congratsLabel, 0, 0);
            tableLayout.Controls.Add(statsLabel, 0, 1);
            tableLayout.Controls.Add(playAgainButton, 0, 2);
            tableLayout.Controls.Add(menuButton, 0, 3);

            completionForm.Controls.Add(tableLayout);

            if (completionForm.ShowDialog() == DialogResult.OK)
            {
                // Играем заново
            }
            else
            {
                this.Close();
            }
        }

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

        private void PauseButton_Click(object sender, EventArgs e)
        {
            if (!isPaused)
            {
                gameTimer.Stop();
                pauseOverlay.Visible = true;
                pauseButton.Text = "▶️ Продолжить";
                isPaused = true;

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
                gameTimer.Start();
                pauseOverlay.Visible = false;
                pauseButton.Text = "⏸️ Пауза";
                isPaused = false;

                foreach (Control control in gamePanel.Controls)
                {
                    if (control is Button button)
                    {
                        button.Enabled = true;
                    }
                }
            }
        }

        private void MenuButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UpdateStars()
        {
            starsPanel.Controls.Clear();

            int maxMoves = gameBoard.Rows * gameBoard.Columns * 2;
            double efficiency = (double)moves / maxMoves;

            if (efficiency > 0.8) stars = 1;
            else if (efficiency > 0.6) stars = 2;
            else stars = 3;

            for (int i = 0; i < 3; i++)
            {
                PictureBox star = new PictureBox();
                star.Size = new Size(30, 30);
                star.SizeMode = PictureBoxSizeMode.StretchImage;
                try
                {
                    star.Image = i < stars ?
                        Image.FromFile("img/ui/star_filled.png") :
                        Image.FromFile("img/ui/star_empty.png");
                }
                catch
                {
                    // Если изображения нет, используем цветные квадраты
                    star.BackColor = i < stars ? Color.Gold : Color.Gray;
                }
                starsPanel.Controls.Add(star);
            }
        }

        private void UpdateTimer(int seconds)
        {
            timerLabel.Text = gameTimer.GetFormattedTime();
        }

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

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !gameBoard.AllCardsMatched())
            {
                DialogResult result = MessageBox.Show("Вы точно хотите выйти из игры? Текущий прогресс будет потерян.",
                    "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (gamePanel != null)
            {
                InitializeCardButtons();
            }
        }
    }
}