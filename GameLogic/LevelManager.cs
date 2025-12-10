namespace MemoryGame.GameLogic
{
    public static class LevelManager
    {
        public static (int rows, int columns) GetLevelDimensions(string level)
        {
            switch (level.ToLower())
            {
                case "легкий": return (2, 2);
                case "средний": return (3, 3);
                case "сложный": return (4, 4);
                case "эксперт": return (5, 5);
                default: return (2, 2);
            }
        }

        public static bool IsValidCustomLevel(int rows, int columns)
        {
            int totalCards = rows * columns;

            // Проверяем диапазоны строк и столбцов
            if (rows < 1 || rows > 5 || columns < 1 || columns > 5)
                return false;

            // Минимум 4 карты (2 пары)
            if (totalCards < 4)
                return false;

            // Максимум 25 карт (5×5)
            if (totalCards > 25)
                return false;

            // Четное количество карт
            if (totalCards % 2 != 0)
                return false;

            return true;
        }

        //метод для определения количества спецкарт
        public static int GetSpecialCardsCountForCustom(int totalCards)
        {
            if (totalCards < 6) return 0;        // До 6 карт - без спецкарт
            if (totalCards < 12) return 1;       // 6-11 карт - 1 подсказка
            if (totalCards < 20) return 2;       // 12-19 карт - 2 подсказки
            return 3;                            // 20+ карт - 2 подсказки + 1 перемешивание
        }
    }
}