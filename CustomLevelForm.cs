using System;
using System.Drawing;
using System.Windows.Forms;

namespace MemoryGame
{
    public class CustomLevelForm : Form
    {
        public int SelectedRows { get; private set; }
        public int SelectedColumns { get; private set; }

        private NumericUpDown rowsNumeric;
        private NumericUpDown columnsNumeric;

        public CustomLevelForm()
        {
            // Двойная буферизация - предотвращает мерцание
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |  //для снижения мерцания
                         ControlStyles.UserPaint |  //отображение элемента управления выполняет сам элемент, а не операционная система
                         ControlStyles.DoubleBuffer, true); //сначла рисует в буфере памяти,  затем за раз выводится все на экран
            this.DoubleBuffered = true; // Дополнительная двойная буферизация

            // Устанавливаем черный фон для предотвращения видимости рабочего стола
            this.BackColor = Color.Black;

            // Настраиваем форму для поддержки анимации
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            InitializeCustomComponents();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        private void InitializeCustomComponents()
        {
            this.SuspendLayout();

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;

            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.Black;

            // Двойная буферизация для панели
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(mainPanel, true, null);

            try
            {
                if (System.IO.File.Exists("img/ui/background2.jpg"))
                {
                    mainPanel.BackgroundImage = Image.FromFile("img/ui/background2.jpg");
                    mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch { }

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
            tableLayout.BackColor = Color.Black;

            // Выбор строк
            Label rowsLabel = new Label();
            rowsLabel.Text = "Количество строк (2-5):";
            rowsLabel.Font = new Font("Times New Roman", 20, FontStyle.Regular);
            rowsLabel.TextAlign = ContentAlignment.MiddleCenter;
            rowsLabel.Dock = DockStyle.Fill;
            tableLayout.Controls.Add(rowsLabel, 0, 1);
            tableLayout.BackColor = Color.Black;

            rowsNumeric = new NumericUpDown();
            rowsNumeric.Font = new Font("Times New Roman", 20, FontStyle.Regular);
            rowsNumeric.Minimum = 2;
            rowsNumeric.Maximum = 5;
            rowsNumeric.Value = 3;
            rowsNumeric.Dock = DockStyle.Fill;
            rowsNumeric.TextAlign = HorizontalAlignment.Center;
            rowsNumeric.Margin = new Padding(200, 5, 200, 5);
            tableLayout.Controls.Add(rowsNumeric, 0, 2);
            tableLayout.BackColor = Color.Black;

            // Выбор столбцов
            Label columnsLabel = new Label();
            columnsLabel.Text = "Количество столбцов (2-5):";
            columnsLabel.Font = new Font("Times New Roman", 20, FontStyle.Regular);
            columnsLabel.TextAlign = ContentAlignment.MiddleCenter;
            columnsLabel.Dock = DockStyle.Fill;
            tableLayout.Controls.Add(columnsLabel, 0, 3);
            tableLayout.BackColor = Color.Black;

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

            Button startButton = CreateCustomButton("Начать игру", Color.Green);
            startButton.Click += StartButton_Click;

            Button cancelButton = CreateCustomButton("Отмена", Color.Red);
            cancelButton.Click += CancelButton_Click;

            buttonPanel.Controls.Add(startButton);
            buttonPanel.Controls.Add(cancelButton);
            tableLayout.Controls.Add(buttonPanel, 0, 5);

            mainPanel.Controls.Add(tableLayout);
            this.Controls.Add(mainPanel);

            this.ResumeLayout(true);
            this.PerformLayout();
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
            // Сохраняем выбранные значения
            SelectedRows = (int)rowsNumeric.Value;
            SelectedColumns = (int)columnsNumeric.Value;

            // Устанавливаем результат диалога
            this.DialogResult = DialogResult.OK;

            // Закрываем форму
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            // Устанавливаем результат отмены
            this.DialogResult = DialogResult.Cancel;

            // Закрываем форму
            this.Close();
        }
    }
}