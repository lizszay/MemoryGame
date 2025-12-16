using MemoryGame.GameLogic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MemoryGame
{
    public partial class CustomLevelForm : BufferedForm
    {
        // === СВОЙСТВА ДЛЯ ДОСТУПА ИЗВНЕ ===
        public int SelectedRows { get; private set; }
        public int SelectedColumns { get; private set; }

        // === КОНСТРУКТОР ===
        public CustomLevelForm()
        {
            // Настройка внешнего вида формы
            ConfigureFormAppearance();

            // Инициализация компонентов из Designer файла
            InitializeComponent();
        }

        // === ОБРАБОТЧИК СОБЫТИЯ КЛИКА ПО КНОПКЕ "НАЧАТЬ" ===
        private void StartButton_Click(object sender, EventArgs e)
        {
            // Сохраняем выбранные пользователем значения
            SelectedRows = (int)rowsNumeric.Value;
            SelectedColumns = (int)columnsNumeric.Value;

            int totalCards = SelectedRows * SelectedColumns;

            // Проверка валидности уровня
            if (!IsValidLevel(SelectedRows, SelectedColumns))
            {
                ShowValidationError(totalCards);
                return;
            }

            // Показываем информацию о создаваемом уровне
            ShowLevelConfirmation(totalCards);
        }

        // === ОБРАБОТЧИК СОБЫТИЯ КНОПКИ "ОТМЕНА" ===
        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // === МЕТОД ДЛЯ ОБНОВЛЕНИЯ ИНФОРМАЦИОННОЙ МЕТКИ ===
        private void UpdateInfoLabel()
        {
            int rows = (int)rowsNumeric.Value;
            int columns = (int)columnsNumeric.Value;
            int totalCards = rows * columns;

            // Формирование информационного текста
            string info = BuildInfoText(totalCards);
            infoLabel.Text = info;
        }

        // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===

        /// <summary>
        /// Настраивает внешний вид формы
        /// </summary>
        private void ConfigureFormAppearance()
        {
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            this.WindowState = FormWindowState.Normal;
            this.BackColor = Color.LightSteelBlue;
            this.Size = new Size(800, 700);
            this.MaximumSize = new Size(800, 700);
            this.MinimumSize = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ControlBox = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Настройка уровня";
        }

        /// <summary>
        /// Определяет количество спецкарт в зависимости от общего числа карт
        /// </summary>
        private int GetSpecialCardsCount(int totalCards)
        {
            return LevelManager.GetSpecialCardsCountForCustom(totalCards);                            // 20+ карт - 2 подсказки + 1 перемешивание
        }

        /// <summary>
        /// Проверяет, является ли уровень валидным
        /// </summary>
        private bool IsValidLevel(int rows, int columns)
        {
            int totalCards = rows * columns;
            if (rows < 1 || rows > 5 || columns < 1 || columns > 5) 
                return false;
            if (totalCards > 25 || totalCards < 4)
                return false;

            int specialCount = GetSpecialCardsCount(totalCards);
            int regularCards = totalCards - specialCount;

            // Минимум 4 обычных карты (2 пары)
            if (regularCards < 4)
                return false;

            // Обычные карты должны быть чётными
            if (regularCards % 2 != 0)
                return false;

            return true;
        }

        /// <summary>
        /// Показывает сообщение об ошибке валидации
        /// </summary>
        private void ShowValidationError(int totalCards)
        {
            string errorMessage = "Невозможно создать уровень!\n\n";

            // Диагностика конкретной ошибки
            if (totalCards < 4)
                errorMessage += "• Минимум 4 карты (2 пары)\n";
            else if (totalCards > 25)
                errorMessage += "• Максимум 25 карт (5×5)\n";
            else
                errorMessage += "• Количество строк и столбцов: от 1 до 5\n";

            // Примеры допустимых конфигураций
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
        }

        /// <summary>
        /// Формирует информационный текст для метки
        /// </summary>
        private string BuildInfoText(int totalCards)
        {
            string info = $"Всего карт: {totalCards}";

            // Добавление предупреждений
            if (totalCards < 2)
            {
                info += " (минимум 2 карты)";
            }
            else
            {
                int specialCards = GetSpecialCardsCount(totalCards);
                if (specialCards < 0)
                    info += " — невозможно создать";
                else
                    info += GetSpecialCardsInfo(specialCards);
            }

            return info;
        }

        /// <summary>
        /// Возвращает строку с информацией о спецкартах
        /// </summary>
        private string GetSpecialCardsInfo(int specialCards)
        {
            switch (specialCards)
            {
                case 0: return " - без спецкарт";
                case 1: return " - 1 подсказка";
                case 2: return " - 2 подсказки";
                default: return " - 2 подсказки + перемешивание";
            }
        }

        /// <summary>
        /// Показывает диалог подтверждения создания уровня
        /// </summary>
        private void ShowLevelConfirmation(int totalCards)
        {
            // Расчет статистики уровня
            int specialCardsCount = GetSpecialCardsCount(totalCards);
            LevelStats stats = CalculateLevelStats(totalCards, specialCardsCount);

            // Формирование сообщения
            string message = BuildConfirmationMessage(stats);

            // Показ диалога подтверждения
            DialogResult confirm = MessageBox.Show(
                message + "\n\nНачать игру?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            // Обработка ответа пользователя
            if (confirm == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// Рассчитывает статистику уровня
        /// </summary>
        private LevelStats CalculateLevelStats(int totalCards, int specialCardsCount)
        {
            LevelStats stats = new LevelStats();
            stats.TotalCards = totalCards;
            stats.SpecialCardsCount = specialCardsCount;

            // Спецкарты — одиночные, каждая занимает 1 клетку
            stats.SpecialCardsTotal = specialCardsCount;

            // Определяем, какие именно спецкарты есть
            if (specialCardsCount == 0)
            {
                stats.SpecialInfo = "нет (все карты обычные)";
            }
            else if (specialCardsCount == 1)
            {
                stats.SpecialInfo = "1 подсказка";
            }
            else if (specialCardsCount == 2)
            {
                stats.SpecialInfo = "1 подсказка + 1 перемешивание";
            }
            else if (specialCardsCount == 3)
            {
                stats.SpecialInfo = "2 подсказки + 1 перемешивание";
            }
            else
            {
                // На случай ошибки (не должно происходить)
                stats.SpecialInfo = $"{specialCardsCount} спецкарт";
            }

            // Обычные карты — только они образуют пары
            int regularCards = totalCards - specialCardsCount;
            stats.RegularPairs = regularCards / 2; // гарантированно чётное число по валидации

            // Общее число пар = только обычные пары!
            // Спецкарты НЕ являются парами и не участвуют в подсчёте пар
            stats.TotalPairs = stats.RegularPairs;

            return stats;
        }

        /// <summary>
        /// Формирует сообщение подтверждения
        /// </summary>
        private string BuildConfirmationMessage(LevelStats stats)
        {
            string message = $"Создаётся уровень:\n\n";
            message += $"• Размер поля: {SelectedRows} × {SelectedColumns}\n";
            message += $"• Всего карт: {stats.TotalCards}\n";
            message += $"• Обычных пар: {stats.RegularPairs}\n";
            message += $"• Спецкарты: {stats.SpecialInfo}\n";
            message += $"• Всего пар на поле: {stats.TotalPairs}";
            return message;
        }

        /// <summary>
        /// Вспомогательный класс для хранения статистики уровня
        /// </summary>
        private class LevelStats
        {
            public int TotalCards { get; set; }
            public int SpecialCardsCount { get; set; }
            public int SpecialCardsTotal { get; set; }
            public string SpecialInfo { get; set; }
            public int RegularPairs { get; set; }
            public int TotalPairs { get; set; }
        }
    }
}