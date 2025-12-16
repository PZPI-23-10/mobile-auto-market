namespace AutoMarket.Messages
{
    // Це "конверт", у якому ми передаватимемо ID машини і новий статус
    public class FavoriteChangeMessage
    {
        public int CarId { get; set; }
        public bool IsFavorite { get; set; }

        public FavoriteChangeMessage(int id, bool isFavorite)
        {
            CarId = id;
            IsFavorite = isFavorite;
        }
    }
}