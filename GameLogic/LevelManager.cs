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
            return rows >= 2 && rows <= 5 && columns >= 2 && columns <= 5 && (rows * columns) % 2 == 0;
        }
    }
}