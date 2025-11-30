using System.Drawing;

namespace MemoryGame.GameLogic
{
    public class Card
    {
        public int Id { get; set; }
        public Image Image { get; set; }
        public CardType Type { get; set; }
        public bool IsFlipped { get; set; }
        public bool IsMatched { get; set; }
        public string Theme { get; set; }

        public Card(int id, CardType type, string theme = "")
        {
            Id = id;
            Type = type;
            Theme = theme;
            IsFlipped = false;
            IsMatched = false;
            LoadImage();
        }

        private void LoadImage()
        {
            string imagePath = "";

            switch (Type)
            {
                case CardType.Regular:
                    imagePath = $"img/cards/regular/{Theme.ToLower()}/{Id}.jpg";
                    break;
                case CardType.Hint:
                    imagePath = "img/cards/special/hint.jpg";
                    break;
                case CardType.Shuffle:
                    imagePath = "img/cards/special/shuffle.jpg";
                    break;
            }

            if (System.IO.File.Exists(imagePath))
            {
                Image = Image.FromFile(imagePath);
            }
        }

        public Image GetBackImage()
        {
            return Image.FromFile("img/cards/special/back.jpg");
        }
    }

    public enum CardType
    {
        Regular,
        Hint,
        Shuffle
    }
}