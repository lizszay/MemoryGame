using System;
using System.Drawing;
using System.Windows.Forms;

namespace MemoryGame
{
    public partial class CustomLevelForm : Form
    {
        public int SelectedRows { get; private set; }
        public int SelectedColumns { get; private set; }

        private NumericUpDown rowsNumeric;
        private NumericUpDown columnsNumeric;

        public CustomLevelForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.LightBlue;

            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            try
            {
                if (System.IO.File.Exists("img/ui/background.jpg"))
                {
                    mainPanel.BackgroundImage = Image.FromFile("img/ui/background.jpg");
                    mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фона: {ex.Message}");
            }

            TableLayoutPanel tableLayout = new TableLayoutPanel();
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.RowCount = 6;
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 15));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 15));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 15));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 15));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = "ПОЛЬЗОВАТЕЛЬСКИЙ УРОВЕНЬ";
            titleLabel.Font = new Font("Times New Roman", 36, FontStyle.Bold);
            titleLabel.ForeColor = Color.DarkBlue;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Dock = DockStyle.Fill;
            tableLayout.Controls.Add(titleLabel, 0, 0);

            // Выбор строк
            Label rowsLabel = new Label();
            rowsLabel.Text = "Количество строк (2-5):";
            rowsLabel.Font = new Font("Times New Roman", 20, FontStyle.Regular);
            rowsLabel.TextAlign = ContentAlignment.MiddleCenter;
            rowsLabel.Dock = DockStyle.Fill;
            tableLayout.Controls.Add(rowsLabel, 0, 1);

            rowsNumeric = new NumericUpDown();
            rowsNumeric.Font = new Font("Times New Roman", 20, FontStyle.Regular);
            rowsNumeric.Minimum = 2;
            rowsNumeric.Maximum = 5;
            rowsNumeric.Value = 3;
            rowsNumeric.Dock = DockStyle.Fill;
            rowsNumeric.TextAlign = HorizontalAlignment.Center;
            rowsNumeric.Margin = new Padding(200, 5, 200, 5);
            tableLayout.Controls.Add(rowsNumeric, 0, 2);

            // Выбор столбцов
            Label columnsLabel = new Label();
            columnsLabel.Text = "Количество столбцов (2-5):";
            columnsLabel.Font = new Font("Times New Roman", 20, FontStyle.Regular);
            columnsLabel.TextAlign = ContentAlignment.MiddleCenter;
            columnsLabel.Dock = DockStyle.Fill;
            tableLayout.Controls.Add(columnsLabel, 0, 3);

            columnsNumeric = new NumericUpDown();
            columnsNumeric.Font = new Font("Times New Roman", 20, FontStyle.Regular);
            columnsNumeric.Minimum = 2;
            columnsNumeric.Maximum = 5;
            columnsNumeric.Value = 3;
            columnsNumeric.Dock = DockStyle.Fill;
            columnsNumeric.TextAlign = HorizontalAlignment.Center;
            columnsNumeric.Margin = new Padding(200, 5, 200, 5);
            tableLayout.Controls.Add(columnsNumeric, 0, 4);

            // Кнопки
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.LeftToRight;
            buttonPanel.WrapContents = false;
            buttonPanel.Anchor = AnchorStyles.None;

            Button startButton = CreateCustomButton("Начать игру", Color.Green);
            startButton.Click += StartButton_Click;

            Button cancelButton = CreateCustomButton("Отмена", Color.Red);
            cancelButton.Click += CancelButton_Click;

            buttonPanel.Controls.Add(startButton);
            buttonPanel.Controls.Add(cancelButton);
            tableLayout.Controls.Add(buttonPanel, 0, 5);

            mainPanel.Controls.Add(tableLayout);
            this.Controls.Add(mainPanel);
        }

        private Button CreateCustomButton(string text, Color backColor)
        {
            Button button = new Button();
            button.Text = text;
            button.Font = new Font("Times New Roman", 18, FontStyle.Bold);
            button.BackColor = backColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 2;
            button.FlatAppearance.BorderColor = Color.DarkBlue;
            button.Size = new Size(200, 60);
            button.Margin = new Padding(20);
            button.Cursor = Cursors.Hand;
            return button;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            SelectedRows = (int)rowsNumeric.Value;
            SelectedColumns = (int)columnsNumeric.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void CustomLevelForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && this.DialogResult != DialogResult.OK)
            {
                DialogResult result = MessageBox.Show("Вы точно хотите выйти? Настройки не будут сохранены.",
                    "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}