using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MemoryGame.GameLogic
{
    public class GameBoard
    {
        public List<Card> Cards { get; private set; }
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public string Theme { get; private set; }
        public string Level { get; private set; }

        private Random random;

        public GameBoard(int rows, int columns, string theme, string level)
        {
            Rows = rows;
            Columns = columns;
            Theme = theme;
            Level = level;
            Cards = new List<Card>();
            random = new Random();
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            int totalCards = Rows * Columns;
            int specialCount = GetRequiredSpecialCardCount();
            int regularCardsCount = totalCards - specialCount;

            if (regularCardsCount <= 0 || regularCardsCount % 2 != 0)
            {
                throw new ArgumentException(
                    $"Невозможно создать уровень: {totalCards} карт, {specialCount} спецкарт → {regularCardsCount} обычных (должно быть >0 и чётное)");
            }

            // Создаём обычные карты
            int regularPairsNeeded = regularCardsCount / 2;
            List<int> regularCardIds = GetRandomCardIds(regularPairsNeeded);
            List<Card> regularCards = new List<Card>();

            foreach (int id in regularCardIds)
            {
                regularCards.Add(new Card(id, CardType.Regular, Theme));
                regularCards.Add(new Card(id, CardType.Regular, Theme));
            }

            // Создаём спецкарты (уже знаем, сколько нужно)
            List<Card> specialCards = GetSpecialCards(specialCount);

            Cards.AddRange(regularCards);
            Cards.AddRange(specialCards);

            ShuffleCards();
        }

        private List<int> GetRandomCardIds(int count)
        {
            if (count > 11) count = 11; // Максимум 11 различных картинок

            List<int> allIds = Enumerable.Range(1, 11).ToList();
            List<int> selectedIds = new List<int>();

            while (selectedIds.Count < count)
            {
                int randomId = allIds[random.Next(allIds.Count)];
                if (!selectedIds.Contains(randomId))
                {
                    selectedIds.Add(randomId);
                }
            }

            return selectedIds;
        }

        private List<Card> GetSpecialCards(int count)
        {
            List<Card> specialCards = new List<Card>();

            if (count <= 0) return specialCards;

            switch (Level.ToLower())
            {
                case "средний":
                    // Должно быть 1 — это hint
                    specialCards.Add(new Card(0, CardType.Hint));
                    break;

                case "сложный":
                    // 2 карты: hint + shuffle
                    if (count >= 1) specialCards.Add(new Card(0, CardType.Hint));
                    if (count >= 2) specialCards.Add(new Card(0, CardType.Shuffle));
                    break;

                case "эксперт":
                    // 3 карты: 2 hint + 1 shuffle
                    if (count >= 1) specialCards.Add(new Card(0, CardType.Hint));
                    if (count >= 2) specialCards.Add(new Card(0, CardType.Hint));
                    if (count >= 3) specialCards.Add(new Card(0, CardType.Shuffle));
                    break;

                default: // Пользовательский
                    if (count >= 1) specialCards.Add(new Card(0, CardType.Hint));
                    if (count >= 2) specialCards.Add(new Card(0, CardType.Shuffle));
                    if (count >= 3) specialCards.Add(new Card(0, CardType.Hint)); // второй hint
                    break;
            }

            return specialCards;
        }

        private int GetRequiredSpecialCardCount()
        {
            switch (Level.ToLower())
            {
                case "легкий":
                    return 0;
                case "средний":
                    return 1; // 1 hint
                case "сложный":
                    return 2; // 1 hint + 1 shuffle
                case "эксперт":
                    return 3; // 2 hint + 1 shuffle
                default: // Пользовательский
                    int total = Rows * Columns;
                    if (total >= 20) return 3;
                    if (total >= 12) return 2;
                    if (total >= 6) return 1;
                    return 0;
            }
        }

        public void ShuffleCards()
        {
            int n = Cards.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                Card value = Cards[k];
                Cards[k] = Cards[n];
                Cards[n] = value;
            }
        }

        public bool AllCardsMatched()
        {
            return Cards.All(card => card.IsMatched);
        }
    }
}