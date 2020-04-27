using System;

namespace StockSDK
{
    [Serializable]
    public class ItemLine
    {
        public Item Item;
        public int Quantity;

        public ItemLine(Item item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }

        public static string ToStringHeader() => "Qt\tPrice\tProduct";
        
        public override string ToString() => $"{Quantity}\t{Quantity * Item.Price}$\t{Item}";
    }
}