namespace StockSDK
{
    public class ItemLine
    {
        public Item Item;
        public int Quantity;

        public ItemLine(Item item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }
    }
}