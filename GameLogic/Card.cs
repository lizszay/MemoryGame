using System.Drawing;
using System.IO;

namespace MemoryGame.GameLogic
{
    public class Card
    {
        public int Id { get; set; } //ид карты
        public Image Image { get; set; }    //изображение лицевой стороны карты
        public CardType Type { get; set; }  //обычная, подсказка, перемешивыание
        public bool IsFlipped { get; set; } //переернута ли
        public bool IsMatched { get; set; } //найдена ли пара
        public string Theme { get; set; }   //тема карточки

        public Card(int id, CardType type, string theme = "")
        {
            Id = id;
            Type = type;
            Theme = theme;
            IsFlipped = false;  // Изначально все карты рубашкой вверх
            IsMatched = false;  // Изначально ни одна карта не найдена
            LoadImage();        // Загружаем изображение
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

            // карточка создается, но свойство Image остается null
            if (File.Exists(imagePath))
            {
                Image = Image.FromFile(imagePath);
            }
        }

        public Image GetBackImage()
        {
            return Image.FromFile("img/cards/special/back.jpg");
        }
    }

    //enum - тип данных, для создания констант
    public enum CardType
    {
        Regular,
        Hint,
        Shuffle
    }
}