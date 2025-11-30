using System;
using System.Drawing;
using System.Windows.Forms;

namespace MemoryGame
{
    public partial class MainMenuForm : Form
    {
        private string selectedTheme = "Животные";
        private Panel mainPanel;

        public MainMenuForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.LightBlue;
            this.Font = new Font("Times New Roman", 14, FontStyle.Regular);

            // Главная панель с фоном
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.Transparent;

            try
            {
                string bgPath = System.IO.Path.Combine(Application.StartupPath, "img", "ui", "background.jpg");
                if (System.IO.File.Exists(bgPath))
                {
                    mainPanel.BackgroundImage = Image.FromFile(bgPath);
                    mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch (Exception ex)
            {
                // Можно оставить без фона, если не найден
            }

            CreateMenuInterface();
            this.Controls.Add(mainPanel);

            this.FormClosing += MainMenuForm_FormClosing;
        }

        private void CreateMenuInterface()
        {
            mainPanel.Controls.Clear();

            TableLayoutPanel mainTable = new TableLayoutPanel();
            mainTable.Dock = DockStyle.Fill;
            mainTable.RowCount = 4;
            mainTable.ColumnCount = 1;
            mainTable.BackColor = Color.Transparent;

            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 15f)); // Заголовок
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 25f)); // Темы
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 40f)); // Уровни
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 20f)); // Нижние кнопки

            // === 1. ЗАГОЛОВОК ===
            Label titleLabel = new Label();
            titleLabel.Text = "MEMORY GAME";
            titleLabel.Font = new Font("Times New Roman", 52, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(255, 255, 255); // Белый текст на зелёном
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Dock = DockStyle.Fill;
            titleLabel.BackColor = Color.Transparent;
            titleLabel.Margin = new Padding(10);
            mainTable.Controls.Add(titleLabel, 0, 0);

            // === 2. ВЫБОР ТЕМЫ — ЦЕНТРИРОВАННЫЕ РАДИОКНОПКИ ===
            Panel themePanel = new Panel();
            themePanel.Dock = DockStyle.Fill;
            themePanel.BackColor = Color.Transparent;
            themePanel.Margin = new Padding(20);
            themePanel.Padding = new Padding(10);

            Label themeLabel = new Label();
            themeLabel.Text = "ВЫБЕРИТЕ ТЕМУ КАРТОЧЕК:";
            themeLabel.Font = new Font("Times New Roman", 24, FontStyle.Bold);
            themeLabel.ForeColor = Color.FromArgb(255, 255, 255); // Белый текст
            themeLabel.TextAlign = ContentAlignment.MiddleCenter;
            themeLabel.Dock = DockStyle.Top;
            themeLabel.Height = 60;
            themeLabel.BackColor = Color.Transparent;

            FlowLayoutPanel themeFlow = new FlowLayoutPanel();
            themeFlow.Dock = DockStyle.Fill;
            themeFlow.FlowDirection = FlowDirection.LeftToRight;
            themeFlow.WrapContents = false; // ← Не переносим на новую строку
            themeFlow.AutoSize = false;
            themeFlow.BackColor = Color.Transparent;
            themeFlow.Margin = new Padding(50, 10, 50, 10);

            // Центрируем элементы внутри FlowLayoutPanel
            themeFlow.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            themeFlow.AutoScroll = false;

            RadioButton rbAnimals = CreateThemeRadioButton("🐾 ЖИВОТНЫЕ", "Животные", true, Color.FromArgb(255, 200, 80)); // Оранжевый
            RadioButton rbGeometry = CreateThemeRadioButton("🔷 ГЕОМЕТРИЯ", "Геометрические фигуры", false, Color.FromArgb(100, 150, 255)); // Синий
            RadioButton rbPlants = CreateThemeRadioButton("🌿 РАСТЕНИЯ", "Растения", false, Color.FromArgb(150, 255, 100)); // Зелёный

            themeFlow.Controls.Add(rbAnimals);
            themeFlow.Controls.Add(rbGeometry);
            themeFlow.Controls.Add(rbPlants);

            themePanel.Controls.Add(themeFlow);
            themePanel.Controls.Add(themeLabel);
            mainTable.Controls.Add(themePanel, 0, 1);

            // === 3. ВЫБОР УРОВНЯ — КРАСИВЫЕ КНОПКИ ===
            Panel levelPanel = new Panel();
            levelPanel.Dock = DockStyle.Fill;
            levelPanel.BackColor = Color.Transparent;
            levelPanel.Margin = new Padding(20);
            levelPanel.Padding = new Padding(10);

            Label levelLabel = new Label();
            levelLabel.Text = "ВЫБЕРИТЕ УРОВЕНЬ СЛОЖНОСТИ:";
            levelLabel.Font = new Font("Times New Roman", 24, FontStyle.Bold);
            levelLabel.ForeColor = Color.FromArgb(255, 255, 255); // Белый текст
            levelLabel.TextAlign = ContentAlignment.MiddleCenter;
            levelLabel.Dock = DockStyle.Top;
            levelLabel.Height = 60;
            levelLabel.BackColor = Color.Transparent;

            TableLayoutPanel levelTable = new TableLayoutPanel();
            levelTable.Dock = DockStyle.Fill;
            levelTable.RowCount = 5;
            levelTable.ColumnCount = 1;
            levelTable.BackColor = Color.Transparent;
            levelTable.Padding = new Padding(100, 10, 100, 10);

            for (int i = 0; i < 5; i++)
            {
                levelTable.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
            }

            // Разные цвета + контрастный текст
            Button btnEasy = CreateLevelButton("⭐ ЛЕГКИЙ (2×2)", "Легкий", Color.FromArgb(255, 200, 80), Color.DarkBlue);
            Button btnMedium = CreateLevelButton("⭐⭐ СРЕДНИЙ (3×3)", "Средний", Color.FromArgb(255, 170, 50), Color.DarkBlue);
            Button btnHard = CreateLevelButton("⭐⭐⭐ СЛОЖНЫЙ (4×4)", "Сложный", Color.FromArgb(255, 140, 30), Color.White);
            Button btnExpert = CreateLevelButton("⭐⭐⭐⭐ ЭКСПЕРТ (5×5)", "Эксперт", Color.FromArgb(255, 110, 10), Color.White);
            Button btnCustom = CreateLevelButton("🎮 ПОЛЬЗОВАТЕЛЬСКИЙ", "Пользовательский", Color.FromArgb(200, 150, 255), Color.DarkBlue);

            levelTable.Controls.Add(btnEasy, 0, 0);
            levelTable.Controls.Add(btnMedium, 0, 1);
            levelTable.Controls.Add(btnHard, 0, 2);
            levelTable.Controls.Add(btnExpert, 0, 3);
            levelTable.Controls.Add(btnCustom, 0, 4);

            levelPanel.Controls.Add(levelTable);
            levelPanel.Controls.Add(levelLabel);
            mainTable.Controls.Add(levelPanel, 0, 2);

            // === 4. НИЖНИЕ КНОПКИ ===
            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Fill;
            bottomPanel.BackColor = Color.Transparent;
            bottomPanel.Margin = new Padding(20);
            bottomPanel.Padding = new Padding(20);

            TableLayoutPanel buttonTable = new TableLayoutPanel();
            buttonTable.Dock = DockStyle.Fill;
            buttonTable.RowCount = 1;
            buttonTable.ColumnCount = 3;
            buttonTable.BackColor = Color.Transparent;

            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));

            // Кнопка правил слева
            Button btnRules = new Button();
            btnRules.Text = "📖 ПРАВИЛА ИГРЫ";
            btnRules.Font = new Font("Times New Roman", 16, FontStyle.Bold);
            btnRules.BackColor = Color.LightSteelBlue;
            btnRules.ForeColor = Color.DarkBlue;
            btnRules.FlatStyle = FlatStyle.Flat;
            btnRules.FlatAppearance.BorderSize = 3;
            btnRules.FlatAppearance.BorderColor = Color.DarkBlue;
            btnRules.Dock = DockStyle.Fill;
            btnRules.Margin = new Padding(20, 10, 10, 10);
            btnRules.Cursor = Cursors.Hand;
            btnRules.Click += (s, e) => ShowRules();

            // Пустое место в центре
            Panel spacer = new Panel();
            spacer.Dock = DockStyle.Fill;
            spacer.BackColor = Color.Transparent;

            // Кнопка выхода справа
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

            buttonTable.Controls.Add(btnRules, 0, 0);
            buttonTable.Controls.Add(spacer, 1, 0);
            buttonTable.Controls.Add(btnExit, 2, 0);

            bottomPanel.Controls.Add(buttonTable);
            mainTable.Controls.Add(bottomPanel, 0, 3);

            mainPanel.Controls.Add(mainTable);
        }

        private Panel CreateThemePanel()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.FromArgb(200, Color.White);
            panel.Margin = new Padding(10);
            panel.Padding = new Padding(10);

            Label themeLabel = new Label();
            themeLabel.Text = "ВЫБЕРИТЕ ТЕМУ КАРТОЧЕК:";
            themeLabel.Font = new Font("Times New Roman", 24, FontStyle.Bold);
            themeLabel.ForeColor = Color.DarkBlue;
            themeLabel.TextAlign = ContentAlignment.MiddleCenter;
            themeLabel.Dock = DockStyle.Top;
            themeLabel.Height = 60;
            themeLabel.BackColor = Color.Transparent;

            FlowLayoutPanel themeFlow = new FlowLayoutPanel();
            themeFlow.Dock = DockStyle.Fill;
            themeFlow.FlowDirection = FlowDirection.LeftToRight;
            themeFlow.WrapContents = true;
            themeFlow.AutoScroll = false;
            themeFlow.Padding = new Padding(50, 10, 50, 10);
            themeFlow.BackColor = Color.Transparent;

            // Создаем радиокнопки с четкими размерами
            RadioButton rbAnimals = CreateThemeRadioButton("🐾 ЖИВОТНЫЕ", "Животные", true, Color.FromArgb(255, 200, 80));
            RadioButton rbGeometry = CreateThemeRadioButton("🔷 ГЕОМЕТРИЯ", "Геометрические фигуры", false, Color.FromArgb(100, 150, 255));
            RadioButton rbPlants = CreateThemeRadioButton("🌿 РАСТЕНИЯ", "Растения", false, Color.FromArgb(150, 255, 100));

            themeFlow.Controls.Add(rbAnimals);
            themeFlow.Controls.Add(rbGeometry);
            themeFlow.Controls.Add(rbPlants);

            panel.Controls.Add(themeFlow);
            panel.Controls.Add(themeLabel);

            return panel;
        }

        private RadioButton CreateThemeRadioButton(string text, string value, bool isChecked, Color bgColor)
        {
            RadioButton rb = new RadioButton();
            rb.Text = text;
            rb.Font = new Font("Times New Roman", 20, FontStyle.Bold);
            rb.Appearance = Appearance.Normal; // ← КРУЖОЧЕК ВИДЕН!
            rb.Size = new Size(250, 70);
            rb.Margin = new Padding(15);
            rb.TextAlign = ContentAlignment.MiddleCenter;
            rb.Checked = isChecked;
            rb.Tag = value;

            // Подбираем цвет текста под фон
            rb.ForeColor = IsDarkColor(bgColor) ? Color.White : Color.DarkBlue;
            rb.BackColor = bgColor;

            rb.CheckedChanged += (s, e) =>
            {
                if (rb.Checked)
                {
                    selectedTheme = value;
                }
            };

            return rb;
        }

        // Вспомогательная функция: определяет, тёмный ли цвет
        private bool IsDarkColor(Color c)
        {
            double luminance = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255;
            return luminance < 0.5;
        }

        private Panel CreateLevelPanel()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.FromArgb(200, Color.White);
            panel.Margin = new Padding(10);
            panel.Padding = new Padding(10);

            Label levelLabel = new Label();
            levelLabel.Text = "ВЫБЕРИТЕ УРОВЕНЬ СЛОЖНОСТИ:";
            levelLabel.Font = new Font("Times New Roman", 24, FontStyle.Bold);
            levelLabel.ForeColor = Color.DarkBlue;
            levelLabel.TextAlign = ContentAlignment.MiddleCenter;
            levelLabel.Dock = DockStyle.Top;
            levelLabel.Height = 60;
            levelLabel.BackColor = Color.Transparent;

            TableLayoutPanel levelTable = new TableLayoutPanel();
            levelTable.Dock = DockStyle.Fill;
            levelTable.RowCount = 5;
            levelTable.ColumnCount = 1;
            levelTable.BackColor = Color.Transparent;
            levelTable.Padding = new Padding(100, 10, 100, 10);

            // Равномерное распределение высоты для кнопок
            for (int i = 0; i < 5; i++)
            {
                levelTable.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
            }

            // Создаем кнопки уровней
            Button btnEasy = CreateLevelButton("⭐ ЛЕГКИЙ (2×2)", "Легкий", Color.FromArgb(255, 200, 80), Color.DarkBlue);
            Button btnMedium = CreateLevelButton("⭐⭐ СРЕДНИЙ (3×3)", "Средний", Color.FromArgb(255, 170, 50), Color.DarkBlue);
            Button btnHard = CreateLevelButton("⭐⭐⭐ СЛОЖНЫЙ (4×4)", "Сложный", Color.FromArgb(255, 140, 30), Color.White);
            Button btnExpert = CreateLevelButton("⭐⭐⭐⭐ ЭКСПЕРТ (5×5)", "Эксперт", Color.FromArgb(255, 110, 10), Color.White);
            Button btnCustom = CreateLevelButton("🎮 ПОЛЬЗОВАТЕЛЬСКИЙ", "Пользовательский", Color.FromArgb(200, 150, 255), Color.DarkBlue);

            levelTable.Controls.Add(btnEasy, 0, 0);
            levelTable.Controls.Add(btnMedium, 0, 1);
            levelTable.Controls.Add(btnHard, 0, 2);
            levelTable.Controls.Add(btnExpert, 0, 3);
            levelTable.Controls.Add(btnCustom, 0, 4);

            panel.Controls.Add(levelTable);
            panel.Controls.Add(levelLabel);

            return panel;
        }

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
            btn.Dock = DockStyle.Fill;
            btn.Margin = new Padding(10, 5, 10, 5);
            btn.Tag = levelTag;
            btn.Cursor = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleCenter;

            btn.Click += LevelButton_Click;
            return btn;
        }

        private Panel CreateBottomPanel()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.FromArgb(200, Color.LightBlue);
            panel.Margin = new Padding(10);
            panel.Padding = new Padding(20);

            TableLayoutPanel buttonTable = new TableLayoutPanel();
            buttonTable.Dock = DockStyle.Fill;
            buttonTable.RowCount = 1;
            buttonTable.ColumnCount = 3;
            buttonTable.BackColor = Color.Transparent;

            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));

            // Кнопка правил слева
            Button btnRules = new Button();
            btnRules.Text = "📖 ПРАВИЛА ИГРЫ";
            btnRules.Font = new Font("Times New Roman", 16, FontStyle.Bold);
            btnRules.BackColor = Color.LightSteelBlue;
            btnRules.ForeColor = Color.DarkBlue;
            btnRules.FlatStyle = FlatStyle.Flat;
            btnRules.FlatAppearance.BorderSize = 3;
            btnRules.FlatAppearance.BorderColor = Color.DarkBlue;
            btnRules.Dock = DockStyle.Fill;
            btnRules.Margin = new Padding(50, 10, 10, 10);
            btnRules.Cursor = Cursors.Hand;
            btnRules.Click += (s, e) => ShowRules();

            // Пустое место в центре
            Panel spacer = new Panel();
            spacer.Dock = DockStyle.Fill;
            spacer.BackColor = Color.Transparent;

            // Кнопка выхода справа
            Button btnExit = new Button();
            btnExit.Text = "❌ ВЫЙТИ ИЗ ИГРЫ";
            btnExit.Font = new Font("Times New Roman", 16, FontStyle.Bold);
            btnExit.BackColor = Color.IndianRed;
            btnExit.ForeColor = Color.White;
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderSize = 3;
            btnExit.FlatAppearance.BorderColor = Color.DarkRed;
            btnExit.Dock = DockStyle.Fill;
            btnExit.Margin = new Padding(10, 10, 50, 10);
            btnExit.Cursor = Cursors.Hand;
            btnExit.Click += ExitButton_Click;

            buttonTable.Controls.Add(btnRules, 0, 0);
            buttonTable.Controls.Add(spacer, 1, 0);
            buttonTable.Controls.Add(btnExit, 2, 0);

            panel.Controls.Add(buttonTable);
            return panel;
        }

        private void LevelButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string level = btn?.Tag?.ToString();

            if (level == "Пользовательский")
            {
                using (CustomLevelForm customForm = new CustomLevelForm())
                {
                    if (customForm.ShowDialog() == DialogResult.OK)
                    {
                        StartGame(selectedTheme, level, customForm.SelectedRows, customForm.SelectedColumns);
                    }
                }
            }
            else
            {
                StartGame(selectedTheme, level);
            }
        }

        private void StartGame(string theme, string level, int rows = 0, int cols = 0)
        {
            GameForm gameForm = (rows > 0 && cols > 0)
                ? new GameForm(theme, level, rows, cols)
                : new GameForm(theme, level);

            this.Hide();
            gameForm.ShowDialog();
            this.Show();
        }

        private void ShowRules()
        {
            using (RulesForm rules = new RulesForm(this))
            {
                this.Hide();
                rules.ShowDialog();
                this.Show();
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            ExitApplication();
        }

        private void ExitApplication()
        {
            if (MessageBox.Show(
                "Вы действительно хотите выйти из игры?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void MainMenuForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                ExitApplication();
            }
        }
    }
}