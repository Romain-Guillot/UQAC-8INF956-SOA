using System;


namespace StockSDK
{
    [Serializable]
    public class Item
    {
        public string Name;
        public double Price;

        public Item(string name, double price)
        {
            Name = name;
            Price = price;
        }

        public override string ToString() => $"{Name} ({Price}$)";
    }
}