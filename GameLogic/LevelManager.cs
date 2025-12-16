namespace MemoryGame.GameLogic
{
    public static class LevelManager
    {
        //получить размеры уровня
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
            if (rows < 1 || rows > 5 || columns < 1 || columns > 5)
                return false;
            if (totalCards > 25 || totalCards < 4)
                return false;

            int specialCount = GetSpecialCardsCountForCustom(totalCards);
            int regularCards = totalCards - specialCount;

            if (regularCards < 4 || regularCards % 2 != 0)
                return false;

            return true;
        }

        //метод для определения количества спецкарт
        public static int GetSpecialCardsCountForCustom(int totalCards)
        {
            if (totalCards < 4 || totalCards > 25)
                return -1;

            // Устанавливаем МАКСИМАЛЬНО допустимое число спецкарт по ТЗ
            int maxSpecial;
            if (totalCards <= 9)
                maxSpecial = 1;   // ← ИЗМЕНЕНО: раньше было 0, но тогда 5 карт не работали
            else if (totalCards <= 16)
                maxSpecial = 1;
            else if (totalCards < 25)
                maxSpecial = 2;
            else
                maxSpecial = 3; // total == 25

            // Ограничиваем сверху по правилам игры
            if (maxSpecial > 3) maxSpecial = 3;

            // Перебираем от maxSpecial вниз
            for (int s = maxSpecial; s >= 0; s--)
            {
                int regular = totalCards - s;
                // Должно быть: минимум 4, максимум 22, чётное
                if (regular >= 4 && regular <= 22 && regular % 2 == 0)
                    return s;
            }

            return -1; // невозможно
        }
    }
}