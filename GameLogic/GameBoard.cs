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

            if (regularCardsCount < 4 || regularCardsCount % 2 != 0)
            {
                throw new ArgumentException(
					$"Невозможно создать уровень: {totalCards} карт, " +
					$"{specialCount} спецкарт → {regularCardsCount} обычных. " +
					"Требуется минимум 4 обычные карты (2 пары), и их число должно быть чётным."
				);
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

			// Создаём спецкарты
			List<Card> specialCards = GetSpecialCards(specialCount);

			//добавление элементов коллекции
			Cards.AddRange(regularCards);
			Cards.AddRange(specialCards);

			ShuffleCards();
		}

		private List<int> GetRandomCardIds(int count)
		{
			if (count > 11) count = 11;

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
					specialCards.Add(new Card(0, CardType.Hint));
					break;

				case "сложный":
					if (count >= 1) specialCards.Add(new Card(0, CardType.Hint));
					if (count >= 2) specialCards.Add(new Card(0, CardType.Shuffle));
					break;

				case "эксперт":
					if (count >= 1) specialCards.Add(new Card(0, CardType.Hint));
					if (count >= 2) specialCards.Add(new Card(0, CardType.Hint));
					if (count >= 3) specialCards.Add(new Card(0, CardType.Shuffle));
					break;

				case "пользовательский":
					if (count == 1)
					{
						specialCards.Add(new Card(0, CardType.Hint));
					}
					else if (count == 2)
					{
						specialCards.Add(new Card(0, CardType.Hint));
						specialCards.Add(new Card(0, CardType.Shuffle));
					}
					else if (count == 3)
					{
						specialCards.Add(new Card(0, CardType.Hint));
						specialCards.Add(new Card(0, CardType.Hint));
						specialCards.Add(new Card(0, CardType.Shuffle));
					}
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
					return 1;
				case "сложный":
					return 2;
				case "эксперт":
					return 3;
				default: // Пользовательский
					int total = Rows * Columns;
					int special = LevelManager.GetSpecialCardsCountForCustom(total);
					if (special == -1)
						throw new ArgumentException("Невозможно создать пользовательский уровень");
					return special;
			}
		}

		public void ShuffleCards()
		{
			int n = Cards.Count;
			while (n > 1)
			{
				n--;
				int k = random.Next(n - 1);
				Card value = Cards[k];  //временная перменная
				Cards[k] = Cards[n];    //меняем местами
				Cards[n] = value;   //меняем местами
			}
		}

		public bool AllCardsMatched()
		{
			return Cards.All(card => card.IsMatched);   //для каждой карты присвоено значение не найденной карты
		}
	}
}