using System;
using System.Drawing;
using System.Windows.Forms;

namespace MemoryGame
{
    public partial class CustomLevelForm : BufferedForm  // ← partial здесь
    {
        public int SelectedRows { get; private set; }
        public int SelectedColumns { get; private set; }

        public CustomLevelForm()
        {
            // ПЕРЕОПРЕДЕЛЯЕМ настройки из BufferedForm:
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            this.WindowState = FormWindowState.Normal;
            this.BackColor = Color.LightSteelBlue;

            // Фиксируем размер окна
            this.Size = new Size(800, 700);
            this.MaximumSize = new Size(800, 700);
            this.MinimumSize = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Убираем кнопки изменения размера
            this.ControlBox = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.Text = "Настройка уровня";

            // Инициализация UI из Designer
            InitializeComponents(); // ← вызываем метод из Designer файла
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                return cp;
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            // Сохраняем выбранные значения
            SelectedRows = (int)rowsNumeric.Value;  // ← доступ к элементам из Designer
            SelectedColumns = (int)columnsNumeric.Value;

            // Вычисляем общее количество карт один раз
            int totalCards = SelectedRows * SelectedColumns;

            // ВАЛИДАЦИЯ
            if (!IsValidLevel(SelectedRows, SelectedColumns))
            {
                string errorMessage = "Невозможно создать уровень!\n\n";

                if (totalCards < 4)
                    errorMessage += "• Минимум 4 карты (2 пары)\n";
                else if (totalCards > 25)
                    errorMessage += "• Максимум 25 карт (5×5)\n";
                else if (totalCards % 2 != 0)
                    errorMessage += "• Общее количество карт должно быть чётным\n";
                else if (SelectedRows < 1 || SelectedRows > 5 || SelectedColumns < 1 || SelectedColumns > 5)
                    errorMessage += "• Количество строк и столбцов: от 1 до 5\n";

                errorMessage += "\nПримеры допустимых размеров:\n";
                errorMessage += "• 1×4 = 4 карты (2 пары)\n";
                errorMessage += "• 2×2 = 4 карты (2 пары)\n";
                errorMessage += "• 2×3 = 6 карт (3 пары)\n";
                errorMessage += "• 3×4 = 12 карт (6 пар)\n";
                errorMessage += "• 4×5 = 20 карт (10 пар)\n";
                errorMessage += "• 5×4 = 20 карт (10 пар)\n";
                errorMessage += "• 5×5 = 25 карт (нечётное - НЕЛЬЗЯ)\n";

                MessageBox.Show(errorMessage, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Показать информацию о создаваемом уровне
            int specialCardsCount = GetSpecialCardsCount(totalCards);
            int specialCardsTotal = 0;
            string specialInfo = "";

            // Пересчитываем сколько карт занимают спецкарты
            if (specialCardsCount == 1)
            {
                specialCardsTotal = 2; // 1 подсказка = 2 карты
                specialInfo = "1 подсказка";
            }
            else if (specialCardsCount == 2)
            {
                specialCardsTotal = 4; // 2 подсказки = 4 карты
                specialInfo = "2 подсказки";
            }
            else if (specialCardsCount == 3)
            {
                specialCardsTotal = 6; // 2 подсказки + 1 перемешивание = 6 карт
                specialInfo = "2 подсказки + 1 перемешивание";
            }
            else
            {
                specialInfo = "нет (все карты обычные)";
            }

            int regularPairs = (totalCards - specialCardsTotal) / 2;
            int totalPairs = regularPairs +
                (specialCardsCount == 1 ? 1 : 0) + // 1 подсказка = 1 пара
                (specialCardsCount == 2 ? 2 : 0) + // 2 подсказки = 2 пары
                (specialCardsCount == 3 ? 3 : 0);  // 2 подсказки + 1 перемешивание = 3 пары

            string message = $"Создаётся уровень:\n\n";
            message += $"• Размер поля: {SelectedRows} × {SelectedColumns}\n";
            message += $"• Всего карт: {totalCards}\n";
            message += $"• Обычных пар: {regularPairs}\n";
            message += $"• Спецкарты: {specialInfo}\n";
            message += $"• Всего пар на поле: {totalPairs}";

            DialogResult confirm = MessageBox.Show(
                message + "\n\nНачать игру?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void UpdateInfoLabel()
        {
            int rows = (int)rowsNumeric.Value;
            int columns = (int)columnsNumeric.Value;
            int totalCards = rows * columns;

            string info = $"Всего карт: {totalCards}";

            if (totalCards < 2)
            {
                info += " (минимум 2 карты)";
            }
            else if (totalCards % 2 != 0)
            {
                info += " (должно быть чётное число)";
            }
            else
            {
                // Информация о спецкартах
                int specialCards = GetSpecialCardsCount(totalCards);
                if (specialCards == 0)
                    info += " - без спецкарт";
                else if (specialCards == 1)
                    info += " - 1 подсказка";
                else if (specialCards == 2)
                    info += " - 2 подсказки";
                else
                    info += " - 2 подсказки + перемешивание";
            }

            infoLabel.Text = info; // ← доступ к infoLabel из Designer
        }

        // НОВЫЙ метод для определения количества спецкарт
        private int GetSpecialCardsCount(int totalCards)
        {
            if (totalCards < 6) return 0;        // До 6 карт - без спецкарт
            if (totalCards < 12) return 1;       // 6-11 карт - 1 подсказка
            if (totalCards < 20) return 2;       // 12-19 карт - 2 подсказки
            return 3;                            // 20+ карт - 2 подсказки + 1 перемешивание
        }

        // НОВЫЙ метод для проверки валидности
        private bool IsValidLevel(int rows, int columns)
        {
            int totalCards = rows * columns;

            // Проверяем диапазоны
            if (rows < 1 || rows > 5 || columns < 1 || columns > 5)
                return false;

            // Минимум 4 карты (2 пары)
            if (totalCards < 4)
                return false;

            // Максимум 25 карт
            if (totalCards > 25)
                return false;

            // Четное количество
            if (totalCards % 2 != 0)
                return false;

            return true;
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